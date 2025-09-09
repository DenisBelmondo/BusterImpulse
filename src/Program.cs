using Raylib_cs.BleedingEdge;
using System.Numerics;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;

static IVector2 DirectionToIVector2(Direction direction)
{
    switch (direction)
    {
    case Direction.North:
        return new(0, -1);
    case Direction.East:
        return new(-1, 0);
    case Direction.South:
        return new(0, 1);
    case Direction.West:
        return new(1, 0);
    }

    return new(0, 0);
}

var world = new World
{
    Player = new(),
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

Camera3D camera = new()
{
    FovY = 90,
    Projection = CameraProjection.Perspective,
    Up = new(0, 1, 0),
};

camera.Position = new(world.Player.Position.X, 0, world.Player.Position.Y);
Vector3 cameraDirection = Vector3RotateByAxisAngle(new(0, 0, -1), Vector3.UnitY, Pi * (int)world.Player.Direction / 2);

SetConfigFlags(ConfigFlags.WindowResizable);
InitWindow(640, 480, "Buster Impulse");
{
    Mesh tileMesh = GenMeshCube(1, 1, 1);
    Image testImage = LoadImage("static/textures/test.png");

    ImageFlipVertical(ref testImage);

    Texture2D testTexture = LoadTextureFromImage(testImage);
    Model tileModel = LoadModelFromMesh(tileMesh);

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

        Update(delta);

        //
        // render
        //

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            var cameraPosition = Vector3.Lerp(camera.Position, new(world.Player.Position.X, 0, world.Player.Position.Y), 10 * (float)delta);
            var cameraYaw = Vector3SignedAngle(cameraDirection, Vector3RotateByAxisAngle(new(0, 0, -1), Vector3.UnitY, Pi * (int)world.Player.Direction / 2), Vector3.UnitY) * 5 * (float)delta;

            cameraDirection = Vector3RotateByQuaternion(cameraDirection, Quaternion.CreateFromYawPitchRoll(cameraYaw, 0, 0));
            camera.Position = cameraPosition;
            camera.Target = camera.Position + cameraDirection;

            BeginMode3D(camera);
            {
                for (var y = 0; y < world.TileMap.GetLength(0); y++)
                {
                    for (var x = 0; x < world.TileMap.GetLength(1); x++)
                    {
                        if (world.TileMap[x, y] != 0)
                        {
                            DrawModel(tileModel, new(x, 0, y), 1, Color.White);
                        }
                    }
                }
            }
            EndMode3D();
        }
        EndDrawing();
    }

    UnloadModel(tileModel);
    UnloadImage(testImage);
}
CloseWindow();

static float Vector3SignedAngle(Vector3 Va, Vector3 Vb, Vector3 Vn)
{
    var angle = MathF.Acos(Vector3.Dot(Vector3.Normalize(Va), Vector3.Normalize(Vb)));
    var cross = Vector3.Cross(Va, Vb);

    angle = angle * MathF.Sign(Vector3.Dot(Vn, cross));

    return angle;
}

void Update(double delta)
{
    var forwardIsPressed = false;
    var backwardIsPressed = false;
    var strafeLeftIsPressed = false;
    var strafeRightIsPressed = false;
    var lookRightIsPressed = false;
    var lookLeftIsPressed = false;

    for (int intKey = (int)GetKeyPressed(); intKey != 0; intKey = (int)GetKeyPressed())
    {
        KeyboardKey key = (KeyboardKey)intKey;

        forwardIsPressed |= key == KeyboardKey.W;
        backwardIsPressed |= key == KeyboardKey.S;
        strafeLeftIsPressed |= key == KeyboardKey.A;
        strafeRightIsPressed |= key == KeyboardKey.D;
        lookRightIsPressed |= key == KeyboardKey.Right;
        lookLeftIsPressed |= key == KeyboardKey.Left;
    }

    var turnAxis = Convert.ToInt32(lookRightIsPressed) - Convert.ToInt32(lookLeftIsPressed);
    var desiredDirection = ((int)world.Player.Direction) - turnAxis;

    desiredDirection %= Enum.GetValues<Direction>().Length;

    world.Player.Direction = (Direction)desiredDirection;

    var hasMoved = forwardIsPressed || backwardIsPressed || strafeLeftIsPressed || strafeRightIsPressed;
    var forwardAxis = Convert.ToInt32(forwardIsPressed) - Convert.ToInt32(backwardIsPressed);
    var strafeAxis = Convert.ToInt32(strafeRightIsPressed) - Convert.ToInt32(strafeLeftIsPressed);
    IVector2 move;

    move = DirectionToIVector2(world.Player.Direction) * forwardAxis;
    // move += new Vector2(-world.Player.Direction.Y, world.Player.Direction.X) * strafeAxis;

    var desiredPosition = world.Player.Position + move;

    if (world.TileMap[((int)desiredPosition.X), ((int)desiredPosition.Y)] == 0)
    {
        world.Player.Position = desiredPosition;
    }

    //
    // world state queue (remove later)
    //

    do
    {
        var shouldDequeue = false;

        do
        {
            if (world.TaskQueue.TryPeek(out var node))
            {
                if (!node.WasEntered)
                {
                    node.WasEntered = node.Enter?.Invoke() ?? false;
                }

                if (!node.WasEntered)
                {
                    shouldDequeue = true;
                    break;
                }

                if (node.Update.Invoke(delta))
                {
                    shouldDequeue = true;
                    break;
                }
            }
        } while (false);

        if (shouldDequeue)
        {
            world.TaskQueue.Dequeue().Exit?.Invoke();
        }
    } while (false);
}

struct IVector2(int x, int y)
{
    public int X = x;
    public int Y = y;

    public static IVector2 operator +(IVector2 v1, IVector2 v2) => new(v1.X + v2.X, v1.Y + v2.Y);
    public static IVector2 operator -(IVector2 v1, IVector2 v2) => new(v1.X - v2.X, v1.Y - v2.Y);
    public static IVector2 operator *(IVector2 v1, IVector2 v2) => new(v1.X * v2.X, v1.Y * v2.Y);
    public static IVector2 operator /(IVector2 v1, IVector2 v2) => new(v1.X / v2.X, v1.Y / v2.Y);
    public static IVector2 operator *(IVector2 v2, int v) => new(v2.X * v, v2.Y * v);
    public static IVector2 operator /(IVector2 v2, int v) => new(v2.X / v, v2.Y / v);
}

public enum Direction
{
    North,
    East,
    South,
    West,
}

struct Entity()
{
    public IVector2 Position;
    public Direction Direction;
}

struct World()
{
    public class TaskQueueNode
    {
        public required Func<double, bool> Update;
        public Func<bool>? Enter;
        public Func<bool>? Exit;
        public bool WasEntered;
    }

    public Entity Player;
    public int[,] TileMap;
    public Queue<TaskQueueNode> TaskQueue = [];
}
