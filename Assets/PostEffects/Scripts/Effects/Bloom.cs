using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Bloom")]
    public class Bloom : BaseEffect {
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int Knee = Shader.PropertyToID("_Knee");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int BloomTex = Shader.PropertyToID("_BloomTex");

        [Range(0, 10)]
        public int downsamples;
        [Range(0, 1)] 
        public float threshold = 0.8f;
        [Range(0, 1)]
        public float knee = 0.2f;
        [Range(0, 5)]
        public int radius = 0;
        [Range(0, 1)]
        public float intensity = 1.0f;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Custom/BloomShader"));

            _material.SetFloat(Threshold, threshold);
            _material.SetFloat(Knee, knee);
            _material.SetFloat(Radius, radius);
            _material.SetFloat(Intensity, intensity);
            
            int width = context.width;
            int height = context.height;
            
            int i = 0;
            RenderTexture[] rtSampler = new RenderTexture[downsamples + 1];
            rtSampler[i] = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(source, rtSampler[i], _material, 0);
            for (i = 1; i <= downsamples; i++) {
                width /= 2;
                height /= 2;
                
                rtSampler[i] = RenderTexture.GetTemporary(width, height, 0);
                Graphics.Blit(rtSampler[i-1], rtSampler[i], _material, 1);
            }
            
            for (i = downsamples - 1; i >= 0; i--) {
                RenderTexture upsample = RenderTexture.GetTemporary(rtSampler[i].width, rtSampler[i].height, 0);
                
                Graphics.Blit(rtSampler[i + 1], upsample, _material, 2);

                _material.SetTexture(BloomTex, upsample);
                RenderTexture temp = RenderTexture.GetTemporary(rtSampler[i].width, rtSampler[i].height, 0);
                Graphics.Blit(rtSampler[i], temp, _material, 3);

                rtSampler[i].Release();
                rtSampler[i] = temp;

                upsample.Release();
            }
            _material.SetTexture(BloomTex, rtSampler[0]);
            
            Graphics.Blit(source, destination, _material, 4);
            
            foreach (RenderTexture curRt in rtSampler) {
                curRt.Release();
            }
        }
    }
}