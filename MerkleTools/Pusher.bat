@echo off
cd %~dp0

set /p version= Version? 

echo Packing
dotnet pack --configuration Release
echo Pushing to nuget.org
set /p apikey= API Key? 
..\.nuget\nuget push bin\Release\MerkleTools.%version%.nupkg -s https://nuget.org/ -ApiKey %apikey%
echo Done!
pause
