@echo off
cd %~dp0

set /p version= Version? 

echo Packing
dotnet pack -c MerkleTools.nuspec --version-suffix %version%
echo Pushing to nuget.org
set /p apikey= API Key? 
..\.nuget\nuget push MerkleTools.%version%.nupkg -s https://nuget.org/ -ApiKey %apikey%
echo Done!
pause
