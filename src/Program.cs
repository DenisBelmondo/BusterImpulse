using System.Numerics;
using Belmondo;
using Belmondo.FightFightDanger;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;

static Vector2 Flatten(Vector3 v) => new(v.X, v.Z);
static Vector3 Make3D(Vector2 v) => new(v.X, 0, v.Y);

Game game = new();
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

world.Chests.Add(new Chest()
{
    Items = new Dictionary<int, int>(),
    Entity = new()
    {
        X = 6,
        Y = 5,
    },
});

world.Player.Entity.X = 5;
world.Player.Entity.Y = 5;

game.World = world;

Camera3D camera = new()
{
    FovY = 90,
    Projection = CameraProjection.Perspective,
    Up = Vector3.UnitY,
};

game.World.OldPlayerX = world.Player.Entity.X;
game.World.OldPlayerY = world.Player.Entity.Y;

camera.Position.X = world.Player.Entity.X;
camera.Position.Z = world.Player.Entity.Y;

Vector3 cameraDirection;

static void TextDraw(Font font, string text, Vector2 screenSize, Vector2 position)
{
    var scaleFactor = screenSize.Y / 240f;
    var scaledFontSize = 15 * scaleFactor;

    DrawTextEx(font, text, position + Vector2.One * scaleFactor, scaledFontSize, 1, Color.DarkBlue);
    DrawTextEx(font, text, position, scaledFontSize, 1, Color.White);
}

{
    var (X, Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);
    cameraDirection = new(X, 0, Y);
}

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Fight Fight Danger");
InitAudioDevice();
Resources.CacheAndInitializeAll();

{
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

    //
    // begin state initializations
    //

    //
    // end state initializations
    //

    game.StateAutomaton.CurrentState = game.ExploreState;

    var plasmaShaderTimeLoc = GetShaderLocation(Resources.PlasmaShader, "iTime");
    var plasmaShaderResolutionLoc = GetShaderLocation(Resources.PlasmaShader, "iResolution");

    var screenTransitionShaderTimeLoc = GetShaderLocation(Resources.ScreenTransitionShader, "iTime");
    var screenTransitionShaderResolutionLoc = GetShaderLocation(Resources.ScreenTransitionShader, "iResolution");

    var downmixedShaderLUTLoc = GetShaderLocation(Resources.DownmixedShader, "lutTexture");
    var downmixedShaderLUTSizeLoc = GetShaderLocation(Resources.DownmixedShader, "lutTextureSize");

    var lutSize = new Vector2(Resources.LUTTexture.Width, Resources.LUTTexture.Height);

    while (!WindowShouldClose())
    {
        game.Jukebox.Update();

        double newTime = GetTime();
        double delta = newTime - oldTime;

        oldTime = newTime;

        game.TimeContext.Delta = delta;
        game.TimeContext.Time += delta;
        game.Update();

        // [FIXME]: BRUTAL fucking hack
        var isInBattle = false
            || (game.StateAutomaton.CurrentState == game.BattleScreenWipe && game.CurrentScreenWipeContext.T > 0.75f)
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
                var (X, Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);
                playerDirection3d = new(X, 0, Y);
            }

            camera.Position = Vector3.Lerp(
                new(world.OldPlayerX, 0, world.OldPlayerY),
                new(world.Player.Entity.X, 0, world.Player.Entity.Y),
                world.CameraPositionLerpT);

            Quaternion cameraRotation = QuaternionFromAxisAngle(
                Vector3.UnitY,
                cameraDirection.SignedAngleTo(playerDirection3d, Vector3.UnitY)
                    * 10
                    * (float)delta);

            cameraDirection = Vector3RotateByQuaternion(cameraDirection, cameraRotation);
            camera.Target = camera.Position + cameraDirection;

            Vector2 screenResolution = new(320, 240);
            float fTime = (float)game.TimeContext.Time;
            float screenWipeT = game.CurrentScreenWipeContext.T;

            unsafe
            {
                SetShaderValue(
                    Resources.PlasmaShader,
                    plasmaShaderTimeLoc,
                    &fTime,
                    ShaderUniformDataType.Float);

                SetShaderValue(
                    Resources.PlasmaShader,
                    plasmaShaderResolutionLoc,
                    &screenResolution,
                    ShaderUniformDataType.Vec2);

                SetShaderValue(
                    Resources.ScreenTransitionShader,
                    screenTransitionShaderTimeLoc,
                    &screenWipeT,
                    ShaderUniformDataType.Float);

                SetShaderValue(
                    Resources.ScreenTransitionShader,
                    screenTransitionShaderResolutionLoc,
                    &screenResolution,
                    ShaderUniformDataType.Vec2);
            }

            if (isInBattle)
            {
                BeginShaderMode(Resources.PlasmaShader);
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
                        DrawModel(Resources.FloorModel, Vector3.UnitY * -0.5F, 1, Color.White);
                        DrawModel(Resources.CeilingModel, Vector3.UnitY * 0.5F, 1, Color.White);
                    }
                    Rlgl.EnableBackfaceCulling();

                    for (int y = 0; y < world.TileMap.GetLength(0); y++)
                    {
                        for (int x = 0; x < world.TileMap.GetLength(1); x++)
                        {
                            if (world.TileMap[y, x] != 0)
                            {
                                DrawModel(Resources.TileModel, Make3D(new(x, y)), 1, Color.White);
                            }
                        }
                    }

                    foreach (var chest in world.Chests)
                    {
                        DrawBillboardPro(
                            camera,
                            Resources.ChestAtlas,
                            new Rectangle(0, 0, 32, 32),
                            Make3D(new(chest.Entity.X, chest.Entity.Y)),
                            Vector3.UnitY,
                            Vector2.One / 2f,
                            new Vector2(0.25f, 0.5f),
                            0,
                            Color.White
                        );
                    }
                }

                if (isInBattle)
                {
                    (float X, float Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);

                    X /= 1.5f;
                    Y /= 1.5f;
                    X += world.Player.Entity.X;
                    Y += world.Player.Entity.Y;

                    Rlgl.DisableDepthTest();
                    {
                        DrawBillboardPro(
                            camera,
                            Resources.EnemyAtlas,
                            new(0, 0, 64, 64),
                            Make3D(new(X, Y))
                                + game.ShakeStateContext.Offset,
                            Vector3.UnitY,
                            Vector2.One,
                            Vector2.One / 2f,
                            0,
                            Color.White);
                    }
                }
            }
            EndMode3D();

            if (game.StateAutomaton.CurrentState == game.BattleScreenWipe)
            {
                BeginShaderMode(Resources.ScreenTransitionShader);
                {
                    DrawRectangle(0, 0, 320, 240, Color.Black);
                }
                EndShaderMode();
            }

            if (game.StateAutomaton.CurrentState == game.BattlePlayerAiming)
            {
                DrawCircle(
                    (int)(160 + (game.CurrentPlayerAimingStateContext.CurrentAimValue * 160)),
                    120,
                    8,
                    game.CurrentPlayerAimingStateContext.IsInRange()
                        ? Color.Green
                        : Color.RayWhite);
            }

            for (int i = 0; i < game.Log.Lines.Count; i++)
            {
                TextDraw(
                    Resources.Font,
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
                    Resources.Font,
                    game.CurrentDialogStateContext!.RunningLine.ToString(),
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
            BeginShaderMode(Resources.DownmixedShader);
            {
                SetShaderValue(Resources.DownmixedShader, downmixedShaderLUTLoc, Resources.LUTTexture);

                SetShaderValue(
                    Resources.DownmixedShader,
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
                        Width = GetScreenWidth(),
                        Height = GetScreenHeight(),
                        Position = Vector2.Zero,
                    },
                    Vector2.Zero,
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

Resources.UnloadAll();
CloseAudioDevice();
CloseWindow();
