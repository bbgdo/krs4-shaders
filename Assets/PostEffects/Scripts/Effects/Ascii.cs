using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Ascii")]
    public class Ascii : BaseEffect
    {
        private static readonly int AsciiTex = Shader.PropertyToID("_AsciiTex");
        private static readonly int DownsampledTex = Shader.PropertyToID("_DownsampledTex");
        
        private Material _asciiMaterial;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _asciiMaterial = new Material(Shader.Find("Custom/AsciiShader"));
            
            // TODO: integrate _dogMaterial for better sobel operator for ED
            
            Texture2D asciiLod = Resources.Load<Texture2D>("1x0 8x8 3");
            _asciiMaterial.SetTexture(AsciiTex, asciiLod);
            
            RenderTexture rtD1 = new RenderTexture(context.width / 2, context.height / 2, 0);
            RenderTexture rtD2 = new RenderTexture(context.width / 4, context.height / 4, 0);
            RenderTexture rtD3 = new RenderTexture(context.width / 8, context.height / 8, 0);

            Graphics.Blit(source, rtD1, _asciiMaterial, 0);
            Graphics.Blit(rtD1, rtD2, _asciiMaterial, 0);
            Graphics.Blit(rtD2, rtD3, _asciiMaterial, 0);
            _asciiMaterial.SetTexture(DownsampledTex, rtD3);
            Graphics.Blit(source, destination, _asciiMaterial, 1);
            
            rtD1.Release();
            rtD2.Release();
            rtD3.Release();
        }
    }
}