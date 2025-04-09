using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Saturation")]
    public class Saturation : BaseEffect {
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");

        [Range(0, 2)] 
        public float intensity = 1.0f;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            if (_material is null)
            {
                Shader shader = Shader.Find("Custom/SaturationShader");
                _material = new Material(shader);
            }

            _material.SetFloat(Intensity, intensity);
            Graphics.Blit(source, destination, _material);
        }
    }
}