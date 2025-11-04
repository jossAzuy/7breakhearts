#ifndef RETRO_PSX_VERTEX_SNAP_INCLUDED
#define RETRO_PSX_VERTEX_SNAP_INCLUDED

float3 Retro_SnapViewPosition(float3 worldPos, float4x4 viewMatrix, float gridSize)
{
    float4 viewPos = mul(viewMatrix, float4(worldPos,1));
    if (gridSize > 0.0001)
    {
        viewPos.xyz = round(viewPos.xyz * gridSize) / gridSize;
    }
    // regresar a world (aprox inversa: sería necesario pasar la inverseViewMatrix si precisión requerida)
    return viewPos.xyz; // Para usar en etapa view-space dentro graph; alternativamente exponer pos vista directamente.
}

#endif
