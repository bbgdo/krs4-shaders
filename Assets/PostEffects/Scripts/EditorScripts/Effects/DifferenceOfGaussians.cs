using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/DoG")]
    public class DifferenceOfGaussians : BaseEffect {
        private static readonly int GaussianKernelSize = Shader.PropertyToID("_GaussianKernelSize");
        private static readonly int Sigma1 = Shader.PropertyToID("_Sigma1");
        private static readonly int Sigma2 = Shader.PropertyToID("_SigmaK");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        [Range(0, 10)]
        public int kernelSize = 5;
        [Range(0.0f, 5.0f)]
        public float sigma1 = 1.0f;
        [Range(0.0f, 5.0f)]
        public float sigma2 = 1.5f;
        [Range(0.0f, 1.0f)] 
        public float threshold;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/DoGShader"));
            
            _material.SetInt(GaussianKernelSize, kernelSize);
            _material.SetFloat(Sigma1, sigma1);
            _material.SetFloat(Sigma2, sigma2);
            _material.SetFloat(Threshold, threshold);
            
            RenderTexture rtHorizontalBlur = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtVerticalBlur = new RenderTexture(context.width, context.height, 0);
            
            Graphics.Blit(source, rtHorizontalBlur, _material, 0);
            Graphics.Blit(rtHorizontalBlur, rtVerticalBlur, _material, 1);
            Graphics.Blit(rtVerticalBlur, destination, _material, 2);
            
            rtHorizontalBlur.Release();
            rtVerticalBlur.Release();
        }
    }
}