@echo off
pushd WpfLocalization
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" /p:Configuration=Release /p:RunCodeAnalysis=Never
..\packages\NuGet.CommandLine.4.4.1\tools\NuGet.exe pack -OutputDirectory ..\Build -properties Configuration=Release
popd