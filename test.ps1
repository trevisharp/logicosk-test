cd MadScientist
dotnet build
cd..

./MadScientist/bin/Debug/net8.0/MadScientist.exe new "../repo"
del current.key
del result.test