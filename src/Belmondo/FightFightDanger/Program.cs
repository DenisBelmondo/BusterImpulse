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

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Fight Fight Danger");
InitAudioDevice();
RaylibResources.CacheAndInitializeAll();

{
    RaylibAudioService raylibAudioService = new();
    RaylibInputService raylibInputService = new();

    Services services = new()
    {
        AudioService = raylibAudioService,
        InputService = raylibInputService,
    };

    TimeContext timeContext = new();

    Game game = new(services, timeContext);
    World world = new(services, timeContext);

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

    game.GameState.WorldState.SpawnChest(
        new()
        {
            Items = new Dictionary<int, int>(),
        },
        new()
        {
            Position = (6, 5),
        });

    game.GameState.WorldState.SpawnChest(
        new()
        {
            Items = new Dictionary<int, int>(),
        },
        new()
        {
            Position = (7, 5),
        });

    game.GameState.WorldState.Player.Transform.Position = (5, 5);
    game.World = world;

    Camera3D camera = new()
    {
        FovY = 90,
        Projection = CameraProjection.Perspective,
        Up = Vector3.UnitY,
    };

    game.GameState.WorldState.OldPlayerX = game.GameState.WorldState.Player.Transform.Position.X;
    game.GameState.WorldState.OldPlayerY = game.GameState.WorldState.Player.Transform.Position.Y;

    camera.Position.X = game.GameState.WorldState.Player.Transform.Position.X;
    camera.Position.Z = game.GameState.WorldState.Player.Transform.Position.Y;

    Vector3 cameraDirection;
    {
        var (X, Y) = Direction.ToInt32Tuple(game.GameState.WorldState.Player.Transform.Direction);
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

        timeContext.Delta = delta;
        timeContext.Time += delta;
        game.Update();

        // [FIXME]: BRUTAL fucking hack
        var isInBattle = false
            || (game.StateAutomaton.CurrentState == game.BattleScreenWipe && game.GameState.BattleState.ScreenWipeState.T > 0.75f)
            || game.StateAutomaton.CurrentState == game.BattleStart
            || game.StateAutomaton.CurrentState == game.BattleStartPlayerAttack
            || game.StateAutomaton.CurrentState == game.BattlePlayerAiming
            || game.StateAutomaton.CurrentState == game.BattlePlayerMissed
            || game.StateAutomaton.CurrentState == game.BattleEnemyStartAttack
            || game.StateAutomaton.CurrentState == game.BattleEnemyAttack
            || game.StateAutomaton.CurrentState == game.BattlePlayerAttack;

        //
        // render
        //

        BeginTextureMode(renderTexture);
        {
            ClearBackground(Color.Black);

            Vector3 playerDirection3d;

            {
                var (X, Y) = Direction.ToInt32Tuple(game.GameState.WorldState.Player.Transform.Direction);
                playerDirection3d = new(X, 0, Y);
            }

            camera.Position = Vector3.Lerp(
                new Vector3(game.GameState.WorldState.OldPlayerX, 0, game.GameState.WorldState.OldPlayerY),
                new Vector3(game.GameState.WorldState.Player.Transform.Position.X, 0, game.GameState.WorldState.Player.Transform.Position.Y),
                game.GameState.WorldState.CameraPositionLerpT);

            Quaternion cameraRotation = QuaternionFromAxisAngle(
                Vector3.UnitY,
                cameraDirection.SignedAngleTo(playerDirection3d, Vector3.UnitY)
                    * 10
                    * (float)delta);

            cameraDirection = Vector3RotateByQuaternion(cameraDirection, cameraRotation);
            camera.Target = camera.Position + cameraDirection;

            Vector2 screenResolution = new(320, 240);
            float fTime = (float)timeContext.Time;
            float screenWipeT = game.GameState.BattleState.ScreenWipeState.T;

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
                        foreach (var spawnedChest in game.GameState.WorldState.Chests)
                        {
                            int subFrame = (int)(spawnedChest.Value.Current.Openness * 3);

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
                    (float X, float Y) = Direction.ToInt32Tuple(game.GameState.WorldState.Player.Transform.Direction);

                    X /= 1.5f;
                    Y /= 1.5f;
                    X += game.GameState.WorldState.Player.Transform.Position.X;
                    Y += game.GameState.WorldState.Player.Transform.Position.Y;

                    Rlgl.DisableDepthTest();
                    {
                        DrawBillboardPro(
                            camera,
                            RaylibResources.EnemyAtlas,
                            new(0, 0, 64, 64),
                            Make3D(new(X, Y))
                                + game.ShakeStateContext.GetValueOrDefault().Offset,
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
                    (int)(160 + (game.GameState.BattleState.PlayerAimingState.CurrentAimValue * 160)),
                    120,
                    8,
                    game.GameState.BattleState.PlayerAimingState.IsInRange()
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
                    game.GameState.DialogState.RunningLine.ToString(),
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
                    new Vector2((-GetScreenWidth() / 2f) + (320 / 2f * (GetScreenHeight() / 240f)), 0),
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
