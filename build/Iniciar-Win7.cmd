@echo off
setlocal enableextensions
cd /d "%~dp0"

set "EXE=NetworkScanner.exe"
set "MISSING="

if not exist "%EXE%" (
  echo ERRO: %EXE% nao foi encontrado nesta pasta.
  echo Mantenha este .cmd na MESMA pasta do NetworkScanner.exe.
  echo.
  pause
  exit /b 1
)

rem --- So valida no Windows 7 (versao 6.1). Em outros Windows, apenas executa. ---
ver | findstr /c:"6.1." >nul
if errorlevel 1 goto :launch

echo ============================================================
echo  Network Scanner - checagem de pre-requisitos do Windows 7
echo ============================================================
echo.

rem --- Pasta das DLLs de sistema x86 (SysWOW64 no Win7 64-bit, System32 no 32-bit) ---
set "SYSX86=%SystemRoot%\System32"
if exist "%SystemRoot%\SysWOW64\kernel32.dll" set "SYSX86=%SystemRoot%\SysWOW64"

call :checkdll ucrtbase.dll     "Universal C Runtime (UCRT)"
call :checkdll vcruntime140.dll "Visual C++ 2015-2019 Redistributable x86"
call :checkdll msvcp140.dll     "Visual C++ 2015-2019 Redistributable x86"

echo.
if defined MISSING goto :fail

echo Pre-requisitos OK. Iniciando...
echo.
goto :launch

:checkdll
if exist "%SYSX86%\%~1" (
  echo   [ OK ]      %~1
  goto :eof
)
echo   [ FALTA ]   %~1  -  %~2
set "MISSING=1"
goto :eof

:fail
echo ------------------------------------------------------------
echo  Faltam componentes para rodar no Windows 7. Instale:
echo.
echo  1. Visual C++ 2015-2019 Redistributable x86
echo     https://aka.ms/vs/16/release/vc_redist.x86.exe
echo     Instala UCRT + vcruntime140 + msvcp140.
echo.
echo  2. Se o instalador acima recusar rodar, ou se mesmo com
echo     tudo OK o app abrir e fechar, instale a atualizacao SHA-2:
echo     KB4474419 - https://www.catalog.update.microsoft.com/
echo     Procure por KB4474419 e baixe a versao x86 do Windows 7.
echo.
echo  Requer tambem Windows 7 SP1 com Windows Update em dia.
echo ------------------------------------------------------------
echo.
pause
exit /b 1

:launch
"%EXE%" %*
exit /b %errorlevel%
