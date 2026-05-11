# HellCoffee.Engine

A 2D game engine built on top of **MonoGame 3.8** (.NET 9.0), designed for retro pixel art games with a clean, modular architecture.

---

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Systems](#systems)
  - [Core](#core)
  - [Scene Management](#scene-management)
  - [Input System](#input-system)
  - [Audio System](#audio-system)
  - [Graphics System](#graphics-system)
  - [Collision System](#collision-system)
  - [Game Management](#game-management)
  - [Debug System](#debug-system)
- [Demo Project](#demo-project)
- [Getting Started](#getting-started)
- [Dependencies](#dependencies)

---

## Overview

HellCoffee.Engine is a framework that abstracts common game development patterns — scene management, input handling, audio, sprites, animation, camera, tilemaps, collision detection, and save systems — so you can focus on building your game.

Key design decisions:
- **Virtual resolution** decoupled from window size for pixel-perfect scaling
- **Scene-based architecture** for clean game state organization
- **Component composition** over deep inheritance
- **Singleton access** to core systems via `Core.Instance`
- **Self-contained demo** with all textures generated procedurally (no external assets required)

---

## Project Structure

```
HellCoffee.Engine/
├── HellCoffee.Engine/            # Core framework (Class Library)
│   ├── Core/
│   │   └── Core.cs
│   ├── Scene/
│   │   ├── Scene.cs
│   │   └── SceneManager.cs
│   ├── Input/
│   │   ├── InputManager.cs
│   │   ├── KeyboardInput.cs
│   │   ├── MouseInput.cs
│   │   ├── GamepadInput.cs
│   │   └── InputAction.cs
│   ├── Audio/
│   │   └── AudioManager.cs
│   ├── Graphics/
│   │   ├── Sprites/
│   │   │   ├── TextureRegion.cs
│   │   │   ├── Sprite.cs
│   │   │   ├── AnimatedSprite.cs
│   │   │   ├── SpriteSheet.cs
│   │   │   └── TextureAtlas.cs
│   │   ├── Animation/
│   │   │   ├── Animation.cs
│   │   │   ├── AnimationFrame.cs
│   │   │   └── AnimationController.cs
│   │   ├── Camera/
│   │   │   ├── Camera2D.cs
│   │   │   └── CameraShake.cs
│   │   ├── Backgrounds/
│   │   │   ├── BackgroundLayer.cs
│   │   │   ├── SolidBackground.cs
│   │   │   └── ParallaxBackground.cs
│   │   └── Tilemap/
│   │       ├── Tilemap.cs
│   │       ├── TilemapLayer.cs
│   │       ├── Tileset.cs
│   │       └── Tile.cs
│   ├── Collision/
│   │   ├── Shapes/
│   │   │   ├── ICollisionShape.cs
│   │   │   ├── RectShape.cs
│   │   │   └── CircleShape.cs
│   │   ├── TileCollision.cs
│   │   └── PixelCollision.cs
│   ├── GameManager/
│   │   ├── GameManager.cs
│   │   ├── GameSettings.cs
│   │   └── SaveSystem.cs
│   └── Debug/
│       ├── DebugOverlay.cs
│       └── PixelFont.cs
│
├── HellCoffee.Demo/              # Demo game (WinExe)
│   ├── Program.cs
│   ├── Game1.cs                  # DemoGame : Core
│   ├── DemoScene.cs
│   └── Content/
│
└── HellCoffee.slnx               # Solution file
```

---

## Architecture

```
┌──────────────────────────────────────────────────────┐
│                   HellCoffee.Demo                    │
│           DemoGame (extends Core)                    │
│           DemoScene (extends Scene)                  │
└──────────────────┬───────────────────────────────────┘
                   │ uses
┌──────────────────▼───────────────────────────────────┐
│                 HellCoffee.Engine                    │
│                                                      │
│  Core ──┬── SceneManager ── Scene                    │
│         ├── InputManager  ── Keyboard/Mouse/Gamepad  │
│         ├── AudioManager  ── Music + SFX             │
│         ├── DebugOverlay  ── FPS / Collisions        │
│         └── Graphics      ── SpriteBatch (MonoGame)  │
│                                                      │
│  GameManager ── SaveSystem ── GameSettings           │
│  Camera2D    ── CameraShake                          │
│  AnimatedSprite ── AnimationController               │
│  TilemapLayer   ── TileCollision                     │
│  RectShape / CircleShape / PixelCollision            │
└──────────────────────────────────────────────────────┘
                   │ extends
┌──────────────────▼───────────────────────────────────┐
│              MonoGame 3.8 (.NET 9.0)                 │
└──────────────────────────────────────────────────────┘
```

---

## Systems

### Core

**File:** `HellCoffee.Engine/Core/Core.cs`

Abstract base class for your game. Extend it to create your game entry point.

```csharp
public class DemoGame : Core
{
    public DemoGame() : base(virtualWidth: 320, virtualHeight: 180) { }

    protected override void OnInitialize()
    {
        Window.Title = "My Game";
        Scenes.Change(new MyScene());
    }
}
```

**Key static accessors available anywhere:**

| Property | Type | Description |
|---|---|---|
| `Core.Instance` | `Core` | Singleton instance |
| `Core.GD` | `GraphicsDevice` | MonoGame graphics device |
| `Core.SB` | `SpriteBatch` | Main sprite batch |
| `Core.Input` | `InputManager` | Input hub |
| `Core.Audio` | `AudioManager` | Audio manager |
| `Core.Scenes` | `SceneManager` | Scene controller |
| `Core.Debug` | `DebugOverlay` | Debug overlay |
| `Core.VirtualWidth/Height` | `int` | Virtual render resolution |

The engine renders to a `RenderTarget2D` at the virtual resolution and scales it to the window using `PointClamp` sampling for crisp pixel art.

---

### Scene Management

**Files:** `Scene/Scene.cs`, `Scene/SceneManager.cs`

Organize your game into scenes. Each scene has its own `ContentManager` for automatic resource cleanup.

```csharp
public class GameplayScene : Scene
{
    private Player _player;

    protected override void OnInitialize()
    {
        _player = new Player();
    }

    public override void LoadContent()
    {
        // Content is automatically unloaded when leaving this scene
        var texture = Content.Load<Texture2D>("player");
    }

    public override void Update(GameTime gameTime) { }

    public override void Draw(GameTime gameTime) { }
}
```

**Switching scenes:**

```csharp
// Instant switch
Core.Scenes.Change(new MenuScene());

// With fade transition
Core.Scenes.ChangeFade(new GameplayScene(), fadeSpeed: 2.5f);
```

---

### Input System

**Files:** `Input/` directory

Unified input manager for keyboard, mouse, and up to 4 gamepads.

```csharp
var input = Core.Input;

// Keyboard
if (input.Keyboard.JustPressed(Keys.Space)) Jump();
if (input.Keyboard.IsDown(Keys.A)) MoveLeft();

// Mouse
var pos = input.Mouse.Position;
if (input.Mouse.JustPressed(MouseButton.Left)) Shoot();
var scroll = input.Mouse.ScrollDelta;

// Gamepad (player 1)
var pad = input.Gamepad(PlayerIndex.One);
if (pad.JustPressed(Buttons.A)) Jump();
pad.Vibrate(leftMotor: 0.5f, rightMotor: 0.5f, duration: 0.3f);
var stick = pad.LeftStick; // Vector2

// Named actions (rebindable)
var jump = new InputAction()
    .AddKey(Keys.Space)
    .AddKey(Keys.W)
    .AddButton(Buttons.A);

if (jump.JustPressed()) Jump();
```

---

### Audio System

**File:** `Audio/AudioManager.cs`

Manages music and sound effects with separate volume controls.

```csharp
var audio = Core.Audio;

// Music
audio.PlayMusic(Content.Load<Song>("music/theme"), repeat: true);
audio.PauseMusic();
audio.ResumeMusic();
audio.MusicVolume = 0.5f;   // 0.0 - 1.0

// Sound effects
audio.PlaySound(Content.Load<SoundEffect>("sfx/jump"));
audio.PlaySound(sfx, volume: 0.8f, pitch: 0.2f, pan: 0f, loop: false);
audio.SfxVolume = 1.0f;

// Global mute
audio.ToggleMute();
audio.StopAllSounds();
```

---

### Graphics System

#### Sprites

```csharp
// Basic sprite
var sprite = new Sprite(texture, position: new Vector2(100, 50));
sprite.Scale = new Vector2(2f, 2f);
sprite.Rotation = 0.5f;
sprite.Color = Color.Red;
sprite.CenterOrigin();
sprite.FlipHorizontal();
sprite.Draw(Core.SB);

// Sub-texture region from a sprite sheet
var region = new TextureRegion(atlas, sourceRect: new Rectangle(0, 0, 16, 16));
```

#### Animation

```csharp
// Extract frames from a sprite sheet grid
var frames = SpriteSheet.FromGrid(texture, frameWidth: 16, frameHeight: 16);

// Build an animation
var runAnim = new Animation(frames[0..4], frameDuration: 0.1f, loop: true);
var jumpAnim = new Animation(new[] { frames[4] }, frameDuration: 0.1f, loop: false);

// AnimationController state machine
var controller = new AnimationController();
controller.Register("idle", idleAnim);
controller.Register("run",  runAnim);
controller.Register("jump", jumpAnim);
controller.OnAnimationFinished += name => { /* ... */ };

controller.Play("run");
controller.Update(gameTime);

// Animated sprite (combines Sprite + AnimationController)
var animSprite = new AnimatedSprite(texture, controller, position);
animSprite.Update(gameTime);
animSprite.Draw(Core.SB);
```

#### TextureAtlas

```csharp
// Load atlas from XML definition
var atlas = TextureAtlas.Load(Content, "atlas");

// Create sprite from named region
var sprite = atlas.CreateSprite("player_idle");

// Create animated sprite from named animation
var animated = atlas.CreateAnimatedSprite("player");
animated.Controller.Play("run");
```

#### Camera

```csharp
var camera = new Camera2D(Core.VirtualWidth, Core.VirtualHeight);

// Follow a target with smoothing
camera.Position = Vector2.Lerp(camera.Position, playerPos, 0.1f);
camera.Zoom = 2f;
camera.Rotation = 0f;

// Screen shake
camera.Shake.Trigger(intensity: 5f, duration: 0.3f);
camera.Shake.Update(gameTime);

// Apply to SpriteBatch
Core.SB.Begin(transformMatrix: camera.TransformMatrix);

// Screen to world coordinate conversion
var worldPos = camera.ScreenToWorld(mousePosition);
```

#### Parallax Backgrounds

```csharp
var backgrounds = new List<BackgroundLayer>
{
    new SolidBackground(Color.DarkSlateBlue),
    new ParallaxBackground(skyTexture,   parallaxX: 0.1f, parallaxY: 0f, repeatX: true),
    new ParallaxBackground(cloudsTexture, parallaxX: 0.4f, parallaxY: 0f, repeatX: true),
    new ParallaxBackground(hillsTexture,  parallaxX: 0.7f, parallaxY: 0f, repeatX: true),
};

// In Draw():
foreach (var bg in backgrounds)
    bg.Draw(Core.SB, camera.Position);
```

#### Tilemap

```csharp
// Create tileset from texture
var tileset = new Tileset(tileTexture, tileWidth: 16, tileHeight: 16);

// Create a layer and set tiles
var layer = new TilemapLayer(tileset, mapWidth: 40, mapHeight: 15, tileSize: 16);
layer.SetTile(x: 5, y: 10, tileIndex: 3, flags: TileFlags.Solid);

// Render (frustum culled automatically)
layer.Draw(Core.SB, camera);

// Query collision
bool solid = layer.IsSolidAt(worldX, worldY);
var solidRects = layer.GetSolidTileRects(regionRect);

// Available tile flags:
// TileFlags.None | Solid | OneWay | Lethal | Water | Ladder
```

---

### Collision System

#### Shapes

```csharp
var box    = new RectShape(x: 10, y: 10, width: 16, height: 24);
var circle = new CircleShape(centerX: 50, centerY: 50, radius: 8);

// Test intersection
if (box.Intersects(circle)) { /* collision! */ }

// Get minimum separation vector
Vector2 sep = box.GetSeparation(otherBox);
position += sep; // push out of overlap
```

#### Tile Collision Resolution

```csharp
// Resolve entity against a tilemap layer
var separation = TileCollision.Resolve(box, tilemapLayer);
position += separation;

// Ground / wall queries
bool grounded    = TileCollision.IsGrounded(box, tilemapLayer);
bool wallLeft    = TileCollision.HasWallLeft(box, tilemapLayer);
bool wallRight   = TileCollision.HasWallRight(box, tilemapLayer);
bool ceiling     = TileCollision.HasCeiling(box, tilemapLayer);
```

#### Pixel-Perfect Collision

```csharp
// Simple pixel collision
bool hit = PixelCollision.Check(textureA, posA, textureB, posB);

// With transform (rotation / scale)
bool hit = PixelCollision.CheckTransformed(texA, transformA, texB, transformB);
```

---

### Game Management

#### GameManager

```csharp
var gm = GameManager.Instance;

// State
gm.Pause();
gm.Resume();
gm.TogglePause();

// Score / lives
gm.AddScore(100);
gm.Lives--;

// Events
gm.OnScoreChanged += score => UpdateHUD(score);
gm.OnGameOver += () => Core.Scenes.ChangeFade(new GameOverScene());
gm.OnPaused  += () => ShowPauseMenu();

// Navigate
gm.GoToScene(new MenuScene());
gm.Reset();

// Persistence
gm.SaveGame(slot: 0);
gm.LoadGame(slot: 0);
```

#### Settings

```csharp
// Loaded/saved automatically to %LocalAppData%\HellCoffee\settings.json
var settings = GameSettings.Instance;

settings.MusicVolume = 0.7f;
settings.SfxVolume   = 1.0f;
settings.Fullscreen  = true;
settings.Language    = "pt-BR";
settings.Save();
```

#### Save System

```csharp
// Generic JSON persistence (%LocalAppData%\HellCoffee\saves\{slot}.json)
var data = new MyGameSave { Level = 3, Score = 5000 };
SaveSystem.Save(data, slot: 0);

var loaded = SaveSystem.Load<MyGameSave>(slot: 0);
bool exists = SaveSystem.HasSave(slot: 0);
SaveSystem.DeleteSave(slot: 0);
var slots = SaveSystem.GetSaveSlots(); // int[]
```

---

### Debug System

**Files:** `Debug/DebugOverlay.cs`, `Debug/PixelFont.cs`

In-game debug overlay with no external font dependencies.

```csharp
var debug = Core.Debug;

// Toggle features with function keys (only in DEBUG builds)
// F1 → ShowCollisions
// F2 → ShowFps
// F3 → ShowTileGrid
// F4 → Invincible

// Add custom info lines to the overlay
debug.AddInfoLine($"Velocity: {velocity}");
debug.AddInfoLine($"Grounded: {isGrounded}");
debug.ClearInfoLines();

// Draw collision shapes manually
debug.DrawCollisionRect(Core.SB, rect, Color.Lime);
debug.DrawCollisionCircle(Core.SB, circle, Color.Orange);
debug.DrawTileGrid(Core.SB, tilemapLayer, camera);

// Embedded pixel font (no assets needed)
PixelFont.Draw(Core.SB, "HELLO WORLD", position, Color.White, scale: 2f);
PixelFont.DrawWithShadow(Core.SB, "FPS: 60", position, Color.Yellow);
var size = PixelFont.Measure("text", scale: 1f); // Vector2
```

---

## Demo Project

`HellCoffee.Demo` is a fully working demo showcasing most engine features with **zero external assets** — all textures are generated procedurally at runtime.

**Features demonstrated:**
- Player character with run/jump/idle animation state machine
- Physics: gravity, jump force, ground clamping, landing camera shake
- AABB collision detection and resolution
- Smooth camera follow with configurable lag
- Parallax background layers (sky + far + near)
- Debug overlay with collision shapes and info lines

**Controls:**

| Action | Keys |
|---|---|
| Move | A / D or Arrow Keys |
| Jump | W / Up / Space |
| Toggle Collisions | F1 |
| Toggle FPS | F2 |
| Toggle Tile Grid | F3 |
| Toggle Invincible | F4 |

**Running the demo:**

```bash
cd HellCoffee.Demo
dotnet run
```

---

## Getting Started

### 1. Add the engine to your solution

Reference `HellCoffee.Engine` from your game project:

```xml
<ProjectReference Include="..\HellCoffee.Engine\HellCoffee.Engine.csproj" />
```

### 2. Create your game class

```csharp
// Program.cs
using var game = new MyGame();
game.Run();

// MyGame.cs
public class MyGame : Core
{
    public MyGame() : base(virtualWidth: 320, virtualHeight: 180) { }

    protected override void OnInitialize()
    {
        Window.Title = "My Game";
        IsMouseVisible = false;
        Scenes.Change(new TitleScene());
    }
}
```

### 3. Create your first scene

```csharp
public class TitleScene : Scene
{
    public override void LoadContent()
    {
        // Load your assets here
    }

    public override void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.JustPressed(Keys.Enter))
            Core.Scenes.ChangeFade(new GameplayScene());
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GD.Clear(Color.Black);
        Core.SB.Begin(samplerState: SamplerState.PointClamp);
        // Draw your scene
        Core.SB.End();
    }
}
```

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `MonoGame.Framework.DesktopGL` | 3.8.* | Core graphics, audio, input framework |
| `MonoGame.Content.Builder.Task` | 3.8.* | Content pipeline (MGCB) integration |

**Runtime:** .NET 9.0  
**Platform:** Windows (DesktopGL profile — portable to Linux/macOS)

---

## License

This project is open source. Feel free to use it as a base for your own games.
