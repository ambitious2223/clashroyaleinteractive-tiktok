@echo off
title ZrdRoyale Server & Game Launcher

:: Kill any running dotnet processes
taskkill /F /IM dotnet.exe 2>nul

:: Wait a moment for processes to terminate
timeout /t 2 /nobreak >nul

:: Start the server in a new command window
start "ZrdRoyale Server" cmd /k "cd /d C:\Users\ahmad\OneDrive\Documents\MuMuSharedFolder\ZrdRoyale\src\ClashRoyale && dotnet run -c Release"

:: Wait 5 seconds for server to initialize
timeout /t 5 /nobreak >nul

:: Force-stop Clash Royale on MuMu emulator
& "C:\Program Files\Netease\MuMuPlayer\nx_main\adb.exe" -s 127.0.0.1:5557 shell am force-stop com.supercell.clashroyale

:: Launch Clash Royale game
& "C:\Program Files\Netease\MuMuPlayer\nx_main\adb.exe" -s 127.0.0.1:5557 shell am start -n com.supercell.clashroyale/.GameApp

echo.
echo Server and game launch sequence complete.
echo Please wait for the home lobby to load.
pause