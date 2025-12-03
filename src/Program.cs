using System.Numerics;
using Belmondo;
using Raylib_cs.BleedingEdge;
using static Belmondo.FightFightDanger;
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
    { 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
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
    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
};

world.Player.Entity.X = 5;
world.Player.Entity.Y = 5;

game.World = world;

Camera3D camera = new()
{
    FovY = 90,
    Projection = CameraProjection.Perspective,
    Up = Vector3.UnitY,
};

camera.Position.X = world.Player.Entity.X;
camera.Position.Z = world.Player.Entity.Y;

Vector3 cameraDirection;

static void TextDraw(Font font, string text, Vector2 screenSize, Vector2 position)
{
    var scaleFactor = screenSize.Y / 240f;
    var scaledFontSize = 15 * scaleFactor;

    DrawTextEx(font, text, position + Vector2.One * scaleFactor, scaledFontSize, 1, Color.Black);
    DrawTextEx(font, text, position, scaledFontSize, 1, Color.White);
}

{
    var (X, Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);
    cameraDirection = new(X, 0, Y);
}

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Buster Impulse");
InitAudioDevice();
Resources.CacheAndInitializeAll();

{
    RenderTexture2D renderTexture = LoadRenderTexture(640, 480);

    double oldTime = GetTime();

    //
    // begin state initializations
    //

    //
    // end state initializations
    //

    game.StateAutomaton.CurrentState = game.ExploreState;

    while (!WindowShouldClose())
    {
        game.Jukebox.Update();

        double newTime = GetTime();
        double delta = newTime - oldTime;

        oldTime = newTime;

        game.TimeContext.Delta = delta;
        game.TimeContext.Time += delta;
        game.Update();

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

            camera.Position = Vector3.Lerp(new(world.OldPlayerX, 0, world.OldPlayerY), new(world.Player.Entity.X, 0, world.Player.Entity.Y), world.CameraPositionLerpT);

            Quaternion cameraRotation = QuaternionFromAxisAngle(Vector3.UnitY, cameraDirection.SignedAngleTo(playerDirection3d, Vector3.UnitY) * 10 * (float)delta);

            cameraDirection = Vector3RotateByQuaternion(cameraDirection, cameraRotation);
            camera.Target = camera.Position + cameraDirection;

            BeginMode3D(camera);
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

                // [FIXME]: BRUTAL fucking hack
                var isInBattle = false
                    || game.StateAutomaton.CurrentState == game.BattleStart
                    || game.StateAutomaton.CurrentState == game.BattleStartPlayerAttack
                    || game.StateAutomaton.CurrentState == game.BattlePlayerAiming
                    || game.StateAutomaton.CurrentState == game.BattlePlayerMissed
                    || game.StateAutomaton.CurrentState == game.BattleEnemyStartAttack
                    || game.StateAutomaton.CurrentState == game.BattleEnemyAttack
                    || game.StateAutomaton.CurrentState == game.BattlePlayerAttack;

                if (isInBattle)
                {
                    (float X, float Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);

                    X /= 2;
                    Y /= 2;
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

            BeginShaderMode(Resources.PlasmaShader);
            {
                DrawRectangle(0, 0, 640, 480, Color.White);
            }
            EndShaderMode();

            if (game.StateAutomaton.CurrentState == game.BattlePlayerAiming)
            {
                DrawCircle((int)(320 + (game.CurrentPlayerAimingStateContext.CurrentAimValue * 320)), 240, 24, game.CurrentPlayerAimingStateContext.IsInRange() ? Color.Green : Color.RayWhite);
            }

            for (int i = 0; i < game.Log.Lines.Count; i++)
            {
                TextDraw(Resources.Font, game.Log.Lines[i], new(renderTexture.Texture.Width, renderTexture.Texture.Height), new(0, i * 15 * GetScreenHeight() / 240f));
            }
        }
        EndTextureMode();

        BeginDrawing();
        {
            DrawTexturePro(
                renderTexture.Texture,
                new Rectangle()
                {
                    Width = 640,
                    Height = -480,
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
        EndDrawing();
    }

    UnloadRenderTexture(renderTexture);
}

Resources.UnloadAll();
CloseAudioDevice();
CloseWindow();
