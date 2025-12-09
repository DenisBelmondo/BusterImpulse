using System.Numerics;
using Belmondo.FightFightDanger;
using Belmondo.FightFightDanger.Raylib_cs;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;
using static Belmondo.Mathematics.Extensions;

static Vector2 Flatten(Vector3 v) => new(v.X, v.Z);
static Vector3 Make3D(Vector2 v) => new(v.X, 0, v.Y);

static void TextDraw(Font font, string text, Vector2 screenSize, Vector2 position)
{
    var scaleFactor = screenSize.Y / 240f;
    var scaledFontSize = 15 * scaleFactor;

    DrawTextEx(font, text, position + Vector2.One * scaleFactor, scaledFontSize, 1, Color.DarkBlue);
    DrawTextEx(font, text, position, scaledFontSize, 1, Color.White);
}

ViewModel viewModel = new();

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Fight Fight Danger");
InitAudioDevice();
RaylibResources.CacheAndInitializeAll();

{
    RaylibAudioService raylibAudioService = new();
    RaylibInputService raylibInputService = new();

    GameContext gameContext = new()
    {
        AudioService = raylibAudioService,
        InputService = raylibInputService,
    };

    Game game = new()
    {
        GameContext = gameContext,
    };

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
            Items = new Dictionary<int, int>(),
        },
        new()
        {
            Position = (6, 5),
        });

    world.SpawnChest(
        new()
        {
            Items = new Dictionary<int, int>(),
        },
        new()
        {
            Position = (7, 5),
        });

    world.Player.Transform.Position = (5, 5);
    game.World = world;
    game.Initialize();

    Camera3D camera = new()
    {
        FovY = 90,
        Projection = CameraProjection.Perspective,
        Up = Vector3.UnitY,
    };

    world.OldPlayerX = world.Player.Transform.Position.X;
    world.OldPlayerY = world.Player.Transform.Position.Y;

    camera.Position.X = world.Player.Transform.Position.X;
    camera.Position.Z = world.Player.Transform.Position.Y;

    Vector3 cameraDirection;
    {
        var (X, Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);
        cameraDirection = new(X, 0, Y);
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

    RenderTexture2D renderTexture = LoadRenderTexture(320, 240);

    double oldTime = GetTime();

    game.StateAutomaton.CurrentState = game.ExploreState;

    var plasmaShaderTimeLoc = GetShaderLocation(RaylibResources.PlasmaShader, "iTime");
    var plasmaShaderResolutionLoc = GetShaderLocation(RaylibResources.PlasmaShader, "iResolution");

    var screenTransitionShaderTimeLoc = GetShaderLocation(RaylibResources.ScreenTransitionShader, "iTime");
    var screenTransitionShaderResolutionLoc = GetShaderLocation(RaylibResources.ScreenTransitionShader, "iResolution");

    var downmixedShaderLUTLoc = GetShaderLocation(RaylibResources.DownmixedShader, "lutTexture");
    var downmixedShaderLUTSizeLoc = GetShaderLocation(RaylibResources.DownmixedShader, "lutTextureSize");

    var lutSize = new Vector2(RaylibResources.LUTTexture.Width, RaylibResources.LUTTexture.Height);

    while (!WindowShouldClose())
    {
        raylibAudioService.Update();

        double newTime = GetTime();
        double delta = newTime - oldTime;

        oldTime = newTime;

        gameContext.Delta = delta;
        gameContext.Time += delta;
        game.Update();
        viewModel.Update(gameContext);

        // [FIXME]: BRUTAL fucking hack
        var isInBattle = false
            || (game.StateAutomaton.CurrentState == game.BattleScreenWipe && game.BattleState.ScreenWipeState.T > 0.75f)
            || game.StateAutomaton.CurrentState == game.BattleStart
            || game.StateAutomaton.CurrentState == game.BattleStartPlayerAttack
            || game.StateAutomaton.CurrentState == game.BattlePlayerAiming
            || game.StateAutomaton.CurrentState == game.BattlePlayerMissed
            || game.StateAutomaton.CurrentState == game.BattleEnemyStartAttack
            || game.StateAutomaton.CurrentState == game.BattleEnemyAttack
            || game.StateAutomaton.CurrentState == game.BattlePlayerAttack
            || game.StateAutomaton.CurrentState == game.BattlePlayerItem;

        //
        // render
        //

        BeginTextureMode(renderTexture);
        {
            ClearBackground(Color.Black);

            Vector3 playerDirection3d;

            {
                var (X, Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);
                playerDirection3d = new(X, 0, Y);
            }

            camera.Position = Vector3.Lerp(
                new Vector3(world.OldPlayerX, 0, world.OldPlayerY),
                new Vector3(world.Player.Transform.Position.X, 0, world.Player.Transform.Position.Y),
                world.CameraPositionLerpT);

            Quaternion cameraRotation = QuaternionFromAxisAngle(
                Vector3.UnitY,
                cameraDirection.SignedAngleTo(playerDirection3d, Vector3.UnitY)
                    * 10
                    * (float)delta);

            cameraDirection = Vector3RotateByQuaternion(cameraDirection, cameraRotation);
            camera.Target = camera.Position + cameraDirection;

            Vector2 screenResolution = new(320, 240);
            float fTime = (float)gameContext.Time;
            float screenWipeT = game.BattleState.ScreenWipeState.T;

            unsafe
            {
                SetShaderValue(
                    RaylibResources.PlasmaShader,
                    plasmaShaderTimeLoc,
                    &fTime,
                    ShaderUniformDataType.Float);

                SetShaderValue(
                    RaylibResources.PlasmaShader,
                    plasmaShaderResolutionLoc,
                    &screenResolution,
                    ShaderUniformDataType.Vec2);

                SetShaderValue(
                    RaylibResources.ScreenTransitionShader,
                    screenTransitionShaderTimeLoc,
                    &screenWipeT,
                    ShaderUniformDataType.Float);

                SetShaderValue(
                    RaylibResources.ScreenTransitionShader,
                    screenTransitionShaderResolutionLoc,
                    &screenResolution,
                    ShaderUniformDataType.Vec2);
            }

            if (isInBattle)
            {
                BeginShaderMode(RaylibResources.PlasmaShader);
                {
                    DrawRectangle(0, 0, 320, 240, Color.White);
                }
                EndShaderMode();
            }

            BeginMode3D(camera);
            {
                if (!isInBattle)
                {
                    Rlgl.DisableBackfaceCulling();
                    {
                        DrawModel(RaylibResources.FloorModel, Vector3.UnitY * -0.5F, 1, Color.White);
                        DrawModel(RaylibResources.CeilingModel, Vector3.UnitY * 0.5F, 1, Color.White);
                    }
                    Rlgl.EnableBackfaceCulling();

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

                    BeginShaderMode(RaylibResources.BaseShader);
                    {
                        foreach (var spawnedChest in world.Chests)
                        {
                            int subFrame = (int)(spawnedChest.Value.Openness * 3);

                            DrawBillboardPro(
                                camera,
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

                if (isInBattle)
                {
                    (float X, float Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);

                    X /= 1.5f;
                    Y /= 1.5f;
                    X += world.Player.Transform.Position.X;
                    Y += world.Player.Transform.Position.Y;

                    var offs = Vector3.Zero;

                    if (viewModel.BattleEnemyBillboardShakeT > 0)
                    {
                        offs = new((Random.Shared.NextSingle() * 2 - 1) / 20f, 0, (Random.Shared.NextSingle() * 2 - 1) / 20f);
                    }

                    if (game.BattleState.Foe.Current.Health <= 0)
                    {
                        offs.Y = Kryz.Tweening.EasingFunctions.InOutCubic(Kryz.Tweening.EasingFunctions.InOutCubic(viewModel.BattleEnemyDieT)) - 1f;
                    }

                    Rlgl.DisableDepthTest();
                    {
                        DrawBillboardPro(
                            camera,
                            RaylibResources.EnemyAtlas,
                            new(viewModel.BattleEnemyFrame * 64, 0, 64, 64),
                            Make3D(new(X, Y))
                                + offs,
                            Vector3.UnitY,
                            Vector2.One,
                            Vector2.One / 2f,
                            0,
                            Color.White);
                    }
                    Rlgl.EnableDepthTest();
                }
            }
            EndMode3D();

            if (game.StateAutomaton.CurrentState == game.BattleScreenWipe)
            {
                BeginShaderMode(RaylibResources.ScreenTransitionShader);
                {
                    DrawRectangle(0, 0, 320, 240, Color.Black);
                }
                EndShaderMode();
            }

            if (game.StateAutomaton.CurrentState == game.BattlePlayerAiming)
            {
                DrawCircle(
                    (int)(160 + (game.BattleState.PlayerAimingState.CurrentAimValue * 160)),
                    120,
                    8,
                    game.BattleState.PlayerAimingState.IsInRange()
                        ? Color.Green
                        : Color.RayWhite);
            }

            for (int i = 0; i < game.Log.Lines.Count; i++)
            {
                TextDraw(
                    RaylibResources.Font,
                    game.Log.Lines[i],
                    new Vector2(
                        renderTexture.Texture.Width,
                        renderTexture.Texture.Height),
                    new Vector2(
                        0,
                        i * 15));
            }

            if (game.DialogStateAutomaton.CurrentState is not null)
            {
                DrawRectangle(0, renderTexture.Texture.Height - 48, renderTexture.Texture.Width, 48, Color.Black);
                TextDraw(
                    RaylibResources.Font,
                    game.DialogState.RunningLine.ToString(),
                    new Vector2(
                        renderTexture.Texture.Width,
                        renderTexture.Texture.Height
                    ),
                    new Vector2(
                        0,
                        renderTexture.Texture.Height - 48
                    )
                );
            }

            // draw hud
            DrawRectangle(0, 240 - 32, 320, 32, new(16, 16, 16));
            DrawTexture(RaylibResources.MugshotTexture, 0, 240 - 32, Color.White);
            DrawTextEx(
                RaylibResources.Font,
                world.Player.Value.RunningHealth.ToString(),
                new(32, 240 - 15),
                15,
                1,
                Color.RayWhite);
        }
        EndTextureMode();

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            BeginShaderMode(RaylibResources.DownmixedShader);
            {
                SetShaderValue(RaylibResources.DownmixedShader, downmixedShaderLUTLoc, RaylibResources.LUTTexture);

                SetShaderValue(
                    RaylibResources.DownmixedShader,
                    downmixedShaderLUTSizeLoc,
                    lutSize,
                    ShaderUniformDataType.Vec2);

                Vector2 shakeOffs = Vector2.Zero;

                if (viewModel.ScreenShakeT > 0)
                {
                    shakeOffs = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle()) * 8;
                }

                DrawTexturePro(
                    renderTexture.Texture,
                    new Rectangle()
                    {
                        Width = 320,
                        Height = -240,
                        Position = Vector2.Zero,
                    },
                    new Rectangle()
                    {
                        Width = 320 * (GetScreenHeight() / 240f),
                        Height = GetScreenHeight(),
                        Position = Vector2.Zero,
                    },
                    // [TODO]: cache screen size values.
                    // [INFO]: the x coordinates are flipped so plus goes left and minus goes right
                    new Vector2((-GetScreenWidth() / 2f) + (320 / 2f * (GetScreenHeight() / 240f)), 0) + shakeOffs,
                    0,
                    Color.White
                );
            }
            EndShaderMode();
        }
        EndDrawing();
    }

    UnloadRenderTexture(renderTexture);
}

RaylibResources.UnloadAll();
CloseAudioDevice();
CloseWindow();
