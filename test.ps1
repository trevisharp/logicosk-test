cd MadScientist
dotnet build
cd..

./MadScientist/bin/Debug/net8.0/MadScientist.exe new "../repo"

cd TestInspector
dotnet build
cd..

./TestInspector/bin/Debug/net8.0-windows/TestInspector.exe "result.test"