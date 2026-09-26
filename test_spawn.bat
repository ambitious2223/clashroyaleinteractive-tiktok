@echo off
title ZrdRoyale Spawn Test

:: POST Giant troop spawn payload to the HTTP Spawner API
echo Sending Giant troop spawn payload to http://127.0.0.1:8080/spawn...

Invoke-RestMethod -Uri "http://127.0.0.1:8080/spawn" -Method POST -ContentType "application/json" -Body '{"troop_id": "giant", "level": 13, "x": 9000, "y": 15000, "team": 0}'

echo.
echo Spawn test complete.
echo Check the server console for results.
pause