@echo off
setlocal
title Tikora Royale - Server + Emulator bridge

:: ==================================================================
::  TIKORA ROYALE
::  Interactive Clash Royale for TikTok LIVE, driven by the Tikora hub.
::
::  Starts:
::    * the game server   (TCP 9339, UDP battle 9449)
::    * the Control API   (http://localhost:9350 - POST /api/spawn)
::    * ADB reverse so the game client on MuMu reaches this PC
::
::  Everything lives under C:\dev\tikora-royale.
:: ==================================================================

set "ROOT=%~dp0.."
set "APP=%ROOT%\engine\server\app"
set "NODE=%ROOT%\engine\server\node\node.exe"
set "ADB=C:\Program Files\Netease\MuMuPlayer\nx_main\adb.exe"
set "DEV=127.0.0.1:16384"

echo.
echo   ==========================================
echo     TIKORA ROYALE
echo   ==========================================

echo [1/3] ADB reverse (game device -^> this PC)...
"%ADB%" -s %DEV% reverse tcp:9339 tcp:9339 >nul 2>&1
"%ADB%" -s %DEV% reverse tcp:9350 tcp:9350 >nul 2>&1

echo [2/3] Starting Tikora Royale server...
start "Tikora Royale Server" cmd /k "cd /d "%APP%" && "%NODE%" ."

echo [3/3] Waiting, then opening the game on the emulator...
timeout /t 12 /nobreak >nul
"%ADB%" -s %DEV% shell am force-stop com.greedycell.astralroyale >nul 2>&1
"%ADB%" -s %DEV% shell monkey -p com.greedycell.astralroyale -c android.intent.category.LAUNCHER 1 >nul 2>&1

echo.
echo   Control API : http://localhost:9350
echo     GET  /api/health   /api/cards   /api/battles
echo     POST /api/spawn    (needs a live battle)
echo   Game TCP 9339 ^| Battle UDP 9449
echo.
echo   Open Tikora, connect LIVE, and map gifts to troop effects.
echo.
pause
