using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/PixelSorting")]
    public class PixelSorting : BaseEffect
    {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int SortedTex = Shader.PropertyToID("_SortedTex");

        [Range(0, 1)] 
        public float lowThreshold = 0.5f;
        [Range(0, 1)] 
        public float highThreshold = 0.65f;

        public ComputeShader pixelSorter;
        
        private int _sortPixelsKernel;
        private Material _material;

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
            
            Graphics.Blit(source, rtMask, _material, 0);
            
            _sortPixelsKernel = pixelSorter.FindKernel("CS_pixel_sorting");
            
            pixelSorter.SetTexture(_sortPixelsKernel, "input", rtMask);
            pixelSorter.SetTexture(_sortPixelsKernel, "output", rtSorted);
            
            pixelSorter.Dispatch(_sortPixelsKernel, Mathf.CeilToInt(context.width / 8.0f), 1, 1);
            
            _material.SetTexture(SortedTex, rtSorted);
            
            Graphics.Blit(source, destination, _material, 1);

            rtMask.Release();
            rtSorted.Release();
        }
    }
}