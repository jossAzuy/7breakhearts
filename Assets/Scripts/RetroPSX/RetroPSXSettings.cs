using UnityEngine;

namespace RetroPSX
{
    [CreateAssetMenu(menuName = "RetroPSX/Settings", fileName = "RetroPSXSettings")]
    public class RetroPSXSettings : ScriptableObject
    {
        [Header("Pixelation")]
        [Range(0.1f, 1f)] public float pixelationFactor = 1f; // 1 = sin pixelación

        [Header("Color Quantization (Fullscreen)")]
        public bool enableFullscreenColor = true;
        [Tooltip("Profundidad de color antes del dithering (valores por canal). 256 = sin reducción.")]
        public Vector3 preDitherColorDepth = new Vector3(256,256,256);
        [Tooltip("Profundidad de color simulada tras dithering.")]
        public Vector3 postDitherColorDepth = new Vector3(32,32,32);

        [Header("Dithering")]
        public bool enableDithering = true;
        public enum DitherMatrix { D2x2, D4x4, D4x4_PSX }
        public DitherMatrix ditherMatrix = DitherMatrix.D4x4_PSX;
        [Range(0f,2f)] public float ditherScale = 1f;

        [Header("Interlacing")]
        public bool enableInterlacing = false;
        [Range(1,4)] public int interlacingSize = 1;

        [Header("Vertex Wobble (ShaderGraph)")]
        public bool enableVertexSnap = true;
        [Tooltip("Grid size para snap en espacio de vista.")]
        public float vertexGridResolution = 100f;

        [Header("Texture Warping (Approx)")]
        [Range(0f,1f)] public float textureWarpFactor = 1f;

        [Header("Lighting Retro Flags")]
        public bool enableRetroVertexLighting = true;
        [Range(0f,1f)] public float retroLightingNormalFactor = 0f;
        [Range(0f,0.999f)] public float retroLightFalloffStart = 0f;

        [Header("Flat Shading Mode")]
        public bool flatShadingCenterLight = false;

        [Header("Runtime Update")]
        public bool updateEveryFrame = true;
    }
}
