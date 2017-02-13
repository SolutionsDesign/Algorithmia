@echo off
call vsvars32.bat
echo Building Release build....
MSBuild SD.Tools.Algorithmia.csproj /v:m /p:Configuration=Release
echo Done!
echo Creating NuGetPackage
nuget.exe pack SD.Tools.Algorithmia.nuspec -NoPackageAnalysis -NonInteractive
echo Done!