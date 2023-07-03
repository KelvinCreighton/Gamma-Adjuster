@echo off

:: Check if the system is 32 or 64 bit
set framework_directory="C:\Windows\Microsoft.NET\Framework64"
if not exist %framework_directory% (
	set framework_directory="C:\Windows\Microsoft.NET\Framework"
)
if not exist %framework_directory% (
	echo Microsoft.NET Framework was not found
	pause
	goto :EOF
)


:: Find the latest version of the framework that contains the csc.exe file
set filename=csc.exe
set latest_framework=""
for /d %%D in (%framework_directory%\*) do (
    if exist %%D\%filename% (
		set latest_framework=%%D
	)
)
:: Check if the csc.exe file was not found
if %latest_framework%=="" (
	echo The "csc.exe" compiler was not found
	pause
	goto :EOF
)


:: Compile the program
set compiler=%latest_framework%\csc.exe
set sources=manual_gamma_adjust.cs
set output=ManualGammaAdjust.exe

%compiler% /out:%output% %sources%

if not %errorlevel% == 0 pause

:EOF