@echo off
pushd.
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsMsBuildCmd.bat"
popd
echo Building Release build....
MSBuild SD.Tools.Algorithmia.csproj /v:m /p:Configuration=Release
echo Done!
echo Creating NuGetPackage
nuget.exe pack SD.Tools.Algorithmia.nuspec -NoPackageAnalysis -NonInteractive
echo Done!