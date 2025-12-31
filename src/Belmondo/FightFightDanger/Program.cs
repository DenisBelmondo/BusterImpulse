using System.Numerics;
using Belmondo.FightFightDanger;
using Belmondo.FightFightDanger.Raylib_cs;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;
using static Belmondo.Mathematics.Extensions;
using System.Text;

internal static class Program
{
    private const int VIRTUAL_SCREEN_WIDTH = 640;
    private const int VIRTUAL_SCREEN_HEIGHT = 480;

    //
    // render stuff
    //
    private static readonly Matrix3x2 _mat240pTo480p = Math2.FitContain(new(320, 240), new(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT));
    private static readonly StringBuilder _sb = new();
    private static readonly Vector3[] _goonDieControlPoints;

    private static Camera3D _camera;
    private static Camera3D _battleCamera;
    private static RenderTexture2D _gameWorldRenderTexture;
    private static RenderTexture2D _outerRenderTexture;
    private static Vector3 _cameraDirection;
    private static Vector2 _currentScreenSize;
    private static bool _shouldQuit;

    static Program()
    {
        _goonDieControlPoints = [
            Vector3.Zero,
            -Vector3.UnitX / 2 + Vector3.UnitY + Vector3.UnitZ,
            -Vector3.UnitX - Vector3.UnitY * 4 + Vector3.UnitZ * 2
        ];
    }

    private static Vector2 Flatten(Vector3 v) => new(v.X, v.Z);
    private static Vector3 Make3D(Vector2 v) => new(v.X, 0, v.Y);

    private static void Main(string[] args)
    {
        SetConfigFlags(ConfigFlags.WindowResizable);

        InitWindow(1024, 768, "Fight Fight Danger");
        InitAudioDevice();
        SetExitKey(KeyboardKey.Null);
        RaylibResources.CacheAndInitializeAll();

        {
            RaylibAudioService raylibAudioService = new();
            RaylibInputService raylibInputService = new();

            TimeContext timeContext = new();

            GameContext gameContext = new()
            {
                AudioService = raylibAudioService,
                InputService = raylibInputService,
                TimeContext = timeContext,
            };

            Game game = new(gameContext);
            UIState uiState = new(gameContext);

            game.Battle.PlayerWon += uiState.StartBattleVictoryScreen;
            game.Battle.Penis += uiState.WipeAwayBattleVictoryScreen;
            game.PlayerDamaged += uiState.ShakeMugshot;
            game.Quit += () => _shouldQuit = true;

            World world = new();

            world.TileMap = new int[,]
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
                { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1 },
                { 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
                { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
                { 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            };

            world.SpawnChest(
                new()
                {
                    Position = (6, 5),
                });

            world.Chests[^1].Value.Inventory.Snacks[SnackType.ChickenLeg] = 1;

            world.SpawnChest(
                new()
                {
                    Position = (7, 5),
                });

            world.Chests[^1].Value.Inventory.Snacks[SnackType.WholeChicken] = 1;

            world.SpawnChest(
                new()
                {
                    Position = (5, 7),
                });

            world.Chests[^1].Value.Inventory.Snacks[SnackType.ChickenLeg] = 1;

            world.ChestOpened += chestID =>
            {
                var inventory = world.Chests[chestID].Value.Inventory;

                foreach ((var snackType, var quantity) in inventory.Snacks)
                {
                    if (quantity > 0)
                    {
                        uiState.ShowPopUp($"{Names.Get(snackType)} x{quantity}");
                    }
                }
            };

            world.Player.Transform.Position = (5, 5);
            game.SetWorld(world);

            _camera = new()
            {
                FovY = 90,
                Projection = CameraProjection.Perspective,
                Up = Vector3.UnitY,
            };

            _battleCamera = new()
            {
                FovY = 60,
                Projection = CameraProjection.Perspective,
                Up = Vector3.UnitY,
            };

            world.OldPlayerX = world.Player.Transform.Position.X;
            world.OldPlayerY = world.Player.Transform.Position.Y;

            game.StateAutomaton.ChangeState(Game.State.Exploring);

            _camera.Position.X = world.Player.Transform.Position.X;
            _camera.Position.Z = world.Player.Transform.Position.Y;

            {
                var (X, Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);
                _cameraDirection = new(X, 0, Y);
            }

            // [INFO]: for all shape drawing routines, raylib actually samples the "blank character texture" in its default
            // character set. for some reason this throws off the fragTexCoord (uv) varyings in shaders. you need to actually
            // inform raylib that all shapes should be drawn with this empty 1x1 texture instead.
            // https://github.com/raysan5/raylib/issues/1730
            {
                Texture2D texture = new()
                {
                    Format = PixelFormat.UncompressedR8G8B8A8,
                    Height = 1,
                    Id = Rlgl.GetTextureIdDefault(),
                    Mipmaps = 1,
                    Width = 1,
                };

                SetShapesTexture(texture, new(0, 0, 1, 1));
            }

            _gameWorldRenderTexture = LoadRenderTexture(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT);
            _outerRenderTexture = LoadRenderTexture(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT);
            SetTextureFilter(_outerRenderTexture.Texture, TextureFilter.Trilinear);

            double oldTime = GetTime();

            while (!_shouldQuit)
            {
                _shouldQuit |= WindowShouldClose();
                _currentScreenSize = new(GetScreenWidth(), GetScreenHeight());

                double newTime = GetTime();
                double delta = newTime - oldTime;

                oldTime = newTime;

                // semi-fixed timestep
                while (delta > 0)
                {
                    double cappedDelta = Math.Min(delta, 1 / 30.0);

                    timeContext.Delta = cappedDelta;

                    raylibInputService.Update();
                    game.Update();

                    delta -= cappedDelta;
                    timeContext.Time += cappedDelta;
                }

                raylibAudioService.Update();
                uiState.Update();
                Render(game, timeContext, uiState);
            }

            UnloadRenderTexture(_gameWorldRenderTexture);
            UnloadRenderTexture(_outerRenderTexture);
        }

        RaylibResources.UnloadAll();
        CloseAudioDevice();
        CloseWindow();
    }

    private static void Render(in Game game, in TimeContext timeContext, UIState uiState)
    {
        BeginTextureMode(_gameWorldRenderTexture);
        {
            ClearBackground(Color.Black);

            if (game.CurrentRenderHint == Game.RenderHint.Exploring)
            {
                if (game.World is not null)
                {
                    RenderWandering(game.World, timeContext);
                }
            }
            else if (game.CurrentRenderHint == Game.RenderHint.Battling)
            {
                if (game.Battle is not null)
                {
                    RenderBattle(game.Battle, timeContext);
                }
            }

            BeginShaderMode(RaylibResources.ScreenTransitionShader2);
            {
                DrawRectanglePro(
                    new()
                    {
                        Width = VIRTUAL_SCREEN_WIDTH,
                        Height = VIRTUAL_SCREEN_HEIGHT,
                    },
                    Vector2.Zero,
                    0,
                    Color.White);
            }
            EndShaderMode();

            if (game.StateAutomaton.CurrentState == Game.State.Menu)
            {
                DrawRectangle(0, 0, 640, 480, new(0, 0, 0, 128));
            }

            //
            // draw hud
            //

            if (game.World is not null)
            {
                DrawRectanglePro(
                    new()
                    {
                        X = _mat240pTo480p.M31,
                        Y = _mat240pTo480p.M32 + (240 - 32) * _mat240pTo480p.M22,
                        Width = 320 * _mat240pTo480p.M11,
                        Height = 32 * _mat240pTo480p.M22,
                    },
                    Vector2.Zero,
                    0,
                    new(16, 16, 16));

                var virtualMugshotX = uiState.CurrentMugshotStateContext.ShakeOffset.X;
                var virtualMugshotY = 240 - RaylibResources.MugshotTexture.Height;

                DrawTexturePro(
                    RaylibResources.MugshotTexture,
                    new()
                    {
                        X = 0,
                        Y = 0,
                        Width = RaylibResources.MugshotTexture.Width,
                        Height = RaylibResources.MugshotTexture.Height,
                    },
                    new()
                    {
                        X = virtualMugshotX * _mat240pTo480p.M11 + _mat240pTo480p.M31,
                        Y = virtualMugshotY * _mat240pTo480p.M22 + _mat240pTo480p.M32,
                        Width = RaylibResources.MugshotTexture.Width * _mat240pTo480p.M11,
                        Height = RaylibResources.MugshotTexture.Height * _mat240pTo480p.M22,
                    },
                    Vector2.Zero,
                    0,
                    Color.White);

                DrawTextEx(
                    RaylibResources.Font,
                    Math.Floor(game.World.Player.Value.RunningHealth).ToString(),
                    new(
                        (32 + 1) * _mat240pTo480p.M11 + _mat240pTo480p.M31,
                        (1 + _mat240pTo480p.M32) + (240 - 15) * _mat240pTo480p.M22),
                    15 * _mat240pTo480p.M22,
                    1 * _mat240pTo480p.M11,
                    new(24, 24, 24));

                DrawTextEx(
                    RaylibResources.Font,
                    Math.Floor(game.World.Player.Value.RunningHealth).ToString(),
                    new(
                        32 * _mat240pTo480p.M11 + _mat240pTo480p.M31,
                        _mat240pTo480p.M32 + (240 - 15) * _mat240pTo480p.M22),
                    15 * _mat240pTo480p.M22,
                    1 * _mat240pTo480p.M11,
                    Color.RayWhite);
            }

            //
            // draw pop  ups
            //

            if (uiState.PopUpT > 0)
            {
                var t = Kryz.Tweening.EasingFunctions.InOutCubic(uiState.PopUpT);

                DrawRectangle(
                    (int)_mat240pTo480p.M31,
                    (int)((t * 16) - 16 + _mat240pTo480p.M32),
                    (int)(320 * _mat240pTo480p.M11),
                    (int)(16 * _mat240pTo480p.M22),
                    new(16, 16, 16));

                DrawTextEx(
                    RaylibResources.Font,
                    uiState.CurrentPopUpMessage,
                    new(
                        4 + _mat240pTo480p.M31,
                        (int)((t * 16) - 16 + 1) * _mat240pTo480p.M22 + _mat240pTo480p.M32),
                    15 * _mat240pTo480p.M22,
                    1 * _mat240pTo480p.M11,
                    Color.RayWhite);
            }

            BeginShaderMode(RaylibResources.ScreenTransitionShader2);
            {
                unsafe
                {
                    float time2;

                    time2 = (float)game.CurrentTransitionContext.FadeT;
                    time2 = Kryz.Tweening.EasingFunctions.InOutCubic(time2);

                    SetShaderValue(
                        RaylibResources.ScreenTransitionShader2,
                        RaylibResources.ScreenTransitionShader2TimeLoc,
                        &time2,
                        ShaderUniformDataType.Float);
                }
            }
            EndShaderMode();

            if (uiState.BattleVictoryWipeT == 0.5)
            {
                DrawRectangle(0, (int)(VIRTUAL_SCREEN_HEIGHT / 2f - 15), VIRTUAL_SCREEN_WIDTH, 30, new(16, 16, 16));

                DrawTextEx(
                    RaylibResources.Font,
                    "Survived!",
                    new Vector2(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT) / 2f - MeasureTextEx(RaylibResources.Font, "Survived!", 30, 2) / 2f,
                    30,
                    2,
                    Color.White);
            }

            if (game.StateAutomaton.CurrentState == Game.State.Menu)
            {
                DrawMenu(in game);
            }
        }
        EndTextureMode();

        BeginTextureMode(_outerRenderTexture);
        {
            BeginShaderMode(RaylibResources.DownmixedShader);
            {
                DrawTexture(_gameWorldRenderTexture.Texture, 0, 0, Color.White);
            }
            EndShaderMode();
        }
        EndTextureMode();

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            SetShaderValue(RaylibResources.DownmixedShader, RaylibResources.DownmixedShaderLUTLoc, RaylibResources.LUTTexture);

            SetShaderValue(
                RaylibResources.DownmixedShader,
                RaylibResources.DownmixedShaderLUTSizeLoc,
                RaylibResources.LUTSize,
                ShaderUniformDataType.Vec2);

            var mat = Math2.FitContain(new(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT), _currentScreenSize);

            DrawTexturePro(
                _outerRenderTexture.Texture,
                new Rectangle()
                {
                    Width = VIRTUAL_SCREEN_WIDTH,
                    Height = VIRTUAL_SCREEN_HEIGHT,
                },
                new Rectangle()
                {
                    X = mat.M31,
                    Y = mat.M32,
                    Width = VIRTUAL_SCREEN_WIDTH * mat.M11,
                    Height = VIRTUAL_SCREEN_HEIGHT * mat.M22,
                },
                Vector2.Zero,
                0,
                Color.White);
        }
        EndDrawing();
    }

    private static void DrawMenu(in Game game)
    {
        DrawRectangle(640 - 256, 0, 256, 480, new(16, 16, 16));

        if (game.CurrentMenuContext.MenuStack.TryPeek(out Menu? menu))
        {
            var emptyText = string.Empty;

            if (menu.ID == ((int)Menus.ID.SnacksMenu))
            {
                emptyText = "Urgh... so hungry...\nGotta find food\nsomewhere...";
            }

            if (menu.Items.Count == 0)
            {
                DrawTextEx(
                    RaylibResources.Font,
                    emptyText,
                    new Vector2(640 - 256 + 8, 8),
                    30,
                    2,
                    Color.DarkGray);
            }
            else
            {
                for (int i = 0; i < menu.Items.Count; i++)
                {
                    // [FIXME]: dear god are you re-generating the string every
                    // render frame?
                    _sb.Clear();
                    _sb.Append(menu.Items[i].Label);

                    if (menu.Items[i].Quantity is int quantity)
                    {
                        _sb.Append($" x{quantity}");
                    }

                    var color = Color.DarkGray;

                    if (i == menu.CurrentItem)
                    {
                        color = Color.RayWhite;
                    }

                    DrawTextEx(
                        RaylibResources.Font,
                        _sb.ToString(),
                        new(640 - 256 + 8, i * 30 + 8),
                        30,
                        2,
                        color);
                }
            }
        }
    }

    private static void RenderWandering(in World world, in TimeContext timeContext)
    {
        Vector3 playerDirection3d;

        {
            var (X, Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);
            playerDirection3d = new(X, 0, Y);
        }

        _camera.Position = Vector3.Lerp(
            Make3D(new(world.OldPlayerX, world.OldPlayerY)),
            Make3D(new(world.Player.Transform.Position.X, world.Player.Transform.Position.Y)),
            world.CameraPositionLerpT);

        Quaternion cameraRotation = QuaternionFromAxisAngle(
            Vector3.UnitY,
            _cameraDirection.SignedAngleTo(playerDirection3d, Vector3.UnitY)
                * 10
                * (float)timeContext.Delta);

        _cameraDirection = Vector3RotateByQuaternion(_cameraDirection, cameraRotation);
        _camera.Target = _camera.Position + _cameraDirection;

        Vector2 virtualScreenSize = new(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT);

        unsafe
        {
            SetShaderValue(
                RaylibResources.PlasmaShader,
                RaylibResources.PlasmaShaderResolutionLoc,
                &virtualScreenSize,
                ShaderUniformDataType.Vec2);

            SetShaderValue(
                RaylibResources.ScreenTransitionShader,
                RaylibResources.ScreenTransitionShaderResolutionLoc,
                &virtualScreenSize,
                ShaderUniformDataType.Vec2);

            SetShaderValue(
                RaylibResources.ScreenTransitionShader2,
                RaylibResources.ScreenTransitionShader2ResolutionLoc,
                &virtualScreenSize,
                ShaderUniformDataType.Vec2);
        }

        BeginMode3D(_camera);
        {
            Rlgl.DisableBackfaceCulling();
            {
                DrawModel(RaylibResources.FloorModel, Vector3.UnitY * -0.5F, 1, Color.White);
                DrawModel(RaylibResources.CeilingModel, Vector3.UnitY * 0.5F, 1, Color.White);
            }
            Rlgl.EnableBackfaceCulling();

            if (world.TileMap is not null)
            {
                for (int y = 0; y < world.TileMap.GetLength(0); y++)
                {
                    for (int x = 0; x < world.TileMap.GetLength(1); x++)
                    {
                        if (world.TileMap[y, x] != 0)
                        {
                            DrawModel(RaylibResources.TileModel, Make3D(new(x, y)), 1, Color.White);
                        }
                    }
                }
            }

            BeginShaderMode(RaylibResources.BaseShader);
            {
                foreach (var spawnedChest in world.Chests)
                {
                    int subFrame = (int)(spawnedChest.Value.Openness * 3);

                    DrawBillboardPro(
                        _camera,
                        RaylibResources.ChestAtlas,
                        new Rectangle(subFrame * 32, 0, 32, 32),
                        Make3D(new(spawnedChest.Transform.Position.X, spawnedChest.Transform.Position.Y)),
                        Vector3.UnitY,
                        Vector2.One / 2f,
                        new Vector2(0.25f, 0.5f),
                        0,
                        Color.White
                    );
                }
            }
            EndShaderMode();
        }
        EndMode3D();
    }

    private static void RenderBattle(in Battle battle, in TimeContext timeContext)
    {
        var enemyPosition = Vector3.UnitZ * 2;
        var enemyShakeOffset = Vector2.Zero;
        var enemyDeathOffset = Vector3.Zero;
        var enemyFrameNumber = 0;
        var shouldDraw = true;

        if (battle.StateAutomaton.CurrentState != Battle.State.Choosing)
        {
            if (battle.CurrentBattleGoon is not null)
            {
                var animation = battle.CurrentBattleGoon.CurrentAnimationContext.Animation;
                var animationDuration = battle.CurrentBattleGoon.CurrentAnimationContext.FlyOffscreenTimer.DurationSeconds;

                if (animationDuration != 0)
                {
                    float t = (float)battle.CurrentBattleGoon.CurrentAnimationContext.FlyOffscreenTimer.GetProgress();

                    enemyDeathOffset = Math2.SampleCatmullRom(
                        _goonDieControlPoints,
                        t);
                }

                enemyShakeOffset.X = battle.CurrentBattleGoon.CurrentAnimationContext.ShakeOffset.X;
                enemyShakeOffset.Y = battle.CurrentBattleGoon.CurrentAnimationContext.ShakeOffset.Y;

                if (battle.CurrentBattleGoon.ShakeStateAutomaton.CurrentState == BinaryState.On)
                {
                    enemyPosition.X = enemyShakeOffset.X;
                    enemyPosition.Y = enemyShakeOffset.Y;
                }

                if (animation == 0 || animation == 1)
                {
                    enemyFrameNumber = 0;
                }
                else if (animation == 2)
                {
                    enemyFrameNumber = 1;
                }
                else if (animation == 3)
                {
                    enemyFrameNumber = 2;
                }
                else if (animation == 4)
                {
                    enemyFrameNumber = 3;
                }
            }
        }

        float x = Math2.SampleParabola(battle.CurrentPlayingContext.PlayerDodgeT, MathF.Sign(battle.CurrentPlayingContext.PlayerDodgeT), -1, 0);

        _battleCamera.Position.X = x;
        _battleCamera.Target = _battleCamera.Position + Vector3.UnitZ;

        BeginMode3D(_battleCamera);
        {
            Rlgl.DisableBackfaceCulling();
            Rlgl.DisableDepthTest();
            {
                BeginShaderMode(RaylibResources.BattleBackgroundShapeShader);
                {
                    unsafe
                    {
                        float time = (float)timeContext.Time;
                        SetShaderValue(RaylibResources.BattleBackgroundShapeShader, RaylibResources.BattleBackgroundShapeShaderTimeLoc, &time, ShaderUniformDataType.Float);
                    }

                    DrawCube(Vector3.Zero, 5, 3, 40, Color.DarkBlue);
                }
                EndShaderMode();
            }
            Rlgl.EnableDepthTest();
            Rlgl.EnableBackfaceCulling();

            if (shouldDraw)
            {
                DrawBillboardRec(
                    _battleCamera,
                    RaylibResources.EnemyAtlas,
                    new()
                    {
                        X = enemyFrameNumber * 64,
                        Y = 0,
                        Width = 64,
                        Height = 64,
                    },
                    enemyPosition + enemyDeathOffset,
                    Vector2.One,
                    Color.White);
            }

            if (battle.CurrentBattleGoon is not null)
            {
                foreach (var bullet in battle.CurrentBattleGoon.Bullets)
                {
                    var bulletOffset = -Vector3.UnitY / 10f;
                    var bulletDestination = Vector3.Zero + Vector3.UnitX * bullet.HorizontalDirection / 10f;

                    var bulletPosition = Vector3Lerp(
                        enemyPosition + bulletOffset,
                        bulletDestination + bulletOffset,
                        bullet.Closeness);

                    DrawCubeV(
                        bulletPosition,
                        (Vector3.One + Vector3.UnitZ * 2) / 60f,
                        Color.Yellow);
                }
            }
        }
        EndMode3D();

        if (battle.StateAutomaton.CurrentState == Battle.State.Choosing)
        {
            DrawTextEx(RaylibResources.Font, "P: Attack Phase", new(0, 0), 15, 1, Color.White);
            DrawTextEx(RaylibResources.Font, "I: Item", new(0, 15), 15, 1, Color.White);
            DrawTextEx(RaylibResources.Font, "R: Run", new(0, 15 * 2), 15, 1, Color.White);
        }
        else if (battle.StateAutomaton.CurrentState == Battle.State.Playing)
        {
            DrawTextEx(RaylibResources.Font, "Dodge left and right!", new(0, 0), 15, 1, Color.White);
            DrawTextEx(RaylibResources.Font, "Align the crosshair with the enemy and press space!", new(0, 15), 15, 1, Color.White);

            if (battle.CurrentPlayingContext.CrosshairIsVisible)
            {
                var scaledSize = Vector2.Zero;
                float rot = (float)((battle.CurrentPlayingContext.CrosshairTimer.DurationSeconds - battle.CurrentPlayingContext.CrosshairTimer.SecondsRemaining) / battle.CurrentPlayingContext.CrosshairTimer.DurationSeconds) * 360;

                scaledSize.X = 16 * _mat240pTo480p.M11;
                scaledSize.Y = 16 * _mat240pTo480p.M22;
                scaledSize *= Vector2.One / (float)((battle.CurrentPlayingContext.CrosshairTimer.DurationSeconds - battle.CurrentPlayingContext.CrosshairTimer.SecondsRemaining) / battle.CurrentPlayingContext.CrosshairTimer.DurationSeconds);

                var crosshairColor = Color.White;

                if (battle.CurrentPlayingContext.CrosshairIsInRange(-0.125f, 0.125f))
                {
                    crosshairColor = Color.Red;
                }

                if (battle.CrosshairStateAutomaton.CurrentState == Battle.CrosshairState.Targeting)
                {
                    crosshairColor = Color.Green;
                }

                DrawTexturePro(
                    RaylibResources.CrosshairAtlasTexture,
                    new()
                    {
                        Width = 16,
                        Height = 16,
                    },
                    new()
                    {
                        X = 160 * (battle.CurrentPlayingContext.CrosshairT + 1) * _mat240pTo480p.M11 + _mat240pTo480p.M31,
                        Y = 120 * _mat240pTo480p.M22 + _mat240pTo480p.M32,
                        Width = scaledSize.X,
                        Height = scaledSize.Y,
                    },
                    scaledSize / 2f,
                    rot,
                    crosshairColor);
            }
        }
    }
}
