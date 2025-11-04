// Modern URP-compatible RenderFeature with legacy + RenderGraph paths.
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_2023_2_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace RetroPSX
{
    public class RetroPSXRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class RetroPSXSettingsRT
        {
            public RetroPSXSettings settings;
            public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public Shader fullScreenShader;
        }

        class RetroPSXFullScreenPass : ScriptableRenderPass
        {
            readonly RetroPSXSettingsRT _cfg;
            Material _mat;
            RTHandle _temp1;

            // Shader property IDs
            static readonly int _PixelationFactor = Shader.PropertyToID("_PixelationFactor");
            static readonly int _PreColorDepth = Shader.PropertyToID("_PreColorDepth");
            static readonly int _PostColorDepth = Shader.PropertyToID("_PostColorDepth");
            static readonly int _DitherScale = Shader.PropertyToID("_DitherScale");
            static readonly int _DitherMatrixMode = Shader.PropertyToID("_DitherMatrixMode");
            static readonly int _InterlaceSize = Shader.PropertyToID("_InterlaceSize");

            public RetroPSXFullScreenPass(RetroPSXSettingsRT cfg) => _cfg = cfg;

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (_cfg.fullScreenShader == null) return;
                if (_mat == null) _mat = CoreUtils.CreateEngineMaterial(_cfg.fullScreenShader);
                RenderingUtils.ReAllocateIfNeeded(ref _temp1, renderingData.cameraData.cameraTargetDescriptor, name: "RetroPSX_Temp1");
                ConfigureInput(ScriptableRenderPassInput.Color);
            }

            void SetMaterialParams(RetroPSXSettings s)
            {
                _mat.SetFloat(_PixelationFactor, s.pixelationFactor);
                _mat.SetVector(_PreColorDepth, s.preDitherColorDepth);
                _mat.SetVector(_PostColorDepth, s.postDitherColorDepth);
                _mat.SetFloat(_DitherScale, s.ditherScale);
                _mat.SetFloat(_DitherMatrixMode, (float)s.ditherMatrix);
                _mat.SetFloat(_InterlaceSize, s.enableInterlacing ? s.interlacingSize : 0);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_2023_2_OR_NEWER
                if (UniversalRenderPipeline.asset != null && UniversalRenderPipeline.asset.useRenderGraph) return; // RenderGraph path handles it
#endif
                if (_cfg.settings == null || _mat == null) return;
                var s = _cfg.settings;
                var cmd = CommandBufferPool.Get("RetroPSXFullscreen");
                using (new ProfilingScope(cmd, new ProfilingSampler("RetroPSX FullScreen (Legacy)")))
                {
                    SetMaterialParams(s);
                    var src = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    // Pass 0
                    Blitter.BlitCameraTexture(cmd, src, _temp1, _mat, 0);
                    // Pass 1 (optional)
                    if (s.enableInterlacing)
                        Blitter.BlitCameraTexture(cmd, _temp1, src, _mat, 1);
                    else
                        Blitter.BlitCameraTexture(cmd, _temp1, src);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

#if UNITY_2023_2_OR_NEWER
            struct PassData
            {
                public Material material;
                public RetroPSXSettings settings;
                public int passIndex;
                public TextureHandle source;
                public TextureHandle dest;
            }

            protected override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_cfg == null || _cfg.settings == null || _cfg.fullScreenShader == null) return;
                if (_mat == null) _mat = CoreUtils.CreateEngineMaterial(_cfg.fullScreenShader);
                var s = _cfg.settings;
                var resources = frameData.Get<UniversalResourceData>();
                var cameraColor = resources.activeColorTexture;

                var desc = resources.cameraDescriptor; desc.depthBufferBits = 0;
                var tempHandle = renderGraph.CreateTexture(new TextureDesc(desc.width, desc.height)
                { colorFormat = desc.graphicsFormat, name = "RetroPSX_TempRG" });

                // Pass 0
                {
                    var pass = renderGraph.AddRenderPass<PassData>("RetroPSX P0", out var pd, ProfilingSampler.Get("RetroPSX P0"));
                    pd.material = _mat; pd.settings = s; pd.passIndex = 0; pd.source = cameraColor; pd.dest = tempHandle;
                    pass.UseTexture(cameraColor, AccessFlags.Read); pass.UseTexture(tempHandle, AccessFlags.Write);
                    pass.SetRenderFunc((PassData d, RenderGraphContext ctx) =>
                    {
                        d.material.SetFloat(_PixelationFactor, d.settings.pixelationFactor);
                        d.material.SetVector(_PreColorDepth, d.settings.preDitherColorDepth);
                        d.material.SetVector(_PostColorDepth, d.settings.postDitherColorDepth);
                        d.material.SetFloat(_DitherScale, d.settings.ditherScale);
                        d.material.SetFloat(_DitherMatrixMode, (float)d.settings.ditherMatrix);
                        d.material.SetFloat(_InterlaceSize, d.settings.enableInterlacing ? d.settings.interlacingSize : 0);
                        Blitter.BlitTexture(ctx.cmd, d.source, Vector2.one, d.dest, d.passIndex);
                    });
                }

                // Pass 1
                {
                    var pass = renderGraph.AddRenderPass<PassData>("RetroPSX P1", out var pd, ProfilingSampler.Get("RetroPSX P1"));
                    pd.material = _mat; pd.settings = s; pd.passIndex = 1; pd.source = tempHandle; pd.dest = cameraColor;
                    pass.UseTexture(tempHandle, AccessFlags.Read); pass.UseTexture(cameraColor, AccessFlags.Write);
                    pass.SetRenderFunc((PassData d, RenderGraphContext ctx) =>
                    {
                        if (!d.settings.enableInterlacing)
                        {
                            Blitter.BlitTexture(ctx.cmd, d.source, Vector2.one, d.dest);
                        }
                        else
                        {
                            d.material.SetFloat(_PixelationFactor, d.settings.pixelationFactor);
                            d.material.SetVector(_PreColorDepth, d.settings.preDitherColorDepth);
                            d.material.SetVector(_PostColorDepth, d.settings.postDitherColorDepth);
                            d.material.SetFloat(_DitherScale, d.settings.ditherScale);
                            d.material.SetFloat(_DitherMatrixMode, (float)d.settings.ditherMatrix);
                            d.material.SetFloat(_InterlaceSize, d.settings.interlacingSize);
                            Blitter.BlitTexture(ctx.cmd, d.source, Vector2.one, d.dest, d.passIndex);
                        }
                    });
                }
            }
#endif
        }

        public RetroPSXSettingsRT settings = new RetroPSXSettingsRT();
        RetroPSXFullScreenPass _pass;

        public override void Create()
        {
            _pass = new RetroPSXFullScreenPass(settings) { renderPassEvent = settings.passEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.fullScreenShader == null || settings.settings == null) return;
            renderer.EnqueuePass(_pass);
        }
    }
}
#else
// Fallback stub if URP not present
using UnityEngine;
namespace RetroPSX { public class RetroPSXRenderFeature : ScriptableObject { } }
#endif
