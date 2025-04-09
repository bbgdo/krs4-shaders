using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Sharpen")]
    public class Sharpen : BaseEffect
    {
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");

        [Range(0, 1)] 
        public float intensity = 1.0f;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            if (_material is null)
            {
                Shader shader = Shader.Find("Custom/SharpenShader");
                _material = new Material(shader);
            }

            _material.SetFloat(Intensity, intensity);
            Graphics.Blit(source, destination, _material);
        }
    }
}