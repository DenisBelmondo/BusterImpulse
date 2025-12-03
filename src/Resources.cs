using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo;

public static partial class FightFightDanger
{
    public static class Resources
    {
        private const string BASE_VERTEX_SHADER_SOURCE =
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

        // NOTE: Add your custom variables here

        void main()
        {
            // Send vertex attributes to fragment shader
            fragTexCoord = vertexTexCoord;
            fragColor = vertexColor;

            // Calculate final vertex position
            gl_Position = mvp*vec4(vertexPosition, 1.0);
        }
        """;

        private const string BASE_FRAGMENT_SHADER_SOURCE =
        """
        #version 330

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;

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
        }
        """;

        private const string SURFACE_VERTEX_SHADER_SOURCE =
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
            gl_Position = mvp * vec4(vertexPosition, 1.0); }
        """;

        private const string SURFACE_FRAGMENT_SHADER_SOURCE =
        """
        #version 330

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;
        in vec3 fragNormal;

        // Input uniform values
        uniform sampler2D texture0;
        uniform vec4 colDiffuse;

        float nearPlane = 0.1;
        float farPlane = 100.0;

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

            hsv.y *= abs(dot(vec3(1 / 0.6, 1.0, 1.0), fragNormal));
            hsv.z *= abs(dot(vec3(0.6, 1.0, 1.0), fragNormal));
            rgba.rgb = hsv2rgb(hsv);
            finalColor = rgba;
        }
        """;

        private const string PLASMA_FRAGMENT_SHADER_SOURCE =
        """
        #version 330

        in vec2 fragCoord;

        uniform float iTime;
        uniform ivec2 iResolution;

        out vec4 fragColor;

        void main()
        {
            float t = iTime*.3;

            // Normalized pixel coordinates (from 0 to 1)
            vec2 uv = (fragCoord-.5*iResolution.xy)/iResolution.y;
            float a = t*.5;
            float s=sin(a), c=cos(a);
            uv*=mat2(c,s,-s,c);
            uv += .1*sin(uv.yx*6.+t);

            uv = abs(uv);
            uv*=mat2(c,s,-s,c);

            vec3 col = 0.5 + 0.5*cos(t*1.4+uv.xyx*5.+vec3(0,2,4)) + .3*sin(uv.xxx*(1.1+.2*sin(t*.9))*20.+t*.4)+.3;

            // Output to screen
            fragColor = pow(vec4(col,1.0)*(1.-length(uv*1.6)),vec4(4));
            fragColor = vec4(1.0, 0, 0, 1.0);
        }
        """;

        public static Shader SurfaceShader;
        public static Shader PlasmaShader;
        public static Image TileTextureImage;
        public static Texture2D TileTexture;
        public static Texture2D FloorTexture;
        public static Texture2D CeilingTexture;
        public static Texture2D ChestAtlas;
        public static Texture2D EnemyTexture;
        public static Texture2D EnemyAtlas;
        public static Texture2D UIAtlas;
        public static Material TileMaterial;
        public static Material FloorMaterial;
        public static Mesh TileMesh;
        public static Mesh PlaneMesh;
        public static Model TileModel;
        public static Model FloorModel;
        public static Model CeilingModel;
        public static Sound StepSound;
        public static Sound SmackSound;
        public static Music Music;
        public static Music BattleMusic;
        public static Font Font;

        public static void CacheAndInitializeAll()
        {
            SurfaceShader = LoadShaderFromMemory(SURFACE_VERTEX_SHADER_SOURCE, SURFACE_FRAGMENT_SHADER_SOURCE);
            PlasmaShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, PLASMA_FRAGMENT_SHADER_SOURCE);
            TileTextureImage = LoadImage("static/textures/cobolt-stone-0-moss-0.png");
            ImageFlipVertical(ref TileTextureImage);
            TileTexture = LoadTextureFromImage(TileTextureImage);
            FloorTexture = LoadTexture("static/textures/cobolt-stone-1-floor-0.png");
            CeilingTexture = LoadTexture("static/textures/cobolt-stone-0-floor-0.png");
            ChestAtlas = LoadTexture("static/textures/chest-wooden-0.png");
            UIAtlas = LoadTexture("static/textures/ui.png");
            EnemyTexture = LoadTexture("static/textures/enemy.png");
            EnemyAtlas = LoadTexture("static/textures/enemy-atlas.png");
            TileMaterial = LoadMaterialDefault();
            FloorMaterial = LoadMaterialDefault();
            TileMesh = GenMeshCube(1, 1, 1);
            TileModel = LoadModelFromMesh(TileMesh);
            PlaneMesh = GenMeshPlane(1000, 1000, 1, 1);
            FloorModel = LoadModelFromMesh(PlaneMesh);
            CeilingModel = LoadModelFromMesh(PlaneMesh);
            Music = LoadMusicStream("static/music/ronde.mp3");
            BattleMusic = LoadMusicStream("static/music/morgan.mp3");
            Font = LoadFont("static/fonts/pixel-font-15.png");
            StepSound = LoadSound("static/sounds/step.wav");
            SmackSound = LoadSound("static/sounds/smack.wav");

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
            UnloadSound(SmackSound);
            UnloadMusicStream(Music);
            UnloadMusicStream(BattleMusic);
            UnloadTexture(EnemyTexture);
            UnloadTexture(EnemyAtlas);
            UnloadTexture(ChestAtlas);
            UnloadTexture(UIAtlas);
            UnloadImage(TileTextureImage);
            UnloadModel(TileModel);
            UnloadModel(FloorModel);
            UnloadShader(SurfaceShader);
            UnloadShader(PlasmaShader);
        }
    }
}
