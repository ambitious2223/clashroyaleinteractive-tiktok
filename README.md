# Clash Royale Interactive (TikTok LIVE)

Turn TikTok LIVE gifts into troops in a live Clash Royale battle. Viewers pick a
side and their gifts spawn troops for that side — viewers vs viewers, with the
streamer joining when they want.

This repository is the **game server**. The TikTok side is the **Tikora** desktop
app (`ambitious2223/tiktok-games-launcher`), which routes gifts to this server.

```
Tikora hub (:27016)  →  clash-royale connector  →  spawn API (:8080)  →  game server (:9339)
```

## Quick start

1. Install the requirements in **[docs/SETUP.md](docs/SETUP.md)**.
2. Run `scripts/start-game.bat` (or let Tikora launch it as the `clash-royale` game).
3. Open Tikora, connect to your LIVE, and assign gifts to the `spawn_troop` effect.
4. Play. See **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** for how it all fits.

## Repository layout

| Path | What it is |
|---|---|
| `src/ClashRoyale/` | the .NET 8 server (ZrdRoyale) |
| `src/ClashRoyale/Core/Network/SpawnHttpServer.cs` | the `:8080` spawn API Tikora calls |
| `scripts/start-game.bat` | one-click launcher (MySQL + ADB + server) |
| `docs/` | setup and architecture notes |
| `config.example.json` | server config template (copy to `config.json`) |

## Credits

A fork of [ZrdRoyale](https://github.com/Zordon1337/ZrdRoyale) by Zordon1337, which
is itself based on the original Clash Royale server work by Incredible / RetroRoyale.
Licensed under GPLv3 (see [LICENSE](LICENSE)).

> Private game servers and LIVE automation are for personal/local use; respect
> Supercell's and TikTok's terms before distributing anything built on this.
