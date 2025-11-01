using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo;

public static partial class FightFightDanger
{
    public static class Resources
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

        // All components are in the range [0...1], including hue.
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
        public static Texture2D ChestAtlas;
        public static Texture2D EnemyTexture;
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
            ChestAtlas = LoadTexture("static/textures/chest-wooden-0.png");
            EnemyTexture = LoadTexture("static/textures/enemy.png");
            TileMaterial = LoadMaterialDefault();
            FloorMaterial = LoadMaterialDefault();
            TileMesh = GenMeshCube(1, 1, 1);
            TileModel = LoadModelFromMesh(TileMesh);
            PlaneMesh = GenMeshPlane(1000, 1000, 1, 1);
            FloorModel = LoadModelFromMesh(PlaneMesh);
            CeilingModel = LoadModelFromMesh(PlaneMesh);
            Music = LoadMusicStream("static/music/ronde.mp3");
            Font = LoadFont("static/fonts/pixel-font-15.png");
            StepSound = LoadSound("static/sounds/step.wav");

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
            UnloadTexture(EnemyTexture);
            UnloadTexture(ChestAtlas);
            UnloadImage(TileTextureImage);
            UnloadModel(TileModel);
            UnloadModel(FloorModel);
            UnloadShader(SurfaceShader);
        }
    }
}
