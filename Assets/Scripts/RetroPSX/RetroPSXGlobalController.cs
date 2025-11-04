using UnityEngine;

namespace RetroPSX
{
    [ExecuteAlways]
    public class RetroPSXGlobalController : MonoBehaviour
    {
        public RetroPSXSettings settings;

        static readonly int _PSX_GridSize = Shader.PropertyToID("_PSX_GridSize");
        static readonly int _PSX_LightingNormalFactor = Shader.PropertyToID("_PSX_LightingNormalFactor");
        static readonly int _PSX_LightFalloffPercent = Shader.PropertyToID("_PSX_LightFalloffPercent");
        static readonly int _PSX_TextureWarpFactor = Shader.PropertyToID("_PSX_TextureWarpFactor");
        static readonly int _PSX_FlatShadingMode = Shader.PropertyToID("_PSX_FlatShadingMode");

        void OnEnable()
        {
            Apply();
        }

        void Update()
        {
            if (settings != null && settings.updateEveryFrame)
                Apply();
        }

        void OnValidate()
        {
            Apply();
        }

        public void Apply()
        {
            if (settings == null) return;
            Shader.SetGlobalFloat(_PSX_GridSize, settings.vertexGridResolution);
            Shader.SetGlobalFloat(_PSX_LightingNormalFactor, settings.retroLightingNormalFactor);
            Shader.SetGlobalFloat(_PSX_LightFalloffPercent, settings.retroLightFalloffStart);
            Shader.SetGlobalFloat(_PSX_TextureWarpFactor, settings.textureWarpFactor);
            Shader.SetGlobalFloat(_PSX_FlatShadingMode, settings.flatShadingCenterLight ? 1f : 0f);

            if (settings.enableRetroVertexLighting)
                Shader.EnableKeyword("RETRO_ENABLE_VERTEX_LIGHTING");
            else
                Shader.DisableKeyword("RETRO_ENABLE_VERTEX_LIGHTING");

            if (settings.flatShadingCenterLight)
                Shader.EnableKeyword("RETRO_FLAT_SHADING_CENTER");
            else
                Shader.DisableKeyword("RETRO_FLAT_SHADING_CENTER");
        }
    }
}
