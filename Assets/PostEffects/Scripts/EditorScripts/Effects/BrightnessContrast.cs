using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/BrightnessContrast")]
    public class BrightnessContrast : BaseEffect
    {
        private static readonly int Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int Contrast = Shader.PropertyToID("_Contrast");

        [Range(0, 2)] 
        public float contrast = 1.0f;
        [Range(-1, 1)] 
        public float brightness;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/BrightnessContrastShader"));

            _material.SetFloat(Contrast, contrast);
            _material.SetFloat(Brightness, brightness);
            Graphics.Blit(source, destination, _material);
        }
    }
}