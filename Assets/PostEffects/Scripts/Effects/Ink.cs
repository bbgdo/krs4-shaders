using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Ink")]
    public class Ink : BaseEffect {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int StipplingBias = Shader.PropertyToID("_StipplingBias");
        private static readonly int NoiseTexture = Shader.PropertyToID("_NoiseTex");
        private static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        private static readonly int StippleTex = Shader.PropertyToID("_StippleTex");
        private static readonly int EdgeThickness = Shader.PropertyToID("_EdgeThickness");
        private static readonly int LuminanceTex = Shader.PropertyToID("_LuminanceTex");
        [Range(0f, 1f)]
        public float highThreshold = 0.3f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)] 
        public float stipplingBias;
        [Range(0f, 2f)]
        public float noiseScale = 1.0f;
        [Range(0, 5)]
        public int edgeThickness;

        private Material _inkMaterial;
        private Material _edgeDetectionMaterial;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _inkMaterial = new Material(Shader.Find("Custom/InkShader"));
            _edgeDetectionMaterial = new Material(Shader.Find("Custom/EdgeDetectionShader"));
            
            _edgeDetectionMaterial.SetFloat(HighThreshold, highThreshold);
            _edgeDetectionMaterial.SetFloat(LowThreshold, lowThreshold);
            _inkMaterial.SetFloat(StipplingBias, stipplingBias);
            _inkMaterial.SetFloat(NoiseScale, noiseScale);
            _inkMaterial.SetInt(EdgeThickness, edgeThickness);
            Texture2D blueNoiseTexture = Resources.Load<Texture2D>("LDR_RGBA_0");
            _inkMaterial.SetTexture(NoiseTexture, blueNoiseTexture);
            
            RenderTexture rtLuminance = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtSobel = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtGradientMagnitude = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtDoubleThreshold = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtHysteresis = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtStipple = new RenderTexture(context.width, context.height, 0);

            Graphics.Blit(source, rtLuminance, _edgeDetectionMaterial, 0);
            Graphics.Blit(rtLuminance, rtSobel, _edgeDetectionMaterial, 1);
            Graphics.Blit(rtSobel, rtGradientMagnitude, _edgeDetectionMaterial, 2);
            Graphics.Blit(rtGradientMagnitude, rtDoubleThreshold, _edgeDetectionMaterial, 3);
            Graphics.Blit(rtDoubleThreshold, rtHysteresis, _edgeDetectionMaterial, 4);
            Graphics.Blit(rtLuminance, rtStipple, _inkMaterial, 0);
            _inkMaterial.SetTexture(LuminanceTex, rtLuminance);
            _inkMaterial.SetTexture(StippleTex, rtStipple);
            Graphics.Blit(rtHysteresis, destination, _inkMaterial, 1);
            
            rtLuminance.Release();
            rtSobel.Release();
            rtGradientMagnitude.Release();
            rtDoubleThreshold.Release();
            rtHysteresis.Release();
            rtStipple.Release();
        }
    }
}