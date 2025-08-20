set ProjectDir=%~1

set "BatchDirectory=%~dp0"
set "ExternalFiles=%ProjectDir%../ExternalFiles/"
set "Packages=%ProjectDir%../Packages/"

mkdir %Packages%

"%ExternalFiles%nuget.exe" ^
	pack ^
	"%BatchDirectory%NuGet.nuspec" ^
	-OutputDirectory "%Packages%"