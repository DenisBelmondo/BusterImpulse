using System.Numerics;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

var world = new World
{
    TileMap = new int[,] {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    },
};

world.Player.Position = new(5, 5);

var camera = new Camera3D()
{
    FovY = 90,
    Projection = CameraProjection.Perspective,
    Up = new(0, 1, 0),
};

SetConfigFlags(ConfigFlags.WindowResizable);
InitWindow(640, 480, "Buster Impulse");
{
    Mesh tileMesh = GenMeshCube(1, 1, 1);
    Texture2D testTexture = LoadTexture("static/textures/test.png");
    Material tileMaterial = LoadMaterialDefault();

    unsafe
    {
        tileMaterial.Maps[(int)MaterialMapIndex.Diffuse].Texture = testTexture;
    }

    while (!WindowShouldClose())
    {
        var keyPressedFunction = IsKeyPressed;
        var forwardAxis = Convert.ToSingle((bool)keyPressedFunction(KeyboardKey.W)) - Convert.ToSingle((bool)keyPressedFunction(KeyboardKey.S));
        var strafeAxis = Convert.ToSingle((bool)keyPressedFunction(KeyboardKey.A)) - Convert.ToSingle((bool)keyPressedFunction(KeyboardKey.D));
        Vector2 move;

        move = world.Player.Direction * forwardAxis;
        move += new Vector2(-world.Player.Direction.Y, world.Player.Direction.X) * -strafeAxis;
        world.Player.Position += move;

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            camera.Position = new(world.Player.Position.X, 0, world.Player.Position.Y);
            camera.Target = camera.Position + new Vector3(world.Player.Direction.X, 0, world.Player.Direction.Y);

            BeginMode3D(camera);
            {
                for (var y = 0; y < world.TileMap.GetLength(0); y++)
                {
                    for (var x = 0; x < world.TileMap.GetLength(1); x++)
                    {
                        if (world.TileMap[x, y] != 0)
                        {
                            DrawMesh(tileMesh, tileMaterial, Matrix4x4.CreateWorld(new(x, 0, y), new(0, 0, -1), new(0, 1, 0)));
                        }
                    }
                }
            }
            EndMode3D();
        }
        EndDrawing();
    }

    UnloadMesh(tileMesh);
    UnloadMaterial(tileMaterial);
}
CloseWindow();

struct Entity
{
    public Vector2 Position;
    public Vector2 Direction = new(0, -1);

    public Entity() { }
}

struct World
{
    public int[,]? TileMap;
    public Entity Player = new();

    public World() { }
}
