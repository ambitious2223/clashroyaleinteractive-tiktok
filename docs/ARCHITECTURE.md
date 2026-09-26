# Clash Royale Interactive — Architecture

## Pieces

| Piece | Repo / location | Role |
|---|---|---|
| **Tikora** | `ambitious2223/tiktok-games-launcher` | Windows app: connects to TikTok LIVE, holds the gift catalog, routes gifts to per-game **effects** over a local WebSocket hub. |
| **Connector** | inside Tikora (`electron/integrations/clash-royale.cjs`) | The `clash-royale` game client. Authenticates to Tikora's hub, keeps the viewer→team roster, and calls the spawn API. |
| **Server** | this repo (ZrdRoyale, .NET 8) | Runs the Clash Royale private server (`:9339`) and an HTTP spawn API (`:8080`). |
| **Client** | Clash Royale 1.9.3 on MuMu | The game the streamer/players watch, redirected to this server. |

## Event flow

```
TikFinity or native TikTok  →  Tikora relay (:27016)
        gift/tier/min-coins mapping
        → effect { effect:"spawn_troop", payload:{ troop_id, lane } }
        → connector (knows the sender's team)
        → POST http://127.0.0.1:8080/spawn { troop_id, x, y, team, level }
        → injected into the live battle
```

Effects and gift names are **never hardcoded** — the streamer creates gift → effect
mappings in Tikora's Hub UI. The connector only declares which effects it
understands.

## Teams (viewers vs viewers)

Most of the time it is **viewers against viewers**; sometimes the streamer plays
against everyone.

- The connector keeps `username → team` (team `0` = Red, team `1` = Blue).
- Viewers join a side (chat command / first gift / Tikora UI); the streamer is
  just another participant.
- Each gift spawns for the sender's team via the `team` field.

## Spawn API (this repo)

`src/ClashRoyale/Core/Network/SpawnHttpServer.cs` on `http://127.0.0.1:8080/`:

| Method | Path | Purpose |
|---|---|---|
| `POST` | `/spawn` | spawn a troop/spell/building into the live battle |
| `GET` | `/battles` | list active battles |
| `GET` | `/cards` | troop catalog (`troop_id` → class/instance ids) |
| `GET` | `/config` | server battle settings |

`POST /spawn` body:

```json
{
  "troop_id": "giant",
  "lane": "left",
  "team": 0,
  "level": 13,
  "x": 3,
  "y": 15
}
```

`troop_id` is preferred; `class_id` + `instance_id` may be sent instead. `team`
selects the side (0/1). The battle must be live with both sides present.

## Current status / next steps

- [x] Server runs, MySQL loads, spawn API listening.
- [ ] Login handshake cleanup (ServerHello) — see `docs/SETUP.md`.
- [ ] Persistent battle so `/spawn` always has a target.
- [ ] Connector + viewer roster in Tikora.
- [ ] Live end-to-end test.