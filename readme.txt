To compile and run search engine (on MAC OSX).

1) have the dotnet framework installed version 7.0 can download at: 
https://dotnet.microsoft.com/en-us/download

2) from within path/to/SearchEngine run 
$ dotnet build SearchEngine.sln

This will build executables for the parser, indexer and searcher all within their respective folders.
To references code the .cs files within each folder contains my working. 

3) to run the seacrher:
$ cd Searcher/bin/Debug/net7.0
$ ./Searcher < in.txt > out.txt

searcher needs the index, unique terms and doc IDs from the indexer to operate

to run parser follow step 3 for parser 
(will need wsj.xml in the parser net7.0 folder to operate)
