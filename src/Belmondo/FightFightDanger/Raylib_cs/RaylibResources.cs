using System.Numerics;
using Microsoft.VisualBasic.FileIO;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo.FightFightDanger;

public static class RaylibResources
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

        #define fog_start 0
        #define fog_end 20

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;

        // Input uniform values
        uniform sampler2D texture0;
        uniform vec4 colDiffuse;

        // Output fragment color
        out vec4 finalColor;

        // NOTE: Add your custom variables here

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

        void main() {
            // Texel color fetching from texture sampler
            vec4 texelColor = texture(texture0, fragTexCoord);

            // NOTE: Implement here your fragment shader code

            // final color is the color from the texture
            //    times the tint color (colDiffuse)
            //    times the fragment color (interpolated vertex color)
            vec4 rgba = texelColor*colDiffuse*fragColor;
            vec3 rgb = rgba.rgb;

            float linearDepth = linearize(gl_FragCoord.z, 0.05, 4000.0);
            float fog_factor = (linearDepth - fog_start) / (fog_end - fog_start);
            fog_factor = clamp(fog_factor, 0.0, 1.0); // Clamp to ensure valid range

            rgba.rgb = mix(rgba.rgb, vec3(0), fog_factor);

            finalColor = rgba;
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

    private const string SCREEN_TRANSITION_2_FRAGMENT_SHADER_SOURCE = """
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

    // World Famous
    float rand(vec2 co) {
        return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
    }

    vec3 get_color(vec2 a) {
        int c = int(rand(a) * 7.);

        switch (c) {
            case 0:
                return vec3(1.,0.,0.);
            case 1:
                return vec3(0.,1.,0.);
            case 2:
                return vec3(0.,0.,1.);
            case 3:
                return vec3(1.,.5,0.);
            case 4:
                return vec3(1.,1.,0.);
            case 5:
                return vec3(1.,0.,1.);
            case 6:
                return vec3(0.,1.,1.);
        }

        return vec3(1.);
    }

    // Thanks to The6P4C for the help with maths
    vec2 rotate(vec2 uv) {
        float theta = radians(45.);
        mat2 rot = mat2(
            cos(theta), -sin(theta),
            sin(theta), cos(theta)
        );

        vec2 center = iResolution.xy/2.;
        uv = (uv - center) * rot + center;
        return uv;
    }

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

    // Max size of square in pixels
    #define SIZE 50.0
    // The bigger, the flatter tiles are
    #define CORNER 20.0
    // Light direction
    #define LIGHT_DIR vec3(0.65, 0.57, 2.0)

    #define pi 3.1415926535897932384626433832795
    #define hfpi 1.5707963267948966192313216916398
    #define PI pi
    #define HFPI hfpi

    vec3 TileSquare(vec2 posSample) {
        float size = 24;
        float halfSize = size / 2.0;

        vec2 screenPos = posSample*iResolution.xy - (iResolution.xy / 2.0) - vec2(halfSize);
        vec2 pos = mod(screenPos, vec2(size)) - vec2(halfSize);

        vec2 uv = posSample - pos/iResolution.xy;

        vec3 texColorSample = vec3(1., .7, .1);

        vec3 normal = normalize(vec3(tan((pos.x/size) * PI), tan((pos.y/size) * PI), CORNER));
        //vec3 normal = normalize(vec3(pos.x/halfSize, pos.y/halfSize, smoothstep(0.0, halfSize, halfSize - sqrt(pos.x*pos.x + pos.y*pos.y))*CORNER)); //nice

        float bright = dot(normal, normalize(LIGHT_DIR));

        bright = pow(bright, 0.5);

        vec3 colFinal = texColorSample * bright;

        vec3 heif = normalize(LIGHT_DIR + vec3(0.0, 0.0, 0.1));

        float spec = pow(dot(heif, normal), 96.0);

        colFinal += vec3(spec);

        // Set the final fragment color.
        return colFinal;
    }

    void main() {
        float t = iTime;

        // Normalized coordinates, quantized to 16x16 squares
        // There's probably a nicer way to express this
        vec2 fragCoord = rotate(fragTexCoord * iResolution.xy);
        vec2 uv = (fragCoord-mod(fragCoord, 24.)) / iResolution.xy;

        // Average of x and y creates a diagonal gradient from bottom left to top right
        // Make y smaller to give the gradient a steeper angle
        // This may need aspect ratio correction? Not sure
        float a = (uv.x + uv.y) / 2.;

        // Add a bit of randomness to make it look swankier :3
        a += rand(uv) * 0.05;

        // Colorful! Like Tetrominoes
        vec3 color = TileSquare(rotate(fragTexCoord));
        //color = vec3(0.,a*1.5,1.);

        // Use that gradient as a measure of when to color each square
        if (t > a && t < a+1.) {
            finalColor = vec4(color, 1.0);
        } else {
            finalColor = vec4(0.);
        }
    }
    """;

    private const string BATTLE_BG_SHAPE_FRAGMENT_SHADER_SOURCE =
    """
        #version 330

        #define fog_start 0
        #define fog_end 20

        // Input vertex attributes (from vertex shader)
        in vec2 fragTexCoord;
        in vec4 fragColor;

        // Input uniform values
        uniform sampler2D texture0;
        uniform vec4 colDiffuse;
        uniform float iTime;

        // Output fragment color
        out vec4 finalColor;

        // NOTE: Add your custom variables here

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

        void main() {
            // Texel color fetching from texture sampler
            vec4 texelColor = texture(texture0, fragTexCoord);

            // NOTE: Implement here your fragment shader code

            // final color is the color from the texture
            //    times the tint color (colDiffuse)
            //    times the fragment color (interpolated vertex color)
            vec4 rgba = texelColor*colDiffuse*fragColor;
            vec3 rgb = rgba.rgb;

            float linearDepth = linearize(gl_FragCoord.z, 0.05, 4000.0);
            float fog_factor = (linearDepth - fog_start) / (fog_end - fog_start);
            fog_factor = clamp(fog_factor, 0.0, 1.0); // Clamp to ensure valid range

            float offs = mod(iTime, 1.);

            rgba.rgb = mix(rgba.rgb, rgba.rgb * 1.2, float(fog_factor >= offs && fog_factor <= offs + .1 ));
            rgba.rgb = mix(rgba.rgb, vec3(0), fog_factor);

            finalColor = rgba;
        }
        """;

    public static Shader BaseShader { get; private set; }
    public static Shader BattleBackgroundShapeShader { get; private set; }
    public static Shader DownmixedShader { get; private set; }
    public static Shader PlasmaShader { get; private set; }
    public static Shader ScreenTransitionShader { get; private set; }
    public static Shader ScreenTransitionShader2 { get; private set; }
    public static Shader SurfaceShader { get; private set; }
    public static Texture2D CeilingTexture { get; private set; }
    public static Texture2D ChestAtlas { get; private set; }
    public static Texture2D CrosshairAtlasTexture { get; private set; }
    public static Texture2D EnemyAtlas { get; private set; }
    public static Texture2D EnemyTexture { get; private set; }
    public static Texture2D FloorTexture { get; private set; }
    public static Texture2D LUTTexture { get; private set; }
    public static Texture2D MugshotTexture { get; private set; }
    public static Texture2D TileTexture { get; private set; }
    public static Texture2D UIAtlasTexture { get; private set; }
    public static Material TileMaterial { get; private set; }
    public static Material FloorMaterial { get; private set; }
    public static Mesh TileMesh { get; private set; }
    public static Mesh PlaneMesh { get; private set; }
    public static Model TileModel { get; private set; }
    public static Model FloorModel { get; private set; }
    public static Model CeilingModel { get; private set; }
    public static Sound BattleStartSound { get; private set; }
    public static Sound ClapSound { get; private set; }
    public static Sound CritSound { get; private set; }
    public static Sound DieSound { get; private set; }
    public static Sound HoughSound { get; private set; }
    public static Sound ItemSound { get; private set; }
    public static Sound MachineGunSound { get; private set; }
    public static Sound MissSound { get; private set; }
    public static Sound OpenChestSound { get; private set; }
    public static Sound SmackSound { get; private set; }
    public static Sound StepSound { get; private set; }
    public static Sound TalkSound { get; private set; }
    public static Sound UICancelSound { get; private set; }
    public static Sound UIConfirmSound { get; private set; }
    public static Sound UIFocusSound { get; private set; }
    public static Music Stage1WanderingMusic { get; private set; }
    public static Music Stage2WanderingMusic { get; private set; }
    public static Music VictoryMusic { get; private set; }
    public static Music BattleMusic { get; private set; }
    public static Font Font { get; private set; }
    public static Font MediumFont { get; private set; }

    public static int BattleBackgroundShapeShaderTimeLoc { get; private set; }
    public static int DownmixedShaderLUTLoc { get; private set; }
    public static int DownmixedShaderLUTSizeLoc { get; private set; }
    public static int PlasmaShaderResolutionLoc { get; private set; }
    public static int PlasmaShaderTimeLoc { get; private set; }
    public static int ScreenTransitionShader2ResolutionLoc { get; private set; }
    public static int ScreenTransitionShader2TimeLoc { get; private set; }
    public static int ScreenTransitionShaderResolutionLoc { get; private set; }
    public static int ScreenTransitionShaderTimeLoc { get; private set; }
    public static Vector2 LUTSize { get; private set; }

    public static void CacheAndInitializeAll()
    {
        BaseShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, BASE_FRAGMENT_SHADER_SOURCE);
        BattleBackgroundShapeShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, BATTLE_BG_SHAPE_FRAGMENT_SHADER_SOURCE);
        DownmixedShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, DOWNMIXED_FRAGMENT_SHADER_SOURCE);
        PlasmaShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, PLASMA_FRAGMENT_SHADER_SOURCE);
        ScreenTransitionShader = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, SCREEN_TRANSITION_FRAGMENT_SHADER_SOURCE);
        ScreenTransitionShader2 = LoadShaderFromMemory(BASE_VERTEX_SHADER_SOURCE, SCREEN_TRANSITION_2_FRAGMENT_SHADER_SOURCE);
        SurfaceShader = LoadShaderFromMemory(SURFACE_VERTEX_SHADER_SOURCE, SURFACE_FRAGMENT_SHADER_SOURCE);

        Image img = LoadImage("static/textures/cobolt-stone-0-moss-0.png");

        ImageFlipVertical(ref img);
        TileTexture = LoadTextureFromImage(img);
        UnloadImage(img);

        CeilingTexture = LoadTexture("static/textures/cobolt-stone-0-floor-0.png");
        ChestAtlas = LoadTexture("static/textures/chest-wooden-0.png");
        CrosshairAtlasTexture = LoadTexture("static/textures/crosshair.png");
        EnemyTexture = LoadTexture("static/textures/enemy.png");
        FloorTexture = LoadTexture("static/textures/cobolt-stone-1-floor-0.png");
        LUTTexture = LoadTexture("static/textures/lut.png");
        MugshotTexture = LoadTexture("static/textures/mugshot.png");
        UIAtlasTexture = LoadTexture("static/textures/ui.png");
        EnemyAtlas = LoadTexture("static/textures/enemy-atlas.png");
        TileMaterial = LoadMaterialDefault();
        FloorMaterial = LoadMaterialDefault();
        TileMesh = GenMeshCube(1, 1, 1);
        TileModel = LoadModelFromMesh(TileMesh);
        PlaneMesh = GenMeshPlane(1000, 1000, 1, 1);
        FloorModel = LoadModelFromMesh(PlaneMesh);
        CeilingModel = LoadModelFromMesh(PlaneMesh);
        Stage1WanderingMusic = LoadMusicStream("static/music/ronde.mp3");
        Stage2WanderingMusic = LoadMusicStream("static/music/draculas-tears.mp3");

        {
            var victoryMusic = LoadMusicStream("static/music/victory.mp3");

            victoryMusic.Looping = false;
            VictoryMusic = victoryMusic;
        }

        BattleMusic = LoadMusicStream("static/music/morgan.mp3");
        Font = LoadFont("static/fonts/pixel-font-15.png");
        MediumFont = LoadFont("static/fonts/pixel-font-11.png");

        BattleStartSound = LoadSound("static/sounds/battle_start.wav");
        ClapSound = LoadSound("static/sounds/clap.wav");
        CritSound = LoadSound("static/sounds/crit.wav");
        DieSound = LoadSound("static/sounds/die.wav");
        HoughSound = LoadSound("static/sounds/hough.wav");
        ItemSound = LoadSound("static/sounds/item.ogg");
        MachineGunSound = LoadSound("static/sounds/machine_gun.wav");
        MissSound = LoadSound("static/sounds/miss.ogg");
        OpenChestSound = LoadSound("static/sounds/open_chest.wav");
        SmackSound = LoadSound("static/sounds/smack.wav");
        StepSound = LoadSound("static/sounds/step.wav");
        TalkSound = LoadSound("static/sounds/talk.wav");
        UICancelSound = LoadSound("static/sounds/ui_cancel.ogg");
        UIConfirmSound = LoadSound("static/sounds/ui_confirm.wav");
        UIFocusSound = LoadSound("static/sounds/ui_focus.ogg");

        unsafe
        {
            TileModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = TileTexture;
            TileModel.Materials[0].Shader = SurfaceShader;
            FloorModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = FloorTexture;
            FloorModel.Materials[0].Shader = SurfaceShader;
            CeilingModel.Materials[0].Maps[(int)MaterialMapIndex.Diffuse].Texture = CeilingTexture;
            CeilingModel.Materials[0].Shader = SurfaceShader;
        }

        BattleBackgroundShapeShaderTimeLoc = GetShaderLocation(BattleBackgroundShapeShader, "iTime");
        DownmixedShaderLUTLoc = GetShaderLocation(DownmixedShader, "lutTexture");
        DownmixedShaderLUTSizeLoc = GetShaderLocation(DownmixedShader, "lutTextureSize");
        PlasmaShaderResolutionLoc = GetShaderLocation(PlasmaShader, "iResolution");
        PlasmaShaderTimeLoc = GetShaderLocation(PlasmaShader, "iTime");
        ScreenTransitionShader2ResolutionLoc = GetShaderLocation(ScreenTransitionShader2, "iResolution");
        ScreenTransitionShader2TimeLoc = GetShaderLocation(ScreenTransitionShader2, "iTime");
        ScreenTransitionShaderResolutionLoc = GetShaderLocation(ScreenTransitionShader, "iResolution");
        ScreenTransitionShaderTimeLoc = GetShaderLocation(ScreenTransitionShader, "iTime");
        LUTSize = new Vector2(LUTTexture.Width, LUTTexture.Height);
    }

    public static void UnloadAll()
    {
        UnloadFont(Font);
        UnloadFont(MediumFont);
        UnloadSound(BattleStartSound);
        UnloadSound(ClapSound);
        UnloadSound(CritSound);
        UnloadSound(DieSound);
        UnloadSound(HoughSound);
        UnloadSound(ItemSound);
        UnloadSound(MachineGunSound);
        UnloadSound(MissSound);
        UnloadSound(OpenChestSound);
        UnloadSound(SmackSound);
        UnloadSound(StepSound);
        UnloadSound(TalkSound);
        UnloadSound(UICancelSound);
        UnloadSound(UIConfirmSound);
        UnloadSound(UIFocusSound);
        UnloadMusicStream(BattleMusic);
        UnloadMusicStream(Stage1WanderingMusic);
        UnloadMusicStream(Stage2WanderingMusic);
        UnloadMusicStream(VictoryMusic);
        UnloadTexture(ChestAtlas);
        UnloadTexture(CrosshairAtlasTexture);
        UnloadTexture(EnemyAtlas);
        UnloadTexture(EnemyTexture);
        UnloadTexture(LUTTexture);
        UnloadTexture(MugshotTexture);
        UnloadTexture(UIAtlasTexture);
        UnloadModel(TileModel);
        UnloadModel(FloorModel);
        UnloadShader(BaseShader);
        UnloadShader(BattleBackgroundShapeShader);
        UnloadShader(DownmixedShader);
        UnloadShader(PlasmaShader);
        UnloadShader(ScreenTransitionShader);
        UnloadShader(ScreenTransitionShader2);
        UnloadShader(SurfaceShader);
    }
}
