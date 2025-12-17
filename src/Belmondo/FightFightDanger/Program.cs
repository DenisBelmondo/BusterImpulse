using System.Numerics;
using Belmondo.FightFightDanger;
using Belmondo.FightFightDanger.Raylib_cs;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;
using static Belmondo.Mathematics.Extensions;

internal class Program
{
    private const int VIRTUAL_SCREEN_WIDTH = 640;
    private const int VIRTUAL_SCREEN_HEIGHT = 480;

    private const int VIRTUAL_SCREEN_HALF_WIDTH = VIRTUAL_SCREEN_WIDTH / 2;
    private const int VIRTUAL_SCREEN_HALF_HEIGHT = VIRTUAL_SCREEN_HEIGHT / 2;

    //
    // render stuff
    //
    private static readonly Vector3[] _goonDieControlPoints;

    private static Camera3D _camera;
    private static Camera3D _battleCamera;
    private static RenderTexture2D _gameWorldRenderTexture;
    private static RenderTexture2D _outerRenderTexture;
    private static Vector3 _cameraDirection;
    private static Vector2 _currentScreenSize;

    private static Vector2 Flatten(Vector3 v) => new(v.X, v.Z);
    private static Vector3 Make3D(Vector2 v) => new(v.X, 0, v.Y);

    private static void TextDraw(Font font, string text, Vector2 screenSize, Vector2 virtualScreenSize, Vector2 position)
    {
        var scaleFactor = screenSize.Y / virtualScreenSize.Y;
        var scaledFontSize = 15 * scaleFactor;

        DrawTextEx(font, text, position + Vector2.One * scaleFactor, scaledFontSize, 1, Color.DarkBlue);
        DrawTextEx(font, text, position, scaledFontSize, 1, Color.White);
    }

    static Program()
    {
        _goonDieControlPoints = [
            Vector3.Zero,
            -Vector3.UnitX / 2 + Vector3.UnitY + Vector3.UnitZ,
            -Vector3.UnitX - Vector3.UnitY * 4 + Vector3.UnitZ * 2
        ];
    }

    private static void Main(string[] args)
    {
        SetConfigFlags(ConfigFlags.WindowResizable);

        InitWindow(1024, 768, "Fight Fight Danger");
        InitAudioDevice();
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

            ViewModel viewModel = new(gameContext);
            Game game = new(gameContext);

            game.EnemyDamaged += viewModel.ShakeBattleEnemy;
            game.EnemyDied += viewModel.KillBattleEnemy;
            game.PlayerDamaged += viewModel.ShakeScreen;

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
                    Items = new Dictionary<int, int>()
                    {
                        [(int)ItemType.ChickenLeg] = 1,
                    },
                },
                new()
                {
                    Position = (6, 5),
                });

            world.SpawnChest(
                new()
                {
                    Items = new Dictionary<int, int>()
                    {
                        [(int)ItemType.WholeChicken] = 1,
                    },
                },
                new()
                {
                    Position = (7, 5),
                });

            world.SpawnChest(
                new()
                {
                    Items = new Dictionary<int, int>()
                    {
                        [(int)ItemType.ChickenLeg] = 1,
                    },
                },
                new()
                {
                    Position = (5, 7),
                });

            world.ChestOpened += chestID =>
            {
                var items = world.Chests[chestID].Value.Items;

                foreach (var kvp in items)
                {
                    viewModel.ShowPopUp($"{ItemTypeNames.Get((ItemType)kvp.Key)} x{1}");
                }
            };

            world.Player.Transform.Position = (5, 5);
            game.World = world;

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

            game.StateAutomaton.CurrentState = Game.ExploreState;

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
            SetTextureFilter(_outerRenderTexture.Texture, TextureFilter.Bilinear);

            double oldTime = GetTime();

            while (!WindowShouldClose())
            {
                _currentScreenSize = new(GetScreenWidth(), GetScreenHeight());
                raylibAudioService.Update();

                double newTime = GetTime();
                double delta = newTime - oldTime;

                oldTime = newTime;

                timeContext.Delta = delta;
                timeContext.Time += delta;

                game.Update();
                viewModel.Update();

                Render(game, timeContext, viewModel);
            }

            UnloadRenderTexture(_gameWorldRenderTexture);
            UnloadRenderTexture(_outerRenderTexture);
        }

        RaylibResources.UnloadAll();
        CloseAudioDevice();
        CloseWindow();
    }

    private static void Render(in Game game, in TimeContext timeContext, in ViewModel viewModel)
    {
        BeginTextureMode(_gameWorldRenderTexture);
        {
            ClearBackground(Color.Black);

            if (game.StateAutomaton.CurrentState == Game.ExploreState)
            {
                if (game.World is not null)
                {
                    RenderWandering(game.World, timeContext);
                }
            }
            else if (game.StateAutomaton.CurrentState == Game.BattleState)
            {
                if (game.Battle is not null)
                {
                    RenderBattle(game.Battle, timeContext);
                }
            }

            var mat = Math2.FitContain(new(320, 240), new(VIRTUAL_SCREEN_WIDTH, VIRTUAL_SCREEN_HEIGHT));

            Console.WriteLine(mat);

            //
            // draw hud
            //

            if (game.World is not null)
            {
                DrawRectanglePro(
                    new()
                    {
                        X = mat.M31,
                        Y = (240 - 32) * mat.M32 + (240 - 32) * mat.M22,
                        Width = 320 * mat.M11,
                        Height = 32 * mat.M22,
                    },
                    Vector2.Zero,
                    0,
                    new(16, 16, 16));

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
                        X = mat.M31,
                        Y = virtualMugshotY * mat.M32 + virtualMugshotY * mat.M22,
                        Width = RaylibResources.MugshotTexture.Width * mat.M11,
                        Height = RaylibResources.MugshotTexture.Height * mat.M22,
                    },
                    Vector2.Zero,
                    0,
                    Color.White);

                DrawTextEx(
                    RaylibResources.Font,
                    Math.Floor(game.World.Player.Value.RunningHealth).ToString(),
                    new(32 * mat.M11 + mat.M31, (240 - 15) * mat.M32 + (240 - 15) * mat.M22),
                    15 * mat.M22,
                    1,
                    Color.RayWhite);
            }

            //
            // draw pop  ups
            //

            if (viewModel.PopUpT > 0)
            {
                var t = Kryz.Tweening.EasingFunctions.InOutCubic(viewModel.PopUpT);

                DrawRectangle(0, (int)(t * 16) - 16, VIRTUAL_SCREEN_WIDTH, 16, new(16, 16, 16));
                DrawTextEx(
                    RaylibResources.Font,
                    viewModel.CurrentPopUpMessage,
                    new(0, (int)(t * 16) - 16),
                    15,
                    1,
                    Color.White);
            }

            BeginShaderMode(RaylibResources.ScreenTransitionShader2);
            {
                unsafe
                {
                    float time = (float)timeContext.Time * 4;

                    SetShaderValue(
                        RaylibResources.ScreenTransitionShader2,
                        RaylibResources.ScreenTransitionShader2TimeLoc,
                        &time,
                        ShaderUniformDataType.Float);
                }
            }
            EndShaderMode();
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
        var enemyDeathOffset = Vector3.Zero;
        var enemyFrameNumber = 0;
        var shouldDraw = true;

        if (battle.StateAutomaton.CurrentState != Battle.ChoosingState)
        {
            if (battle.CurrentBattleGoon is not null)
            {
                var animation = battle.CurrentBattleGoon.CurrentAnimationContext.Animation;
                var animationDuration = battle.CurrentBattleGoon.CurrentAnimationContext.FlyOffscreenTimerContext.DurationSeconds;

                if (animationDuration != 0)
                {
                    var animationSecondsRemaining = battle.CurrentBattleGoon.CurrentAnimationContext.FlyOffscreenTimerContext.SecondsRemaining;
                    var t = (float)((animationDuration - animationSecondsRemaining) / animationDuration);

                    enemyDeathOffset = Math2.SampleCatmullRom(
                        _goonDieControlPoints,
                        t);
                }

                if (battle.CurrentBattleGoon.ShakeStateAutomaton.CurrentState == BattleGoon.ShakeState)
                {
                    enemyPosition.X = battle.CurrentBattleGoon.CurrentShakeContext.Offset;
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
                BeginShaderMode(RaylibResources.SurfaceShader);
                {
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

        if (battle.StateAutomaton.CurrentState == Battle.ChoosingState)
        {
            DrawTextEx(RaylibResources.Font, "P: Attack Phase", new(0, 0), 15, 1, Color.White);
            DrawTextEx(RaylibResources.Font, "I: Item", new(0, 15), 15, 1, Color.White);
            DrawTextEx(RaylibResources.Font, "R: Run", new(0, 15 * 2), 15, 1, Color.White);
        }
        else if (battle.StateAutomaton.CurrentState == Battle.PlayingState)
        {
            DrawTextEx(RaylibResources.Font, "Dodge left and right!", new(0, 0), 15, 1, Color.White);
            DrawTextEx(RaylibResources.Font, "Align the crosshair with the enemy and press space!", new(0, 15), 15, 1, Color.White);

            if (battle.CrosshairStateAutomaton.CurrentState == Battle.CrosshairAimingState)
            {
                var crosshairColor = Color.White;

                if (battle.CurrentPlayingContext.CrosshairIsInRange(-0.125f, 0.125f))
                {
                    crosshairColor = Color.Red;
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
                        X = VIRTUAL_SCREEN_HALF_WIDTH * (battle.CurrentPlayingContext.CrosshairT + 1) - 8,
                        Y = VIRTUAL_SCREEN_HALF_HEIGHT - 8,
                        Width = 16,
                        Height = 16,
                    },
                    Vector2.Zero,
                    0,
                    crosshairColor);
            }
            else if (battle.CrosshairStateAutomaton.CurrentState == Battle.CrosshairTargetState)
            {
                var sizeMod = battle.CurrentPlayingContext.CrosshairTimerContext.SecondsRemaining * 10;
                var texWidth = RaylibResources.CrosshairAtlasTexture.Height * sizeMod;
                var texHeight = RaylibResources.CrosshairAtlasTexture.Height * sizeMod;

                DrawTexturePro(
                    RaylibResources.CrosshairAtlasTexture,
                    new()
                    {
                        X = 16,
                        Width = 16,
                        Height = 16,
                    },
                    new()
                    {
                        X = (float)((VIRTUAL_SCREEN_HALF_WIDTH * (battle.CurrentPlayingContext.CrosshairT + 1)) - texWidth / 2),
                        Y = (float)(VIRTUAL_SCREEN_HALF_HEIGHT - texHeight / 2),
                        Width = (float)texWidth,
                        Height = (float)texHeight,
                    },
                    Vector2.Zero,
                    0,
                    Color.Green);
            }
        }
        else if (battle.StateAutomaton.CurrentState == Battle.VictoryState)
        {
            DrawTextEx(RaylibResources.Font, "You won!", new(0, 0), 15, 1, Color.White);
        }
    }
}
