@echo off
setlocal enabledelayedexpansion
title Clash Royale Interactive - Server

:: ---------------------------------------------------------------
:: Tikora runs this file as the "clash-royale" game.
:: It starts MySQL (if needed) and the ZrdRoyale server:
::   game port  : 9339   (the Clash Royale client connects here)
::   spawn API  : 8080   (Tikora's connector posts troop spawns here)
:: Start the game client in MuMu yourself (with its hosts file pointing at
:: this PC), then start a battle so spawns have somewhere to land.
:: ---------------------------------------------------------------

set "REPO=%~dp0.."
set "SRV=%REPO%\src\ClashRoyale"
set "DOTNET=C:\Program Files\dotnet\dotnet.exe"

echo ============================================================
echo   Clash Royale Interactive - starting server
echo   %SRV%
echo ============================================================

:: 1. Make sure MySQL is running (ignore if not installed as a service).
sc query MySQL80 >nul 2>&1
if errorlevel 1 (
    net start MySQL80 >nul 2>&1
) else (
    for /f "tokens=3" %%s in ('sc query MySQL80 ^| findstr /i "STATE"') do (
        if /i not "%%s"=="RUNNING" net start MySQL80 >nul 2>&1
    )
)
echo [1/2] MySQL checked.

:: 2. Start the .NET server in its own window.
echo [2/2] Starting ZrdRoyale server...
start "ZrdRoyale Server" cmd /k "cd /d "%SRV%" && "%DOTNET%" run -c Release"

timeout /t 4 /nobreak >nul
echo.
echo Server starting:
echo   Spawn API : http://127.0.0.1:8080/spawn
echo   Battles   : http://127.0.0.1:8080/battles
echo   Game port : 9339
echo.
echo Leave this window open while streaming.
pause