using System.Numerics;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

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

world.Player.X = 5;
world.Player.Y = 5;

Camera3D camera = new()
{
    FovY = 90,
    Projection = CameraProjection.Perspective,
    Up = Vector3.UnitY,
};

Image testTextureImage;
Texture2D testTexture;
Material tileMaterial;
Mesh tileMesh;
Model tileModel;

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Buster Impulse");
{
    testTextureImage = LoadImage("static/textures/test.png");
    ImageFlipVertical(ref testTextureImage);
    testTexture = LoadTextureFromImage(testTextureImage);
    tileMaterial = LoadMaterialDefault();
    tileMesh = GenMeshCube(1, 1, 1);
    tileModel = LoadModelFromMesh(tileMesh);

    unsafe
    {
        tileModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = testTexture;
    }

    var oldTime = GetTime();

    while (!WindowShouldClose())
    {
        var newTime = GetTime();
        var delta = newTime - oldTime;

        oldTime = newTime;

        //
        // update
        //

        if (IsKeyPressed(KeyboardKey.Right))
        {
            world.Player.Direction--;
        }
        else if (IsKeyPressed(KeyboardKey.Left))
        {
            world.Player.Direction++;
        }

        world.Player.Direction = Direction.Clamped(world.Player.Direction);

        int? moveDirection = null;

        if (IsKeyPressed(KeyboardKey.W))
        {
            moveDirection = Direction.Clamped(world.Player.Direction);
        }
        else if (IsKeyPressed(KeyboardKey.S))
        {
            moveDirection = Direction.Clamped(world.Player.Direction + 2);
        }
        else if (IsKeyPressed(KeyboardKey.A))
        {
            moveDirection = Direction.Clamped(world.Player.Direction + 1);
        }
        else if (IsKeyPressed(KeyboardKey.D))
        {
            moveDirection = Direction.Clamped(world.Player.Direction + 3);
        }

        if (moveDirection is not null)
        {
            world.TryMove(ref world.Player, (int)moveDirection);
        }

        // render

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            var cameraDirection = world.Player.Direction switch
            {
                Direction.North => new Vector3(0, 0, -1),
                Direction.East => new Vector3(-1, 0, 0),
                Direction.South => new Vector3(0, 0, 1),
                Direction.West => new Vector3(1, 0, 0),
                _ => Vector3.Zero,
            };

            //                   vvv fuck this shit*
            camera.Position = new(-world.Player.X, 0, world.Player.Y);
            camera.Target = camera.Position + cameraDirection;

            BeginMode3D(camera);
            {
                for (var y = 0; y < world.TileMap.GetLength(0); y++)
                {
                    for (var x = 0; x < world.TileMap.GetLength(1); x++)
                    {
                        if (world.TileMap[y, x] != 0)
                        {
                            // *ditto
                            DrawModel(tileModel, new(-x, 0, y), 1, Color.White);
                        }
                    }
                }
            }
            EndMode3D();
        }
        EndDrawing();
    }

    UnloadImage(testTextureImage);
    UnloadModel(tileModel);
}
CloseWindow();

static class Direction
{
    public const int North = 0;
    public const int East = 1;
    public const int South = 2;
    public const int West = 3;

    public static int Clamped(int direction)
    {
        if (direction < 0)
        {
            direction += 4;
        }

        return direction % 4;
    }
}

struct Entity
{
    public int X;
    public int Y;
    public int Direction;
}

struct World()
{
    public int[,] TileMap;
    public Entity Player = new();

    public readonly bool TryMove(ref Entity entity, int direction)
    {
        (int X, int Y) = direction switch
        {
            Direction.North => (0, -1),
            Direction.East => (1, 0),
            Direction.South => (0, 1),
            Direction.West => (-1, 0),
            _ => (0, 0),
        };

        var desiredX = entity.X + X;
        var desiredY = entity.Y + Y;

        if (TileMap[entity.Y, desiredX] != 0 || TileMap[desiredY, entity.X] != 0)
        {
            return false;
        }

        entity.X = desiredX;
        entity.Y = desiredY;

        return true;
    }
}
