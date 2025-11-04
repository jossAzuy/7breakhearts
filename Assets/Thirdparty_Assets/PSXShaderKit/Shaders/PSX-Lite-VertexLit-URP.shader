Shader "PSX/Lite/Vertex Lit URP"
{
    Properties
    {
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1)
        _EmissionColor("Emission Color (RGBA)", Color) = (0,0,0,0)
        _CubemapColor("Cubemap Color (RGBA)", Color) = (0,0,0,0)
        _MainTex("Texture", 2D) = "white" {}
        _EmissiveTex("Emissive", 2D) = "black" {}
        _Cubemap("Cubemap", Cube) = "" {}
        _ReflectionMap("Reflection Map", 2D) = "white" {}
        _ObjectDithering("Per-Object Dithering Enable", Range(0,1)) = 1
        _FlatShading("Flat Shading", Range(0,1)) = 0
        _CustomDepthOffset("Custom Depth Offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        ZWrite On
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile __ PSX_ENABLE_CUSTOM_VERTEX_LIGHTING

            // Keep compatibility with the original cgincs
            #define PSX_TRIANGLE_SORT_OFF
            #include "UnityCG.cginc"
            #include "PSX-Utils.cginc"

            samplerCUBE _Cubemap;
            sampler2D _ReflectionMap;
            float4 _CubemapColor;

            #define PSX_VERTEX_LIT
            #define PSX_CUBEMAP _Cubemap
            #define PSX_CUBEMAP_COLOR _CubemapColor

            #include "PSX-ShaderSrc-Lite.cginc"
            ENDCG
        }
    }
    // Fallbacks are ignored by SRP, but keep for compatibility
    Fallback "PSX/Lite/Unlit"
}
