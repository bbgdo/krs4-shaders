using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Sharpen")]
    public class Sharpen : BaseEffect
    {
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");

        [Range(0, 1)] 
        public float intensity = 1.0f;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/SharpenShader"));

            _material.SetFloat(Intensity, intensity);
            Graphics.Blit(source, destination, _material);
        }
    }
}