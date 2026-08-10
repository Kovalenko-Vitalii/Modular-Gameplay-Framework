# Modular Gameplay Framework

A collection of reusable, self-contained gameplay systems for Unity — a first-person character controller, a global tick system, and a full audio stack (footsteps, ambience, occlusion, surface detection). Each system is decoupled and can be dropped into a new project independently.

## Features

### Character Controller
A first-person controller built on Unity's built-in `CharacterController` and the new Input System.

- Walk, sprint, crouch, and backward-movement speeds, each with independent acceleration/deceleration smoothing
- Jump with coyote time and input buffering, plus a jump cooldown
- Crouch with a smooth height/center transition and an obstruction check (capsule overlap) before standing back up
- Slope handling: walkable-slope limit with a small tolerance to avoid edge jitter, sliding down slopes steeper than the limit, and separate "wall-like" slope detection for near-vertical surfaces
- Ground detection via a sphere cast plus a 5-point ray "ring" for reliable edge/step detection
- Distinct air-movement rules (acceleration, max air speed, wall detachment) so airborne control feels different from grounded control

### Global Tick System
A lightweight `IPlayerTick` / `IPlayerLateTick` overlay on top of Unity's `Update`/`LateUpdate`. Systems register themselves instead of running their own update loops, which makes it trivial to pause all player-related systems at once (`PlayerTickSystem.isTicking`) without touching `Time.timeScale`.

### Surface Identification & Detection
A data-driven way to describe surfaces (metal, wood, grass, etc.) and the audio/VFX tied to each one.

- `SurfaceDatabase` maps a `SurfaceType` to a `SurfaceEntry` (footstep clips, jump/land clips, impact clips, impact particle, decal)
- `SurfaceResolver` raycasts to identify the surface under a point, including Terrain support (resolves the dominant terrain layer via the alphamap and maps it to a surface type)
- Any other system (footsteps, impacts, VFX) can resolve a surface through one shared API

### Footstep Player
Drives footstep audio from actual distance traveled rather than a fixed timer, so step rate follows movement speed. Also handles landing impacts (scaled by landing speed) and jump-start sounds, all resolved through the surface system, with per-clip pitch/volume variation to avoid repetition.

### Ambient Sound System
A layered ambience mixer (`AmbientManager`) that blends a default world profile, global states (e.g. rain), and local `AmbientZone`s.

- Zones expose an `AmbientProfile` (ScriptableObject) with local layer volumes and optional global-layer limiters
- Zone influence fades smoothly with distance (`blendDistance`) as the listener enters/exits a trigger
- All active layer weights are resolved into targets each frame and faded in with `Mathf.MoveTowards`

### Audio Occlusion System
A batched raycast occlusion system for `AudioSource`s. Spreads checks across multiple frames (`maxChecksPerTick`) to stay cheap with many sources, and supports "soft" occlusion — sampling several rays around the source/listener to produce a partial (0–1) occlusion value instead of a binary blocked/unblocked result. Occluded sources are attenuated in volume and low-pass filtered.

### Sound Manager
A persistent singleton for mixer-routed volume control (Master / UI / Subtitle / World), with linear-to-dB conversion, `PlayerPrefs` persistence, and helpers for 2D UI/subtitle one-shots and spatialized 3D one-shots (`PlayWorldOneShot` spawns and auto-cleans up a temporary `AudioSource`).

### Misc Audio Utilities
- `AudioClipBank` — a serializable clip pool with random-without-immediate-repeat selection
- `AudioSpline` — snaps a transform to the nearest point on a spline relative to the listener, useful for ambient sources that should travel along a path (e.g. a river or road)

## Tech Stack
- **C#**
- **Unity** (Input System, Splines package)

## Requirements
- Unity 2022.3 LTS or newer (recommended)
- Input System package
- Splines package (only required if using `AudioSpline`)

## Installation
Clone the repository and open it in Unity, or copy the folders you need into an existing project:

```bash
git clone https://github.com/Kovalenko-Vitalii/Modular-Gameplay-Framework.git
```

## Usage Notes
- The character controller expects a `Transform` reference for camera-relative movement (`orientation`); if left unassigned it falls back to `Camera.main`.
- Systems that tick every frame (footsteps, camera bob) implement `IPlayerTick`/`IPlayerLateTick` and register with `PlayerTickSystem` in `OnEnable`/`OnDisable` — no manual wiring needed beyond having a `PlayerTickSystem` in the scene.
- Ambient zones require a trigger collider and the player object to be tagged `Player`.

## License
Add your license of choice here (e.g. MIT).
