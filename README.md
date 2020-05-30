This is a simple program to generate random encounters per the tables at [The Alexandrian][1] and
look at the distribution of results.  What I would like to do is see how many encounters per day
are generated, so I can simplify the procedure or just use the one from Old-School Essentials.

It probably uses way too many iterations, but you can’t say it’s not thorough. The results for this particular table are the following:

|Type             |0   |1   |2  |3|4|5|6|
|-----------------|----|----|---|-|-|-|-|
|lair             |95.9| 4.1|0.1|0|0|0|0|
|location         |82.4|16.5|1.1|0|0|0|0|
|tracks           |93.7| 6.2|0.1|0|0|0|0|
|wandering monster|84.8|14.2|1  |0|0|0|0|

## Building

Just clone the repository and use `dotnet build` to build. To run, you need to `dotnet run` specifying
the HexCrawlStats project or from within its folder.

[1]: https://thealexandrian.net/wordpress/17333/roleplaying-games/hexcrawl-part-4-encounter-tables
