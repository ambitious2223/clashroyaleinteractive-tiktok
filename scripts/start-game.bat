@echo off
setlocal enabledelayedexpansion
title Clash Royale Interactive - Game Launcher

:: ---------------------------------------------------------------
:: This is the launcher Tikora runs for the "clash-royale" game.
:: It starts MySQL (if needed), the MuMu ADB bridge, and the
:: ZrdRoyale server (HTTP spawn API on :8080 + game server on :9339).
:: ---------------------------------------------------------------

:: Resolve repo paths from this script's location (no hardcoded paths).
set "REPO=%~dp0.."
set "SRV=%REPO%\src\ClashRoyale"
set "ADB=C:\Program Files\Netease\MuMuPlayer\nx_main\adb.exe"
set "DOTNET=C:\Program Files\dotnet\dotnet.exe"

echo ============================================================
echo   Clash Royale Interactive - starting
echo   Server folder: %SRV%
echo ============================================================

:: 1. Make sure MySQL is running (silently ignored if not installed as a service).
sc query MySQL80 >nul 2>&1
if errorlevel 1 (
    net start MySQL80 >nul 2>&1
) else (
    for /f "tokens=3" %%s in ('sc query MySQL80 ^| findstr /i "STATE"') do (
        if /i not "%%s"=="RUNNING" net start MySQL80 >nul 2>&1
    )
)
echo [1/4] MySQL checked.

:: 2. Restart the ADB server so it sees the MuMu emulator.
if exist "%ADB%" (
    "%ADB%" kill-server >nul 2>&1
    timeout /t 2 /nobreak >nul
    "%ADB%" start-server >nul 2>&1
    timeout /t 3 /nobreak >nul

    set "SERIAL="
    for /f "tokens=1" %%D in ('"%ADB%" devices ^| findstr /r "device$"') do (
        if not defined SERIAL set "SERIAL=%%D"
    )
    if defined SERIAL (
        "%ADB%" -s !SERIAL! forward tcp:7555 tcp:7555 >nul 2>&1
        echo [2/4] Emulator found: !SERIAL!  (port 7555 forwarded)
    ) else (
        echo [2/4] WARNING: no emulator found via ADB. Start MuMu Player first.
    )
) else (
    echo [2/4] WARNING: adb.exe not found at "%ADB%".
)

:: 3. Launch the .NET server in its own window.
echo [3/4] Starting ZrdRoyale server...
start "ZrdRoyale Server" cmd /k "cd /d "%SRV%" && "%DOTNET%" run -c Release"

:: 4. Give the server a moment, then show the status endpoints.
timeout /t 5 /nobreak >nul
echo [4/4] Server should be up:
echo        Spawn API : http://127.0.0.1:8080/spawn
echo        Battles   : http://127.0.0.1:8080/battles
echo        Game port : 9339
echo.
echo Tikora's clash-royale connector will talk to :8080 automatically.
echo Leave this window open while streaming.
echo.
pause