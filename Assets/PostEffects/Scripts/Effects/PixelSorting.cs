using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/PixelSorting")]
    public class PixelSorting : BaseEffect
    {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");

        [Range(0, 1)] 
        public float lowThreshold = 0.5f;
        [Range(0, 1)] 
        public float highThreshold = 0.65f;

        private Material _material;
        private ComputeShader _computeShader;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            if (_material is null) {
                Shader shader = Shader.Find("Custom/PixelSortingShader");
                _material = new Material(shader);
            }

            _material.SetFloat(LowThreshold, lowThreshold);
            _material.SetFloat(HighThreshold, highThreshold);

            RenderTexture rtMask = new RenderTexture(context.width, context.height, 0) {
                enableRandomWrite = true
            };
            rtMask.Create();

            RenderTexture rtSorted = new RenderTexture(context.width, context.height, 0) {
                enableRandomWrite = true
            };
            rtSorted.Create();
            
            Graphics.Blit(source, destination, _material, 0);

            rtMask.Release();
            rtSorted.Release();
        }
    }
}