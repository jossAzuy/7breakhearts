#ifndef RETRO_PSX_COLOR_QUANTIZATION_INCLUDED
#define RETRO_PSX_COLOR_QUANTIZATION_INCLUDED

float3 Retro_QuantizeColor(float3 color, float3 depth)
{
    return floor(color * depth) / max(depth, 1);
}

float DitherNoise(float2 uv, float scale)
{
    // simple hash noise for placeholder dithering inside graph if needed
    float2 p = floor(uv * scale);
    float n = sin(dot(p, float2(12.9898,78.233))) * 43758.5453;
    return frac(n);
}

float3 Retro_QuantizeColor_Dither(float3 color, float3 preDepth, float3 postDepth, float2 uv, float ditherScale)
{
    float3 pre = floor(color * preDepth) / max(preDepth,1);
    float noise = DitherNoise(uv, ditherScale);
    float3 shifted = pre * postDepth + noise.xxx;
    return floor(shifted) / max(postDepth,1);
}

#endif
