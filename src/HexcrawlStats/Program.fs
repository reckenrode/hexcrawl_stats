//   HexcrawlStats
//   Copyright © 2020 Randy Eckenrode
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

module HexcrawlStats.Main

open Deedle
open FsRandom
open Hexcrawl
open System

let rollsForDay = List.randomCreate 6 Hexcrawl.maybeEncounter

let isExploring x = 2 <= x && x <= 4

let cpus = System.Environment.ProcessorCount
let iterations = 1_000_000

let generateValues t =
    let inline filterAndPrepareDF (row, results) =
        let row = (t <<< 28) + row
        results
        |> Seq.mapi (fun idx  x ->
            match x with
            | Some (WanderingMonster _) -> ((row, idx), "Type", "wandering monster")
            | Some (Discovery (Lair _)) when isExploring idx -> ((row, idx), "Type", "lair")
            | Some (Discovery (Tracks _)) when isExploring idx -> ((row, idx), "Type", "tracks")
            | Some (Discovery Location) when isExploring idx -> ((row, idx), "Type", "location")
            | _ -> ((row, idx), "Type", "no encounter"))
    async {
        return Utility.createRandomState ()
            |> Seq.ofRandom rollsForDay
            |> Seq.take iterations
            |> Seq.indexed
            |> Seq.collect filterAndPrepareDF
            |> Frame.ofValues
    }

let getBreakdown column =
    column
    |> Series.groupBy (fun _ t -> t)
    |> Series.map (fun _ t ->
        Math.Round ((t |> Stats.count |> float) * 100.0 / (cpus * iterations |> float), 1))
    :> ISeries<obj>

let getRawData () =
    Array.init cpus generateValues
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Frame.mergeAll

let calculateFrequencyPerDay data =
    let cols = data |> Frame.cols
    let breakdowns = cols |> Series.values |> Seq.map getBreakdown
    Frame (cols |> Series.keys, breakdowns)
    |> Frame.dropCol "no encounter"
    |> Frame.transpose
    |> Frame.fillMissingWith 0.0
    |> Frame.sortColsByKey
    |> Frame.sortRowsByKey

[<EntryPoint>]
let main argv =
    let data =
        getRawData ()
        |> Frame.pivotTable
            (fun (day, _) _ -> day)
            (fun _ r -> r.GetAs<string> "Type")
            Frame.countRows
        |> Frame.fillMissingWith 0.0
        |> calculateFrequencyPerDay
    data.Print ()
    0 // return an integer exit code
