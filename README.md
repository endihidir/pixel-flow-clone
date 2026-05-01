# PixelFlow

A Unity 3D mobile puzzle game where colored "pig" units orbit around a pixel grid, scanning each row and column for matching pixels and firing balls to clear them. PixelFlow combines line-based shooting mechanics with a tray-based unit management loop, built on a clean MVP architecture and an extensive custom level pipeline.

---

## 🎮 Overview

The player taps a pig waiting in one of the lanes around the board. The pig jumps to a launch slot, then travels counter-clockwise around the perimeter of the pixel grid. At every grid line it crosses, it scans inward — if the closest occupied pixel matches its color, the pig fires a projectile and clears that pixel. When ammo runs out, the pig is released; otherwise, after completing the lap, it lands in the unit slot tray to be tapped again later.

A level is **won** when the pixel grid is fully cleared and **lost** when the unit slot tray fills up before the level is solved.

---

## 🕹️ Gameplay Loop

| Phase | What happens |
|---|---|
| **Tap a lane front pig** | Front pig is removed from its lane, the rest shift forward, and the lane is briefly locked during the slide animation. |
| **Launch** | The pig jumps onto the launch node on the grid perimeter and orients itself to the path direction. |
| **Orbit** | The pig follows pre-generated path nodes around the grid (with smooth corner curves driven by quadratic Bezier interpolation). |
| **Trigger nodes** | Each row/column produces a trigger node on the path. When the pig hits one, the line is scanned for the first occupied pixel. |
| **Fire** | If the closest pixel matches the pig's color, the pig aims inward, spawns a ball projectile, and the ball flies toward the pixel. The pixel is removed from the model immediately and bounces+scales-down before returning to the pool. |
| **Release** | If the pig runs out of ammo, it scales down and returns to the pool. If the lap completes with ammo remaining, the pig is added to the unit slot tray. |
| **Tap a tray pig** | A pig in the slot tray can be tapped to launch again, with the rest of the tray shifting left. |
| **Win / Fail** | All pixels destroyed → win panel + level advance. Tray fills before pixels are cleared → fail panel + retry. |

Up to **5 pigs can orbit in parallel** without blocking each other, and the path traversal stops early when ammo hits zero or the unit is deactivated.

---

## 🏗️ Architecture

PixelFlow uses a **layered MVP (Model-View-Presenter)** architecture with **manual dependency injection** at the composition root. There is no DI container — `GameBootstrapper` and `GameplayBootstrapper` wire all dependencies by hand. This keeps the call graph explicit, lifetime management trivial, and start-up overhead minimal on mobile.

### Core principles

- **Models** hold game state and raise events (`OnUnitAdvanced`, `OnSlotsFull`, `OnUpdateCellData`, ...). They never touch Unity rendering primitives.
- **Views** are `MonoBehaviour`s that handle Unity-specific work (transforms, raycasts, button listeners). They expose small interfaces and emit input events upward.
- **Presenters** subscribe to model and view events, route input through handlers, and drive the visual layer in response to model changes. They are pure C# classes implementing `IDisposable`.
- **Handlers** encapsulate cross-cutting domain logic (`LaneUnitShootHandler`, `LevelResultHandler`, `*FactoryHandler`). They sit between presenters and models/factories, hiding orchestration details.
- **Services** own application-level concerns that span domains (`GameplaySetupService`, `LevelDataService`, `InputService`, `ObjectPoolService`).
- **Static utilities** are stateless and side-effect-free (`OrbitRunner`, `OrbitLineSearcher`, `LaneUnitOrbitPathGenerator`, `GridIndexUtil`, `DirectionLookup`).
- **ScriptableObjects** drive every tunable value — config containers nest other configs, so the entire game can be reconfigured without code changes.

### Layered composition

```
GameBootstrapper
└── GameplayBootstrapper
    ├── Models           (PixelGridModel, LaneModel, UnitSlotModel, LevelProgressionModel)
    ├── Views            (PixelGridView, LaneView, UnitSlotView, LevelEndView)
    ├── Factories        (PixelCellFactory, LaneUnitFactory, ProjectileFactory)
    ├── FactoryHandlers  (PixelCellFactoryHandler, LaneUnitFactoryHandler)
    ├── Handlers         (LaneUnitShootHandler, LevelResultHandler)
    ├── Presenters       (PixelGridPresenter, LanePresenter, UnitSlotPresenter, LevelEndPresenter)
    └── Services         (GameplaySetupService, InputService)
```

---

## 🔧 Tech Stack

- **Unity** 3D (URP-ready, mobile-first)
- **C#** — modern language features (`record struct`, expression-bodied members, pattern matching)
- **DOTween** — all tweening (move, rotate, scale, jump, sequence)
- **UniTask** — async/await without GC, used for all coroutine-style workflows (orbit traversal, fire pipeline, release sequences, win/fail delays)
- **TextMeshPro** — ammo counters and UI text
- **Unity Input System** — pointer-based tap detection abstracted behind `IInputService`
- **NaughtyAttributes** — inspector buttons, `[Required]`, `[ReadOnly]`, validation helpers

---

## 📁 Project Structure

```
Scripts/
├── Core/                                        # Engine-agnostic, reusable utilities
│   ├── Extensions/                              # Color, Float, Enumerable, GameObject, String
│   ├── Services/
│   │   ├── Pool/                                # Object pool service (type & id keyed)
│   │   └── Save/                                # JSON save service (PlayerPrefs-backed)
│   └── Utils/                                   # EditorLogger, PoolSearchUtils
│
└── Game/
    ├── Bootstrappers/                           # Composition roots + config containers
    ├── Debuggers/                               # OrbitPathDebugger, OrbitAnimationDebugger
    ├── Elements/
    │   ├── LaneElements/                        # BaseLaneUnitObject, PigUnitObject, BallProjectileObject
    │   └── PixelElements/                       # BasePixelCellObject, PixelCellObject
    ├── Factories/                               # Pool wrappers per element type
    ├── Level/
    │   ├── Configs/                             # ColorPalette, LevelDataServiceConfig
    │   ├── Data/                                # ColorId, LevelDefinition, LaneDefinition, JSON DTOs
    │   ├── Editor/                              # Custom editor window + import pipeline
    │   ├── Services/                            # LevelDataService
    │   └── Utils/                               # LevelJsonRuntimeUtils
    ├── Modules/                                 # Reusable animation modules
    ├── MVP/
    │   ├── Models/                              # Grid, Lane, UnitSlot, Level
    │   ├── Presenters/                          # Grid, Lane, UnitSlot, LevelEnd (+ handlers)
    │   └── Views/                               # Grid, Lane, UnitSlot, LevelEnd
    └── Services/                                # GameplaySetupService, InputService, LevelDefinitionProvider
```

---

## 🧩 Core Systems

### Bootstrapping & Composition Root

`GameBootstrapper` runs first and creates global, level-independent services:
- `ObjectPoolService` (configured by `PoolServiceConfigSO`)
- `JsonSaveService` (PlayerPrefs-backed)
- `LevelDataService` (parses JSON levels from Resources)
- `LevelProgressionModel` (tracks current level + completion count, persisted)
- `LevelDefinitionProvider` (read-only facade combining the two above)

It then hands these to `GameplayBootstrapper`, which builds the per-level dependency graph: models, factories, factory handlers, presenters, handlers, the setup service, and finally calls `SetupGameplay()` to spawn the first level.

### Level Pipeline (PNG → JSON → Runtime)

Levels are authored as **PNG textures** and transformed into runtime data through three stages:

1. **`LevelEditorWindow`** (custom editor) — drag in PNG files, configure per-texture settings (level number, difficulty preset, max grid size, alpha threshold), and run greedy generation.
2. **`PngLevelParser` + `LaneGenerator` + `GreedyLevelValidator`** — pixels are quantized to `ColorId`, lanes are assembled greedily so every level is provably solvable, and `LevelJsonConverter` writes a `LevelJson` asset.
3. **`LevelJsonRuntimeUtils.ParseToLevelDefinition`** — converts JSON back into a `LevelDefinition` at runtime (called by `LevelDataService` on initialization).

This pipeline lets designers iterate on level art without ever touching code or hand-editing JSON.

### Orbit & Shoot System

The shooting mechanic is split into three orthogonal pieces:

- **`LaneUnitOrbitPathGenerator`** — given a `PixelGridView`, generates an array of `OrbitNode`s for the four edges of the grid. Trigger nodes (one per row/column) are aligned with cell centers; corner nodes are produced via quadratic Bezier interpolation so the pig's path curves smoothly. The launch node is the trigger node closest to a configurable left offset on the bottom edge.
- **`OrbitRunner`** — a static pure-function path driver. It moves a unit from node to node using DOTween via the `LaneUnitAnimationModule`, calling a pluggable `NodeHandler` callback at every trigger node. The runner stops early when ammo hits zero or the unit becomes inactive — the same runner is used by both the gameplay handler and the in-editor debugger.
- **`OrbitLineSearcher`** — a static line-of-sight scanner. Given a trigger node and a color, it walks the corresponding row or column inward and returns either the first matching pixel or, if a non-matching pixel blocks the line first, no match. This faithfully replicates the "first visible target wins" rule of the genre.

`LaneUnitShootHandler` ties them together: it owns the `_activeOrbiters` list (capped at five), owns the trigger callback, and orchestrates aim → fire → release. Firing is asynchronous and fire-and-forget so the orbit never blocks on individual pixel destructions.

### MVP Layer

| Domain | Model | View | Presenter |
|---|---|---|---|
| Grid | `PixelGridModel` (T[,] storage with active-cell mask) | `PixelGridView` (cell layout, area points) | `PixelGridPresenter` (placement on init) |
| Lane | `LaneModel` (per-lane unit lists) | `LaneView` (lane root construction, raycast) | `LanePresenter` (tap → handler, advance animation) |
| Unit Slot | `UnitSlotModel` (fixed-size slot array, shift-left on remove) | `UnitSlotView` (jump-to-slot animation, raycast) | `UnitSlotPresenter` (tap → handler, slot updates) |
| Level End | — | `LevelEndView` (win/fail panels, two buttons) | `LevelEndPresenter` (handler subscription, reset routing) |

### Result & Reset Pipeline

`LevelResultHandler` is a pure observer that subscribes to `IPixelGridModel.OnUpdateCellData` and `IUnitSlotModel.OnSlotsFull`. When the grid is fully empty it raises `OnLevelSuccess`; when the tray fills it raises `OnLevelFail`. A single `_isResolved` flag prevents double-firing, and `Reset()` clears it so the same handler instance can be reused after a retry.

`LevelEndPresenter` subscribes to those events, applies a short suspense delay through `UniTask`, and either shows the win panel (after advancing the progression model) or the fail panel. Both panels share a single button-click flow that calls `IGameplaySetupService.ResetGameplay()`.

`GameplaySetupService.ResetGameplay()` releases all active pixels, lane units, and projectiles back to their pools (using type-based bulk return), then calls `SetupGameplay()` again. Because the progression model has already advanced on win, the next level loads automatically; on fail, the same level is re-spawned.

### Pool Management

`ObjectPoolService` supports two key modes — **type-keyed** pools (for IPooledObject components, used for pigs/pixels/balls) and **id-keyed** pools (per-prefab, used for arbitrary prefab instances). Pools can be returned in bulk by type, returned all at once, or removed entirely. Returned objects are reparented to a pool root, so destroying lane root containers during reset never destroys pooled units that have already been released.

---

## 🛠️ Level Editor

`LevelEditorWindow` is a custom Unity editor window with three tabs:

- **Generate** — batch-import a folder of PNG textures, set per-texture settings, and produce JSON files for an entire level pack in one click.
- **Load** — load an existing JSON level for inspection.
- **Edit** — graphically edit a loaded level (paint pixels, reorder lanes, adjust ammo) and save back as either a new file or an overwrite.

Settings are persisted to `EditorPrefs` so config paths and texture folders survive across sessions, and a `DifficultyPreset` system maps difficulty levels to lane count, palette size, and ammo distribution so designers can author levels at a chosen difficulty target.

---

## 📊 Stats

| Metric | Count |
|---|---|
| Total scripts | 106 |
| Core scripts | 16 |
| Game scripts | 90 |
| Editor scripts | 10 |
| Interfaces | 24 |
| ScriptableObject types | 9 |
| ScriptableObject assets | 11 |

---

## 🎨 Design Principles

- **Interfaces at every seam** — every model, view, presenter dependency, factory, and service crosses an interface boundary, making swapping implementations and unit-level reasoning trivial.
- **Static where possible** — path generation, line search, orbit traversal, and grid index conversion are static and stateless, so they can be tested in isolation, allocate nothing, and never carry hidden state across calls.
- **Async/await, not coroutines** — UniTask gives composition (`WhenAll`, cancellation, `Forget()`), zero-allocation awaiters, and main-thread safety without `MonoBehaviour` glue.
- **No DI container** — manual wiring at composition roots makes the dependency graph readable in a single screen and removes a major source of mobile start-up overhead.
- **Pool first, instantiate never** — every dynamic Unity object goes through a factory that wraps the pool service. Reset and level transitions return objects in bulk by type.
- **ScriptableObject everywhere** — every magic number lives on a config asset. Code never assumes a value, only reads one.
- **Event-driven coordination** — handlers like `LevelResultHandler` are pure observers wired to model events, so adding new resolution conditions is a one-class change with no edits to the existing flow.

---

## 📝 License

All rights reserved. This project is proprietary.
