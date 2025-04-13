using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Retro")]
    public class Retro : BaseEffect {
        private static readonly int Colors = Shader.PropertyToID("_Colors");
        private static readonly int Spread = Shader.PropertyToID("_Spread");
        [Range(0, 255)]
        public int colors = 255;
        [Range(0, 10)]
        public int downsamples;
        [Range(0.0f, 1.0f)]
        public float spread = 0.5f;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/RetroShader"));
            
            _material.SetInt(Colors, colors);
            _material.SetFloat(Spread, spread);
            
            int width = context.width;
            int height = context.height;
            
            int i = 0;
            RenderTexture[] rtDownsampler = new RenderTexture[downsamples + 1];
            rtDownsampler[i] = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(source, rtDownsampler[i]);
            for (i = 1; i <= downsamples; i++) {
                width /= 2;
                height /= 2;
                
                rtDownsampler[i] = RenderTexture.GetTemporary(width, height, 0);
                Graphics.Blit(rtDownsampler[i-1], rtDownsampler[i], _material, 0);
            }
            
            Graphics.Blit(rtDownsampler[--i], destination, _material, 1);
            
            foreach (RenderTexture curRt in rtDownsampler) {
                curRt.Release();
            }
        }
    }
}