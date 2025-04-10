using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/EdgeDetection")]
    public class EdgeDetection : BaseEffect
    {
        private static readonly int ReduceNoise = Shader.PropertyToID("_ReduceNoise");
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        [Range(0f, 1f)]
        public float highThreshold = 0.4f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)]
        public int reduceNoise;

        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            if (_material is null) {
                Shader shader = Shader.Find("Custom/EdgeDetectionShader");
                _material = new Material(shader);
            }
            
            _material.SetFloat(HighThreshold, highThreshold);
            _material.SetFloat(LowThreshold, lowThreshold);
            _material.SetInt(ReduceNoise, reduceNoise);
            
            RenderTexture rtLuminanceGauss = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtSobel = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtGradientMagnitude = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtDoubleThreshold = new RenderTexture(context.width, context.height, 0);
        
            Graphics.Blit(source, rtLuminanceGauss, _material, 0);
            Graphics.Blit(rtLuminanceGauss, rtSobel, _material, 1);
            Graphics.Blit(rtSobel, rtGradientMagnitude, _material, 2);
            Graphics.Blit(rtGradientMagnitude, rtDoubleThreshold, _material, 3);
            Graphics.Blit(rtDoubleThreshold, destination, _material, 4);
            
            rtLuminanceGauss.Release();
            rtSobel.Release();
            rtGradientMagnitude.Release();
            rtDoubleThreshold.Release();
        }
    }
}