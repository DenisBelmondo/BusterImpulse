using System.Numerics;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;

const string VERTEX_SHADER_SOURCE =
"""
#version 330

// Input vertex attributes
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

// Input uniform values
uniform mat4 mvp;

// Output vertex attributes (to fragment shader)
out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;

// NOTE: Add your custom variables here

void main()
{
    // Send vertex attributes to fragment shader
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    fragNormal = vertexNormal;

    // Calculate final vertex position
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}
""";

const string FRAGMENT_SHADER_SOURCE =
"""
#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add your custom variables here

void main()
{
    // Texel color fetching from texture sampler
    vec4 texelColor = texture(texture0, fragTexCoord);

    // NOTE: Implement here your fragment shader code

    // final color is the color from the texture
    //    times the tint color (colDiffuse)
    //    times the fragment color (interpolated vertex color)
    finalColor = texelColor*colDiffuse*fragColor;
    finalColor.rgb *= abs(dot(vec3(0.5, 1.0, 1.0), fragNormal));
}
""";

World world = new();

world.TileMap = new int[,]
{
    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
    { 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, },
    { 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, },
    { 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, },
    { 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, },
    { 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, },
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

Vector3 cameraDirection;

{
    var (X, Y) = Direction.ToInt32Tuple(world.Player.Direction);
    cameraDirection = new(X, 0, Y);
}

Shader tileShader;
Image testTextureImage;
Texture2D testTexture;
Material tileMaterial;
Mesh tileMesh;
Model tileModel;

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Buster Impulse");
{
    tileShader = LoadShaderFromMemory(VERTEX_SHADER_SOURCE, FRAGMENT_SHADER_SOURCE);
    testTextureImage = LoadImage("static/textures/test.png");
    ImageFlipVertical(ref testTextureImage);
    testTexture = LoadTextureFromImage(testTextureImage);
    tileMaterial = LoadMaterialDefault();
    tileMesh = GenMeshCube(1, 1, 1);
    tileModel = LoadModelFromMesh(tileMesh);

    unsafe
    {
        tileModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = testTexture;
        tileModel.Materials[0].Shader = tileShader;
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
            world.Player.Direction++;
        }
        else if (IsKeyPressed(KeyboardKey.Left))
        {
            world.Player.Direction--;
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
            moveDirection = Direction.Clamped(world.Player.Direction + 3);
        }
        else if (IsKeyPressed(KeyboardKey.D))
        {
            moveDirection = Direction.Clamped(world.Player.Direction + 1);
        }

        if (moveDirection is not null)
        {
            world.TryMove(ref world.Player, (int)moveDirection);
        }

        // render

        BeginDrawing();
        {
            ClearBackground(Color.Black);

            Vector3 playerDirection3d;

            {
                var (X, Y) = Direction.ToInt32Tuple(world.Player.Direction);
                playerDirection3d = new(X, 0, Y);
            }

            camera.Position = Vector3Lerp(camera.Position, new(world.Player.X, 0, world.Player.Y), 10 * (float)delta);

            var cameraRotation = QuaternionFromAxisAngle(Vector3.UnitY, cameraDirection.SignedAngleTo(playerDirection3d, Vector3.UnitY) * 10 * (float)delta);

            cameraDirection = Vector3RotateByQuaternion(cameraDirection, cameraRotation);
            camera.Target = camera.Position + cameraDirection;

            BeginMode3D(camera);
            {
                DrawPlane(Vector3.UnitY * -1, Vector2.One * 1000, Color.Gray);

                for (var y = 0; y < world.TileMap.GetLength(0); y++)
                {
                    for (var x = 0; x < world.TileMap.GetLength(1); x++)
                    {
                        if (world.TileMap[y, x] != 0)
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

    UnloadImage(testTextureImage);
    UnloadModel(tileModel);
    UnloadShader(tileShader);
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

    public static (int X, int Y) ToInt32Tuple(int direction) => direction switch
    {
        Direction.North => (0, -1),
        Direction.East => (1, 0),
        Direction.South => (0, 1),
        Direction.West => (-1, 0),
        _ => (0, 0),
    };
}

static class Math2
{
    public static float Lerp(float v0, float v1, float t) => (1 - t) * v0 + t * v1;

    public static float SignedAngleTo(this Vector3 v1, Vector3 v2, Vector3 up)
    {
        var angle = MathF.Acos(Vector3.Dot(Vector3.Normalize(v1), Vector3.Normalize(v2)));
        var cross = Vector3.Cross(v1, v2);

        if (Vector3.Dot(up, cross) < 0)
        { // Or > 0
            angle = -angle;
        }

        return angle;
    }
}

class Tween<TValue> where TValue : struct
{
    public enum Status
    {
        Uninitialized,
        Initialized,
        Running,
        Finished,
    }

    public struct State
    {
        public TValue From;
        public TValue To;
        public float CurrentT;
        public Status CurrentStatus;
        public TValue CurrentValue;
    }

    public required Func<TValue, TValue, float, TValue> LerpFunction;
    public State CurrentState;

    public void Initialize(TValue from, TValue to) => CurrentState = new()
    {
        From = from,
        To = to,
    };

    public void Update(double deltaTime)
    {
        if (CurrentState.CurrentStatus == Status.Running)
        {
            CurrentState.CurrentT += (float)deltaTime;
            CurrentState.CurrentT = Math.Clamp(CurrentState.CurrentT, 0, 1);
            CurrentState.CurrentValue = LerpFunction(CurrentState.From, CurrentState.To, CurrentState.CurrentT);

            if (CurrentState.CurrentT >= 1)
            {
                CurrentState.CurrentStatus = Status.Finished;
            }
        }
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
        (int X, int Y) = Direction.ToInt32Tuple(direction);

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
