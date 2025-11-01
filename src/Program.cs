using System.Numerics;
using Belmondo;
using Raylib_cs.BleedingEdge;
using static Belmondo.FightFightDanger;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;

static Vector2 Flatten(Vector3 v) => new(v.X, v.Z);
static Vector3 Make3D(Vector2 v) => new(v.X, 0, v.Y);

string currentDialogLine = string.Empty;

World world = new();

world.TileMap = new int[,]
{
    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, },
    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
};

world.Player.Entity.X = 5;
world.Player.Entity.Y = 5;

float oldPlayerX = 0;
float oldPlayerY = 0;
int oldPlayerDirection = 0;
float cameraPositionLerpT = 0;
float cameraDirectionLerpT = 0;

StateAutomaton stateAutomaton = new();

Camera3D camera = new()
{
    FovY = 90,
    Projection = CameraProjection.Perspective,
    Up = Vector3.UnitY,
};

camera.Position.X = world.Player.Entity.X;
camera.Position.Z = world.Player.Entity.Y;

Vector3 cameraDirection;

static void TextDraw(Font font, string text, Vector2 position)
{
    var scaleFactor = GetScreenHeight() / 240f;
    var scaledFontSize = 15 * scaleFactor;

    DrawTextEx(font, text, position + Vector2.One * scaleFactor, scaledFontSize, 1, Color.DarkBlue);
    DrawTextEx(font, text, position, scaledFontSize, 1, Color.White);
}

void TickPlayer(double delta)
{
    // begin player update

    if (IsKeyPressed(KeyboardKey.Right))
    {
        oldPlayerDirection = world.Player.Entity.Direction;
        cameraDirectionLerpT = 0;
        world.Player.Entity.Direction++;
    }
    else if (IsKeyPressed(KeyboardKey.Left))
    {
        oldPlayerDirection = world.Player.Entity.Direction;
        cameraDirectionLerpT = 0;
        world.Player.Entity.Direction--;
    }

    world.Player.Entity.Direction = Direction.Clamped(world.Player.Entity.Direction);
    cameraDirectionLerpT = MathF.Min(cameraDirectionLerpT + (float)delta, 1);

    int? moveDirection = null;

    if (IsKeyDown(KeyboardKey.W))
    {
        moveDirection = Direction.Clamped(world.Player.Entity.Direction);
    }
    else if (IsKeyDown(KeyboardKey.S))
    {
        moveDirection = Direction.Clamped(world.Player.Entity.Direction + 2);
    }
    else if (IsKeyDown(KeyboardKey.A))
    {
        moveDirection = Direction.Clamped(world.Player.Entity.Direction + 3);
    }
    else if (IsKeyDown(KeyboardKey.D))
    {
        moveDirection = Direction.Clamped(world.Player.Entity.Direction + 1);
    }

    if (moveDirection is not null && world.Player.Current.WalkCooldown == 0)
    {
        cameraPositionLerpT = 0;
        oldPlayerX = world.Player.Entity.X;
        oldPlayerY = world.Player.Entity.Y;
        PlaySound(Resources.StepSound);
        world.Player.Current.WalkCooldown = world.Player.Default.WalkCooldown;
        world.TryMove(ref world.Player.Entity, (int)moveDirection);
    }

    world.Player.Current.WalkCooldown = Math.Max(world.Player.Current.WalkCooldown - delta, 0);
    cameraPositionLerpT = MathF.Min(cameraPositionLerpT + ((1.0F / (float)world.Player.Default.WalkCooldown) * (float)delta), 1);

    // end player update
}

void Update(double delta)
{
    stateAutomaton.Update();
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
    PlayMusicStream(Resources.Music);

    double oldTime = GetTime();
    double delta = 0;

    State playState;
    State battleStart;
    State battleAttack;

    playState = new()
    {
        UpdateFunction = () =>
        {
            TickPlayer(delta);
            return null;
        },
    };

    battleStart = new()
    {
        EnterFunction = () =>
        {
            Console.WriteLine("A: attack");
        },
        UpdateFunction = () =>
        {
            if (IsKeyDown(KeyboardKey.A))
            {
                return battleAttack;
            }

            return null;
        },
    };

    battleAttack = new()
    {
    };

    stateAutomaton.CurrentState = playState;

    while (!WindowShouldClose())
    {
        UpdateMusicStream(Resources.Music);

        double newTime = GetTime();

        delta = newTime - oldTime;
        oldTime = newTime;

        Update(delta);

        // render

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            Vector3 playerDirection3d;

            {
                var (X, Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);
                playerDirection3d = new(X, 0, Y);
            }

            camera.Position = Vector3.Lerp(new(oldPlayerX, 0, oldPlayerY), new(world.Player.Entity.X, 0, world.Player.Entity.Y), cameraPositionLerpT);

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

                DrawBillboard(camera, Resources.EnemyTexture, Make3D(new(5, 5)), 1, Color.White);
            }
            EndMode3D();

            TextDraw(Resources.Font, currentDialogLine, Vector2.Zero);
        }
        EndDrawing();
    }
}

Resources.UnloadAll();
CloseAudioDevice();
CloseWindow();
