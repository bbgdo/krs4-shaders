using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Ascii")]
    public class Ascii : BaseEffect
    {
        private static readonly int AsciiTex = Shader.PropertyToID("_AsciiTex");
        private static readonly int DownsampledTex = Shader.PropertyToID("_DownsampledTex");
        private static readonly int GaussianKernelSize = Shader.PropertyToID("_GaussianKernelSize");
        private static readonly int Sigma1 = Shader.PropertyToID("_Sigma1");
        private static readonly int Sigma2 = Shader.PropertyToID("_SigmaK");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int AsciiEdgeTex = Shader.PropertyToID("_AsciiEdgeTex");

        [Range(0, 10)]
        public int kernelSize = 5;
        [Range(0.0f, 5.0f)]
        public float sigma1 = 1.0f;
        [Range(0.0f, 5.0f)]
        public float sigma2 = 1.5f;
        [Range(0.0f, 1.0f)] 
        public float threshold;
        
        private Material _asciiMaterial;
        private Material _dogMaterial;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _asciiMaterial = new Material(Shader.Find("Hidden/Custom/AsciiShader"));
            _dogMaterial = new Material(Shader.Find("Hidden/Custom/DoGShader"));
            
            Texture2D asciiLut = Resources.Load<Texture2D>("1x0 8x8 3");
            _asciiMaterial.SetTexture(AsciiTex, asciiLut);
            Texture2D asciiEdgeLut = Resources.Load<Texture2D>("edges 8x40");
            _asciiMaterial.SetTexture(AsciiEdgeTex, asciiEdgeLut);
            
            _dogMaterial.SetInt(GaussianKernelSize, kernelSize);
            _dogMaterial.SetFloat(Sigma1, sigma1);
            _dogMaterial.SetFloat(Sigma2, sigma2);
            _dogMaterial.SetFloat(Threshold, threshold);
            
            
            RenderTexture rtD1 = new RenderTexture(context.width / 2, context.height / 2, 0);
            RenderTexture rtD2 = new RenderTexture(context.width / 4, context.height / 4, 0);
            RenderTexture rtDFill3 = new RenderTexture(context.width / 8, context.height / 8, 0);
            RenderTexture rtDEdge3 = new RenderTexture(context.width / 8, context.height / 8, 0);

            Graphics.Blit(source, rtD1, _asciiMaterial, 0);
            Graphics.Blit(rtD1, rtD2, _asciiMaterial, 0);
            Graphics.Blit(rtD2, rtDFill3, _asciiMaterial, 0);
            _asciiMaterial.SetTexture(DownsampledTex, rtDFill3);
            
            RenderTexture rtHorizontalBlur = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtVerticalBlur = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtDoG = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtEdges = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtEdgesDownsampled = new RenderTexture(context.width, context.height, 0);
            
            Graphics.Blit(source, rtHorizontalBlur, _dogMaterial, 0);
            Graphics.Blit(rtHorizontalBlur, rtVerticalBlur, _dogMaterial, 1);
            Graphics.Blit(rtVerticalBlur, rtDoG, _dogMaterial, 2);
            Graphics.Blit(rtDoG, rtEdges, _asciiMaterial, 1);
            
            Graphics.Blit(rtEdges, rtD1, _asciiMaterial, 0);
            Graphics.Blit(rtD1, rtD2, _asciiMaterial, 0);
            Graphics.Blit(rtD2, rtDEdge3, _asciiMaterial, 0);
            Graphics.Blit(rtDEdge3, rtEdgesDownsampled, _asciiMaterial, 0);
            
            Graphics.Blit(rtEdgesDownsampled, destination, _asciiMaterial, 2);
            
            rtD1.Release();
            rtD2.Release();
            rtDFill3.Release();
            rtDEdge3.Release();
            rtHorizontalBlur.Release();
            rtVerticalBlur.Release();
            rtDoG.Release();
            rtEdges.Release();
            rtEdgesDownsampled.Release();
        }
    }
}