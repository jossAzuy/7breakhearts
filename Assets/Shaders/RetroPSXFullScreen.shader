Shader "Hidden/RetroPSX/FullScreen"
{
    Properties{}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "PixelColorDither"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D_X(_BlitTexture); SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize;
            float _PixelationFactor;
            float3 _PreColorDepth;
            float3 _PostColorDepth;
            float _DitherScale;
            float _DitherMatrixMode; // 0,1,2

            static const float2 D2x2[4] = { float2(0,0), float2(0.5,0.5), float2(0.75,0.25), float2(0.25,0.75)};
            static const float D4x4[16] = {
                0, 0.5, 0.125,0.625,
                0.75,0.25,0.875,0.375,
                0.1875,0.6875,0.0625,0.5625,
                0.9375,0.4375,0.8125,0.3125
            };
            static const float D4x4PSX[16] = {
                0, 0.5, 0.125,0.625,
                0.75,0.25,0.875,0.375,
                0.1875,0.6875,0.0625,0.5625,
                0.9375,0.4375,0.8125,0.3125 // placeholder (reemplazar con patrón exacto si se desea)
            };

            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes v){ Varyings o; o.positionHCS = TransformObjectToHClip(v.positionOS.xyz); o.uv = v.uv; return o; }

            float3 Quantize(float3 c, float3 depth)
            {
                return floor(c * depth) / max(depth, 1);
            }

            float GetDither(float2 uv, float scale)
            {
                if (_DitherMatrixMode < 0.5)
                {
                    int2 p = int2(floor(uv * _BlitTexture_TexelSize.zw * scale)) & 1;
                    int idx = p.x + p.y * 2;
                    return D2x2[idx].x; // simple
                }
                int2 p4 = int2(floor(uv * _BlitTexture_TexelSize.zw * scale)) & 3;
                int idx4 = p4.x + p4.y * 4;
                if (_DitherMatrixMode < 1.5) return D4x4[idx4];
                return D4x4PSX[idx4];
            }

            float4 Frag(Varyings i):SV_Target
            {
                float2 uv = i.uv;
                // Pixelation: sample at snapped UV if factor < 1
                if (_PixelationFactor < 0.999)
                {
                    float2 pixelCount = _BlitTexture_TexelSize.zw * _PixelationFactor;
                    uv = (floor(uv * pixelCount) + 0.5) / pixelCount;
                }
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                if (_PreColorDepth.x > 0.99 && _PreColorDepth.y > 0.99 && _PreColorDepth.z > 0.99)
                {
                    // no pre quantization
                }
                else
                {
                    col.rgb = Quantize(col.rgb, _PreColorDepth);
                }
                if (_PostColorDepth.x < _PreColorDepth.x - 0.5 || _PostColorDepth.y < _PreColorDepth.y - 0.5 || _PostColorDepth.z < _PreColorDepth.z - 0.5)
                {
                    float d = GetDither(uv, max(_DitherScale, 0.0001));
                    float3 scaled = col.rgb * _PostColorDepth + d.xxx;
                    col.rgb = floor(scaled) / max(_PostColorDepth, 1);
                }
                return col;
            }
            ENDHLSL
        }
        Pass
        {
            Name "Interlace"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D_X(_BlitTexture); SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize; float _InterlaceSize;
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes v){ Varyings o; o.positionHCS = TransformObjectToHClip(v.positionOS.xyz); o.uv = v.uv; return o; }
            float4 Frag(Varyings i):SV_Target
            {
                if (_InterlaceSize > 0.5)
                {
                    float row = floor(i.uv.y * _BlitTexture_TexelSize.w);
                    if (fmod(row, _InterlaceSize*2) < _InterlaceSize)
                    {
                        // Atenuamos filas "apagadas"
                        float4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
                        c.rgb *= 0.5;
                        return c;
                    }
                }
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
            }
            ENDHLSL
        }
    }
}
