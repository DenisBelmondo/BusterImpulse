using System.Numerics;
using Belmondo;
using Raylib_cs.BleedingEdge;
using static Belmondo.FightFightDanger;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;

static Vector2 Flatten(Vector3 v) => new(v.X, v.Z);
static Vector3 Make3D(Vector2 v) => new(v.X, 0, v.Y);

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

Log log = new(8);
RaylibJukebox jukebox = new();

static void TextDraw(Font font, string text, Vector2 screenSize, Vector2 position)
{
    var scaleFactor = screenSize.Y / 240f;
    var scaledFontSize = 15 * scaleFactor;

    DrawTextEx(font, text, position + Vector2.One * scaleFactor, scaledFontSize, 1, Color.Black);
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
    RenderTexture2D renderTexture = LoadRenderTexture(640, 480);

    double oldTime = GetTime();
    double delta = 0;

    State? playState = null;
    State? battleStart = null;
    State? battleStartAttack = null;
    State? battleAttack = null;
    State? battleDefend = null;
    State? battleRun = null;
    State? battleEnemyAttack = null;

    playState = new()
    {
        EnterFunction = () =>
        {
            log.Clear();
            jukebox.ChangeMusic(Resources.Music);
        },
        UpdateFunction = () =>
        {
            if (IsKeyPressed(KeyboardKey.B))
            {
                return new(() => battleStart);
            }

            TickPlayer(delta);

            return new(() => null);
        }
    };

    battleStart = new()
    {
        EnterFunction = () =>
        {
            jukebox.ChangeMusic(Resources.BattleMusic);
            log.Clear();
            log.Add("What will you do?");
            log.Add("A: Attack");
            log.Add("D: Defend");
            log.Add("R: Run");
        },
        UpdateFunction = () =>
        {
            while (true)
            {
                var key = GetKeyPressed();

                if (key == 0)
                {
                    break;
                }

                switch (key)
                {
                case KeyboardKey.A:
                    return new(() => battleStartAttack);
                case KeyboardKey.D:
                    return new(() => battleDefend);
                case KeyboardKey.R:
                    return new(() => battleRun);
                }
            }

            return new(() => null);
        },
    };

    battleStartAttack = new()
    {
        EnterFunction = () =>
        {
            log.Clear();
            log.Add("Player attacks!");
        },
        UpdateFunction = () =>
        {
            if (IsKeyPressed(KeyboardKey.Enter))
            {
                return new(() => battleAttack);
            }

            return new(() => null);
        }
    };

    battleAttack = new()
    {
        EnterFunction = () =>
        {
            log.Add("Player deals 10 damage!");
        },
        UpdateFunction = () =>
        {
            if (IsKeyPressed(KeyboardKey.Enter))
            {
                return new(() => battleEnemyAttack);
            }

            return new(() => null);
        },
    };

    battleDefend = new()
    {
        EnterFunction = () =>
        {
            log.Add("\tPlayer defends!");
        },
        UpdateFunction = () =>
        {
            return new(() => battleEnemyAttack);
        }
    };

    battleRun = new()
    {
        EnterFunction = () =>
        {
            Console.WriteLine("\tPlayer runs!");
        },
        UpdateFunction = () =>
        {
            return new(() => playState);
        }
    };

    battleEnemyAttack = new()
    {
        EnterFunction = () =>
        {
            Console.WriteLine("\tEnemy attacks!");
            Console.WriteLine("\tEnemy deals 5 damage!");
        },
        UpdateFunction = () =>
        {
            return new(() => battleStart);
        }
    };

    stateAutomaton.CurrentState = playState;

    while (!WindowShouldClose())
    {
        jukebox.Update();

        double newTime = GetTime();

        delta = newTime - oldTime;
        oldTime = newTime;

        Update(delta);

        // render

        BeginTextureMode(renderTexture);
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

                // [FIXME]: BRUTAL fucking hack
                if (stateAutomaton.CurrentState == battleStart || stateAutomaton.CurrentState == battleStartAttack || stateAutomaton.CurrentState == battleAttack)
                {
                    (float X, float Y) = Direction.ToInt32Tuple(world.Player.Entity.Direction);

                    X /= 2;
                    Y /= 2;
                    X += world.Player.Entity.X;
                    Y += world.Player.Entity.Y;

                    Rlgl.DisableDepthTest();
                    {
                        DrawBillboard(camera, Resources.EnemyTexture, Make3D(new(X, Y)), 1, Color.White);
                    }
                }
            }
            EndMode3D();

            Rectangle boxRect = new()
            {
                X = 0,
                Y = 0,
                Width = 128,
                Height = 128,
            };

            for (int i = 0; i < log.Lines.Count; i++)
            {
                TextDraw(Resources.Font, log.Lines[i], new(renderTexture.Texture.Width, renderTexture.Texture.Height), new(0, i * 15 * GetScreenHeight() / 240f));
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
