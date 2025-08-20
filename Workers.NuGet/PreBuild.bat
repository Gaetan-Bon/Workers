REM ----
REM Init
REM ----

set ProjectDir=%~1

REM --------------
REM Init Variables
REM --------------

set "ExternalFiles=%ProjectDir%..\ExternalFiles\"
set "Scripts=%ProjectDir%..\Scripts\"

REM Iterate through project dir and subdirs
for /R "%ProjectDir%" %%g in (*.tt) do (

	REM Transform .tt file
	echo "Transform : %%g"
	"%ExternalFiles%TextTransform.exe" "%%g"

	REM Place batch runtime to current iteration file dir
	cd /D "%%~dpg"

	REM Iterate to current directory and subdirs
	for /R %%h in (*.*) do (
		REM Skip *.tt files
		if not "%%~xh"==".tt" (

			REM Trim files with the same name of the .tt file
			if "%%~ng"=="%%~nh" (
				echo "Trim : %%h"
				powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& '%Scripts%TrimFile.ps1' '%%h'"
			)
		)
	)
)