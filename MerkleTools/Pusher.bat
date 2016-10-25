@echo off
cd %~dp0

set /p version= Version? 

echo Packing
dotnet pack --version-suffix %version%
echo Pushing to nuget.org
set /p apikey= API Key? 
..\.nuget\nuget push bin\MerkleTools.nuspec\MerkleTools.1.0.0-%version%.nupkg -s https://nuget.org/ -ApiKey %apikey%
echo Done!
pause
