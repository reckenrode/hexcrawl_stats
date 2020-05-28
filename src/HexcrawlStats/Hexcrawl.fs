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

module HexcrawlStats.Hexcrawl

open FsRandom
open FsRandom.Statistics

type EncounterType = Discovery of DiscoveryType | WanderingMonster of string
and DiscoveryType = Location | Lair of string | Tracks of string

let private creatureWeights =
    let creatures = seq {
        ("Lizardmen", (1, 3));
        ("Tree Trolls", (4, 5));
        ("Adventurers", (6, 6));
        ("Ghouls", (7, 9));
        ("Zombies", (10, 12));
        ("Bat Swarm", (13, 13));
        ("Jungle Bear", (14, 14));
        ("Carrion Crawlers", (15, 15));
        ("Giant Leach", (16, 16));
        ("Orcs", (17, 18));
        ("Wild Boars", (19, 19));
        ("Tyrannosaurus Rex", (20, 20));
    }
    let rangeToWeight (s, e) = float (e - s + 1) / 20.0
    creatures
    |> Seq.map (fun (name, range) -> (name, rangeToWeight range))
    |> Seq.toArray
    |> Array.unzip

let private creature = random {
    let (creatures, weights) = creatureWeights
    return! Array.weightedSampleOne weights creatures
}

let private encounterPercentages =
    let table = Map [
        ("Lizardmen", (30, 50));
        ("Tree Trolls", (40, 50));
        ("Adventurers", (10, 75));
        ("Ghouls", (20, 50));
        ("Zombies", (25, 50));
        ("Bat Swarm", (20, 5));
        ("Jungle Bear", (10, 50));
        ("Carrion Crawlers", (50, 50));
        ("Giant Leach", (0, 0));
        ("Orcs", (25, 50));
        ("Wild Boars", (0, 25));
        ("Tyrannosaurus Rex", (0, 50))
    ]
    fun name -> table |> Map.find name

let private isUnderPerctile percentile = random {
    let! roll = uniformDiscrete (1, 100)
    return roll <= percentile
}

let private isLair = isUnderPerctile
let private isTracks = isUnderPerctile

let private otherEncounter = random {
    let! name = creature
    let (lairPercent, tracksPercent) = name |> encounterPercentages
    let! lairResult = isLair lairPercent
    let! tracksResult = isTracks tracksPercent
    if lairResult then return Discovery (Lair name)
    elif tracksResult then return Discovery (Tracks name)
    else return WanderingMonster name
}

let private (|Location|Other|) = function
    | x when 1 <= x && x <= 10 -> Location
    | _ -> Other

let encounter = random {
    match! uniformDiscrete (1, 20) with
    | Location -> return Discovery Location
    | Other -> return! otherEncounter
}
