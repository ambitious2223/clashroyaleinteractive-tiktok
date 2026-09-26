# Clash Royale Interactive — Setup

This repo is the **game server** half of the Clash Royale interactive stream. The
TikTok side lives in the Tikora app; Tikora sends "spawn troop" effects here.

```
Tikora (hub :27016)  →  connector  →  ZrdRoyale Spawn API (:8080)  →  game server (:9339)  ←  CR client (MuMu)
```

## Requirements

| Item | Where |
|---|---|
| .NET SDK 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| MySQL 8 (service `MySQL80`) | run once: `net start MySQL80` |
| MuMu Player + its `adb.exe` | `C:\Program Files\Netease\MuMuPlayer\nx_main\adb.exe` |
| Clash Royale **1.9.3** client | this repo is built for CR 1.9.3 only |
| Tikora app | runs the hub and the `clash-royale` connector |

## 1. Database

Create the database (the server creates its tables on first start):

```sql
CREATE DATABASE IF NOT EXISTS rrdb;
```

## 2. Server config

Copy `src/ClashRoyale/config.example.json` to `src/ClashRoyale/config.json` and
adjust if needed. Defaults assume MySQL `root` with an empty password on
`127.0.0.1`, database `rrdb`, game port `9339`.

> `config.json` is **not** committed (it is in `.gitignore`). Only the
> `config.example.json` template is.

## 3. Point the game at this server (MuMu emulator)

The client must resolve Supercell's game host to this PC. On a **rooted MuMu**
instance, add this line to the emulator's `/system/etc/hosts`:

```
10.0.2.2   game.clashroyaleapp.com
```

`10.0.2.2` is the emulator's alias for the host PC. A patched 1.9.3 APK that
hardcodes the server IP works too.

## 4. Run it

Double-click `scripts/start-game.bat` (or let Tikora launch it — Tikora's
`clash-royale` game points at this file). It starts MySQL, the ADB bridge, and
the server.

Check it is alive:

- `http://127.0.0.1:8080/cards` — the troop catalog the connector uses
- `http://127.0.0.1:8080/battles` — active battles (a battle must be running to spawn)

## 5. Test a spawn without a stream

> **A battle needs two participants.** ZrdRoyale is a 1v1 server — it has no
> built-in bot opponent, so `/spawn` reports `No active battles` until two
> accounts are actually in a match. The usual setup is **two Clash Royale
> clients** (a second MuMu instance, or two accounts), one on each side. The
> two sides are the "Red" and "Blue" captains; viewers' gifts spawn for whichever
> side the gifter joined.

1. Start a battle in the game client (solo vs bot or vs another player).
2. Confirm it appears at `http://127.0.0.1:8080/battles`.
3. Run `test_spawn.bat`, or:

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:8080/spawn" -Method POST -ContentType "application/json" -Body '{"troop_id":"giant","lane":"left","team":0}'
```

A troop should appear on that side of the battlefield.

## Troubleshooting

| Symptom | Fix |
|---|---|
| `MysqlConnection for players failed` | MySQL not running, or `rrdb`/credentials wrong in `config.json` |
| Client stuck on loading | wrong APK version (must be 1.9.3), or hosts redirect not applied |
| `/spawn` says `No active battles` | start a battle first — the API only injects into a live battle |
| `/spawn` says `Battle not ready (need 2 players)` | the battle needs both sides present |
| Only one side spawns | pass `team` (0 or 1) in the payload |