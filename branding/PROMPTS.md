# Tikora Royale — Branding Asset Prompts

Copy each prompt into your image generator (Midjourney / DALL·E / SDXL / Ideogram).
Generate art **without baked-in text** where noted, then add the words in a design
tool (image models render text unreliably).

---

## Style bible (paste this at the start of every prompt)

> Modern mobile-game / esports branding for "TIKORA ROYALE", a TikTok LIVE
> interactive Clash-Royale-style card battler. Bold flat vector illustration,
> thick outlines, high contrast, saturated crimson-vs-cyan palette
> (Red #E23B3B, Blue #2FA4E7), gold accents #F5C451, deep navy background #10131C.
> Clean, premium, app-store quality, centered composition, generous negative space,
> crisp edges, no photorealism, no lens flare, no watermark.
> Do NOT include any existing game characters, crowns of known franchises, or logos.

Negative prompt (for SDXL/Midjourney):
`photo, 3d render, gritty, realistic face, text artifacts, gibberish text, watermark, signature, clutter, low contrast, blurry, trademarked characters`

---

## 1. Master app icon  — `tikora-royale-icon-512.png`
**512×512 PNG, square, full-bleed, no text.**
Used for: hub game tile + source for every Android launcher size.

> App icon for a mobile game called TIKORA ROYALE. A stylized golden crown made of
> two crossed swords over a shield split crimson and cyan, glowing rim light,
> deep navy circular backdrop, bold flat vector, thick outline, centered, app-store
> icon, high contrast, no text.

## 2. Android adaptive icon foreground — `ic_launcher_foreground.png`
**432×432 PNG, transparent background, artwork inside the middle 288×288 safe zone.**
Used for: modern launcher (adaptive) icon.

> Flat vector game mascot symbol only — a bold golden crowned-sword shield emblem,
> crimson and cyan split, thick outline, no background, no text, centered, leaves
> 20% empty margin on all sides, transparent background.

## 3. Android adaptive icon background — `ic_launcher_background.png`
**432×432 PNG, full-bleed, no subjects.**
> Deep navy radial-gradient background with subtle diagonal crimson and cyan
> energy streaks, flat, clean, no text, no objects.

## 4. Classic launcher icons — `ic_launcher_*.png`
**Downscale the master (Prompt 1) to:** 36, 48, 72, 96, 144, 192 px square.
(No new generation needed — use the 512 master.)

---

## 5. In-game inbox / news banner — `cover.png`
**1024×512 PNG (wide), text left, art right, no critical art in the middle 60%.**
Used for: the news card shown inside the game (`assets.asure.live/royale/cover.png` slot).

> Wide banner for a TikTok LIVE interactive game "TIKORA ROYALE". Left third left
> intentionally clean for text overlay. Right side: two squads of flat vector
> fantasy troops charging at each other, one crimson team and one cyan team,
> golden arena towers in the background, energetic diagonal composition, deep navy
> sky with sparks, bold flat vector, esports key art, no text.

## 6. In-game inbox title badge — `badge_title.png`
**512×160 PNG, transparent, no text.**
> Decorative esports banner ribbon in gold and navy, flat vector, sharp ends,
> empty center panel clean for text, subtle bevel, no text.

---

## 7. Stream title / lower-third banner — `stream_lower_third.png`
**1920×360 PNG, transparent, text area center, safe margins.**
Used for: OBS overlay during the stream.

> Esports lower-third overlay banner, navy glass panel with gold beveled frame and
> crimson/cyan accent strips on each end, flat vector, clean empty center for a
> title, subtle glow, transparent background, no text.

## 8. Red team banner — `team_red.png`
**640×160 PNG, transparent, no text.**
> Solid crimson esports team plate, angular shield shape, gold trim, flat vector,
> clean empty center for text, transparent background.

## 9. Blue team banner — `team_blue.png`
**640×160 PNG, transparent, no text.**
> Solid cyan esports team plate, angular shield shape, gold trim, flat vector,
> clean empty center for text, transparent background.

## 10. Gift alert popup frame — `gift_alert_frame.png`
**600×600 PNG, transparent, open center hole ~360×360.**
Used for: gift/follow alert cards on the overlay.

> Ornate fantasy gift-alert popup frame, gold filigree with crimson and cyan
> gems, flat vector, thick outline, wide transparent center opening, no text.

## 11. "Join a side" instruction card — `join_side_1080.png`
**1080×1080 PNG, TikTok/Live friendly, text areas left clean.**
> Square social card: a crimson knight emblem on one side and a cyan archer emblem
> on the other, split by a golden lightning bolt, deep navy background, bold flat
> vector, empty top and bottom strips for text, no text.

## 12. Vertical livestream cover — `tiktok_cover_1080x1920.png`
**1080×1920 PNG, top third clean for title.**
> Vertical TikTok LIVE cover for an interactive game night. Top third left clean
> for text; bottom two-thirds show an epic flat-vector arena battle with two armies,
> crimson vs cyan, gold crown floating center, sparks and energy, esports poster
> style, deep navy, no text.

## 13. Logo lockup — `logo_lockup.png`
**1600×400 PNG, transparent, no text** (add "TIKORA ROYALE" in a font).
> Esports logo mark: a gold crowned shield with crossed swords, crimson and cyan
> split, bold flat vector, thick outline, transparent background, no text, centered.

## 14. Clan badge (optional) — `clan_badge.png`
**128×128 PNG.**
> Circular clan badge, gold crown and crossed swords on crimson-to-cyan gradient,
> flat vector, thick outline, no text.

---

## Where each file goes (so nothing old shows)

| File | Destination |
|---|---|
| `tikora-royale-icon-512.png` | Tikora hub game icon (Game Store / Game Hub tile) |
| `ic_launcher_*.png`, adaptive pair | repackage into the game client APK (`res/drawable-*/ic_launcher.png`, `res/mipmap-anydpi-v26/`) |
| `cover.png` | replace the in-game news banner (served locally, see note) |
| `stream_*`, `team_*`, `gift_alert_frame`, `join_side_1080`, `tiktok_cover` | OBS / TikTok overlay assets |

## Text that must be replaced (not images)
These are current vendor strings shown in-game; I can swap them once you confirm
your wording:
- Clan name → `Tikora Royale`
- Clan description → e.g. `Official Tikora Royale clan`
- In-game news card title → `Tikora Royale`
- In-game news card text → e.g. `Join the Tikora Royale stream — send a gift to spawn troops!`
- News card button / link → e.g. `Join` / your TikTok or Discord URL
