using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo.FightFightDanger;

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

        void main() {
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

        void main() {
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

        void main() {
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

        #define fog_start 0
        #define fog_end 20

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
        vec3 rgb2hsv(vec3 c) {
            vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
            vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

            float d = q.x - min(q.w, q.y);
            float e = 1.0e-10;
            return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
        }

        // All components are in the range [0…1], including hue.
        vec3 hsv2rgb(vec3 c) {
            vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
            return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
        }

        float linearize(float depth, float near, float far) {
            return (2.0 * near * far) / (far + near - depth * (far - near));
        }


        // NOTE: Add your custom variables here

        void main() {
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
            hsv.z = clamp(hsv.z, 0, 1);

            rgba.rgb = hsv2rgb(hsv);

            float linearDepth = linearize(gl_FragCoord.z, 0.05, 4000.0);
            float fog_factor = (linearDepth - fog_start) / (fog_end - fog_start);
            fog_factor = clamp(fog_factor, 0.0, 1.0); // Clamp to ensure valid range

            rgba.rgb = mix(rgba.rgb, vec3(0), fog_factor);

            finalColor = rgba;
        }
        """;

    private const string PLASMA_FRAGMENT_SHADER_SOURCE =
    """
        #version 330

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;
        in vec3 fragNormal;

        // Input uniform values
        uniform float iTime;
        uniform vec2 iResolution;

        // Output fragment color
        out vec4 finalColor;

        void main() {
            float t = iTime*.3;
            vec2 fragCoord = fragTexCoord * iResolution;

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
            finalColor = pow(vec4(col,1.0)*(1.-length(uv*1.6)),vec4(4));
        }
        """;

    private const string SCREEN_TRANSITION_FRAGMENT_SHADER_SOURCE =
    """
        #version 330

        #define PI 3.14159265359
        #define THETA deg2rad(50.)
        #define DURATION ( 1. + .5 * ( iResolution.x / iResolution.y ) * tan(THETA) )
        #define TIME ( 2. * iTime )

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;
        in vec3 fragNormal;

        // Input uniform values
        uniform float iTime;
        uniform vec2 iResolution;

        // Output fragment color
        out vec4 finalColor;

        float deg2rad( float deg ) {
            return deg * PI / 180.;
        }

        vec2 rot( in vec2 uv, float theta ) {
            vec2 uv2;
            uv2.x = uv.x * cos(theta) - uv.y * sin(theta);
            uv2.y = uv.x * sin(theta) + uv.y * cos(theta);
            return uv2;
        }

        void main() {
            vec2 uv = ( fragTexCoord*iResolution-.5*vec2(iResolution.x,0.) ) / iResolution.y;

            uv.x = abs(uv.x);
            uv = rot(uv, THETA);

            float t = mod( TIME, DURATION );
            float id = -2.*( mod( floor( TIME / DURATION ), 2.) - .5 );

            finalColor = id*(uv.y - t) > 0. ? vec4(0.) : vec4(.1, .1, .5, 1);
        }
        """;

    private const string DOWNMIXED_FRAGMENT_SHADER_SOURCE =
    """
        #version 330

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;

        // Input uniform values
        uniform sampler2D texture0;
        uniform vec4 colDiffuse;
        uniform sampler2D lutTexture;
        uniform vec2 lutTextureSize;

        // Output fragment color
        out vec4 finalColor;

        // NOTE: Add your custom variables here

        vec4 lookup(in vec4 textureColor, in sampler2D lookupTable) {
            textureColor = clamp(textureColor, 0.0, 1.0);

            mediump float blueColor = textureColor.b * 63.0;

            mediump vec2 quad1;
            quad1.y = floor(floor(blueColor) / 8.0);
            quad1.x = floor(blueColor) - (quad1.y * 8.0);

            mediump vec2 quad2;
            quad2.y = floor(ceil(blueColor) / 8.0);
            quad2.x = ceil(blueColor) - (quad2.y * 8.0);

            highp vec2 texPos1;
            texPos1.x = (quad1.x * 0.125) + 0.5/512.0 + ((0.125 - 1.0/512.0) * textureColor.r);
            texPos1.y = (quad1.y * 0.125) + 0.5/512.0 + ((0.125 - 1.0/512.0) * textureColor.g);

            highp vec2 texPos2;
            texPos2.x = (quad2.x * 0.125) + 0.5/512.0 + ((0.125 - 1.0/512.0) * textureColor.r);
            texPos2.y = (quad2.y * 0.125) + 0.5/512.0 + ((0.125 - 1.0/512.0) * textureColor.g);

            lowp vec4 newColor1 = texture2D(lookupTable, texPos1);
            lowp vec4 newColor2 = texture2D(lookupTable, texPos2);

            lowp vec4 newColor = mix(newColor1, newColor2, fract(blueColor));

            return newColor;
        }

        void main() {
            // Texel color fetching from texture sampler
            vec4 texelColor = texture(texture0, fragTexCoord);

            // NOTE: Implement here your fragment shader code

            // final color is the color from the texture
            //    times the tint color (colDiffuse)
            //    times the fragment color (interpolated vertex color)
            vec4 col = texelColor*colDiffuse*fragColor;

            // finalColor = col;
            finalColor = lookup(col, lutTexture);
        }
        """;

    public static Shader SurfaceShader;
    public static Shader PlasmaShader;
    public static Shader ScreenTransitionShader;
    public static Shader DownmixedShader;
    public static Image TileTextureImage;
    public static Texture2D TileTexture;
    public static Texture2D FloorTexture;
    public static Texture2D CeilingTexture;
    public static Texture2D ChestAtlas;
    public static Texture2D EnemyTexture;
    public static Texture2D EnemyAtlas;
    public static Texture2D UIAtlas;
    public static Texture2D LUTTexture;
    public static Material TileMaterial;
    public static Material FloorMaterial;
    public static Mesh TileMesh;
    public static Mesh PlaneMesh;
    public static Model TileModel;
    public static Model FloorModel;
    public static Model CeilingModel;
    public static Sound StepSound;
    public static Sound SmackSound;
    public static Sound BattleStartSound;
    public static Sound OpenChestSound;
    public static Sound TalkSound;
    public static Music Music;
    public static Music BattleMusic;
    public static Font Font;

    public static void CacheAndInitializeAll()
    {
        SurfaceShader = LoadShaderFromMemory(SURFACE_VERTEX_SHADER_SOURCE, SURFACE_FRAGMENT_SHADER_SOURCE);
        PlasmaShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, PLASMA_FRAGMENT_SHADER_SOURCE);
        ScreenTransitionShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, SCREEN_TRANSITION_FRAGMENT_SHADER_SOURCE);
        DownmixedShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, DOWNMIXED_FRAGMENT_SHADER_SOURCE);
        TileTextureImage = LoadImage("static/textures/cobolt-stone-0-moss-0.png");
        ImageFlipVertical(ref TileTextureImage);
        TileTexture = LoadTextureFromImage(TileTextureImage);
        FloorTexture = LoadTexture("static/textures/cobolt-stone-1-floor-0.png");
        CeilingTexture = LoadTexture("static/textures/cobolt-stone-0-floor-0.png");
        ChestAtlas = LoadTexture("static/textures/chest-wooden-0.png");
        UIAtlas = LoadTexture("static/textures/ui.png");
        EnemyTexture = LoadTexture("static/textures/enemy.png");
        LUTTexture = LoadTexture("static/textures/lut.png");
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
        BattleStartSound = LoadSound("static/sounds/battle_start.wav");
        OpenChestSound = LoadSound("static/sounds/open_chest.wav");
        TalkSound = LoadSound("static/sounds/talk.wav");

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
        UnloadSound(BattleStartSound);
        UnloadSound(OpenChestSound);
        UnloadSound(TalkSound);
        UnloadMusicStream(Music);
        UnloadMusicStream(BattleMusic);
        UnloadTexture(EnemyTexture);
        UnloadTexture(EnemyAtlas);
        UnloadTexture(ChestAtlas);
        UnloadTexture(UIAtlas);
        UnloadTexture(LUTTexture);
        UnloadImage(TileTextureImage);
        UnloadModel(TileModel);
        UnloadModel(FloorModel);
        UnloadShader(SurfaceShader);
        UnloadShader(PlasmaShader);
        UnloadShader(ScreenTransitionShader);
        UnloadShader(DownmixedShader);
    }
}
