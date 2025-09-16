using System.Numerics;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;
using static Raylib_cs.BleedingEdge.Raymath;

using Position = (int X, int Y);

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

camera.Position.X = world.Player.X;
camera.Position.Z = world.Player.Y;

Vector3 cameraDirection;

{
    var (X, Y) = Direction.ToInt32Tuple(world.Player.Direction);
    cameraDirection = new(X, 0, Y);
}

SetConfigFlags(ConfigFlags.WindowResizable);

InitWindow(1024, 768, "Buster Impulse");
InitAudioDevice();
Resources.CacheAndInitializeAll();

{
    PlayMusicStream(Resources.Music);

    double oldTime = GetTime();

    while (!WindowShouldClose())
    {
        UpdateMusicStream(Resources.Music);

        double newTime = GetTime();
        double delta = newTime - oldTime;

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
                            DrawModel(Resources.TileModel, new(x, 0, y), 1, Color.White);
                        }
                    }
                }
            }
            EndMode3D();

            DrawText("100+", 16, 16, 40, Color.White);
        }
        EndDrawing();
    }
}

Resources.UnloadAll();
CloseAudioDevice();
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

    public static Position ToInt32Tuple(int direction) => direction switch
    {
        North => (0, -1),
        East => (1, 0),
        South => (0, 1),
        West => (-1, 0),
        _ => (0, 0),
    };
}

static class Math2
{
    public static float Lerp(float v0, float v1, float t) => (1 - t) * v0 + t * v1;

    public static float SignedAngleTo(this Vector3 v1, Vector3 v2, Vector3 up)
    {
        float angle = MathF.Acos(Vector3.Dot(Vector3.Normalize(v1), Vector3.Normalize(v2)));
        var cross = Vector3.Cross(v1, v2);

        if (Vector3.Dot(up, cross) < 0)
        {
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
    public Dictionary<Position, HashSet<int>> BroadPhaseCollisionMap = [];
    public Entity Player = new();

    public readonly void Spawn(int what, Position at) => BroadPhaseCollisionMap[at].Add(what);

    public readonly bool TryMove(ref Entity entity, int direction)
    {
        (int X, int Y) = Direction.ToInt32Tuple(direction);

        int desiredX = entity.X + X;
        int desiredY = entity.Y + Y;

        if (TileMap[entity.Y, desiredX] != 0 || TileMap[desiredY, entity.X] != 0)
        {
            return false;
        }

        entity.X = desiredX;
        entity.Y = desiredY;

        return true;
    }
}

static class Resources
{
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
        vec2 uv = vertexTexCoord;
        vec3 abs_normal = abs(vertexNormal);

        // main triplanar calcs
        uv  = mix(vertexPosition.xy, vertexPosition.zy, round(abs_normal.x));
        uv = mix(uv, vertexPosition.xz, round(abs_normal.y));
        uv += vec2(0.5, 0.5);

        // prevent flipping
        uv.x *= sign(dot(vertexNormal, vec3(-1, 1, 1)));
        uv.y *= sign(dot(abs_normal, vec3(-1, 1, -1)));
        uv.y *= -1;

        // Send vertex attributes to fragment shader
        fragTexCoord = uv;
        // fragTexCoord = vertexTexCoord;
        fragColor = vertexColor;
        fragNormal = vertexNormal;

        // Calculate final vertex position
        gl_Position = mvp * vec4(vertexPosition, 1.0);
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

    // All components are in the range [0…1], including hue.
    vec3 rgb2hsv(vec3 c)
    {
        vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
        vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
        vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

        float d = q.x - min(q.w, q.y);
        float e = 1.0e-10;
        return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
    }

    // All components are in the range [0…1], including hue.
    vec3 hsv2rgb(vec3 c)
    {
        vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
        vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
        return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
    }

    // NOTE: Add your custom variables here

    void main()
    {
        // Texel color fetching from texture sampler
        vec4 texelColor = texture(texture0, fragTexCoord);

        // NOTE: Implement here your fragment shader code

        // final color is the color from the texture
        //    times the tint color (colDiffuse)
        //    times the fragment color (interpolated vertex color)
        vec4 rgba = texelColor * colDiffuse * fragColor;
        vec3 hsv = rgb2hsv(rgba.rgb);

        hsv.z *= abs(dot(vec3(0.5, 1.0, 1.0), fragNormal));
        rgba.rgb = hsv2rgb(hsv);
        finalColor = rgba;
    }
    """;

    public static Shader SurfaceShader;
    public static Image TileTextureImage;
    public static Texture2D TileTexture;
    public static Texture2D FloorTexture;
    public static Texture2D CeilingTexture;
    public static Material TileMaterial;
    public static Material FloorMaterial;
    public static Mesh TileMesh;
    public static Mesh PlaneMesh;
    public static Model TileModel;
    public static Model FloorModel;
    public static Model CeilingModel;
    public static Sound StepSound;
    public static Music Music;
    public static Font Font;

    public static void CacheAndInitializeAll()
    {
        SurfaceShader = LoadShaderFromMemory(VERTEX_SHADER_SOURCE, FRAGMENT_SHADER_SOURCE);
        TileTextureImage = LoadImage("static/textures/cobolt-stone-0-moss-0.png");
        ImageFlipVertical(ref TileTextureImage);
        TileTexture = LoadTextureFromImage(TileTextureImage);
        FloorTexture = LoadTexture("static/textures/cobolt-stone-1-floor-0.png");
        CeilingTexture = LoadTexture("static/textures/cobolt-stone-0-floor-0.png");
        TileMaterial = LoadMaterialDefault();
        FloorMaterial = LoadMaterialDefault();
        TileMesh = GenMeshCube(1, 1, 1);
        TileModel = LoadModelFromMesh(TileMesh);
        PlaneMesh = GenMeshPlane(1000, 1000, 1, 1);
        FloorModel = LoadModelFromMesh(PlaneMesh);
        CeilingModel = LoadModelFromMesh(PlaneMesh);
        Music = LoadMusicStream("static/music/black-dawn.mp3");
        Font = LoadFont("static/fonts/proggy-clean.ttf");

        unsafe
        {
            TileModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = TileTexture;
            TileModel.Materials[0].Shader = SurfaceShader;
            FloorModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = FloorTexture;
            FloorModel.Materials[0].Shader = SurfaceShader;
            CeilingModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = CeilingTexture;
            CeilingModel.Materials[0].Shader = SurfaceShader;
        }
    }

    public static void UnloadAll()
    {
        UnloadFont(Font);
        UnloadSound(StepSound);
        UnloadMusicStream(Music);
        UnloadImage(TileTextureImage);
        UnloadModel(TileModel);
        UnloadModel(FloorModel);
        UnloadShader(SurfaceShader);
    }
}
