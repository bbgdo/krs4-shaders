using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/PixelSorting")]
    public class PixelSorting : BaseEffect {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int SortedTex = Shader.PropertyToID("_SortedTex");
        private static readonly int SortAxis = Shader.PropertyToID("sort_axis");
        private static readonly int Direction = Shader.PropertyToID("sort_direction");
        private static readonly int ComputeInput = Shader.PropertyToID("input");
        private static readonly int ComputeOutput = Shader.PropertyToID("output");

        [Range(0, 1)] 
        public float lowThreshold = 0.5f;
        [Range(0, 1)] 
        public float highThreshold = 0.65f;
        public enum SortDirection {
            Up, Down, Left, Right
        }
        public SortDirection direction;

        public ComputeShader pixelSorter;
        
        private int _sortPixelsKernel;
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/PixelSortingShader"));
            
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
            int sortAxis = direction is SortDirection.Left or SortDirection.Right ? 1 : 0;
            int sortDir = direction is SortDirection.Down or SortDirection.Left ? 0 : 1;

            pixelSorter.SetInt(SortAxis, sortAxis);
            pixelSorter.SetInt(Direction, sortDir);
            pixelSorter.SetTexture(_sortPixelsKernel, ComputeInput, rtMask);
            pixelSorter.SetTexture(_sortPixelsKernel, ComputeOutput, rtSorted);
            
            if (sortAxis == 0) {
                pixelSorter.Dispatch(_sortPixelsKernel, context.width, 1, 1);
            } else {
                pixelSorter.Dispatch(_sortPixelsKernel, 1, context.height, 1);
            }            
            _material.SetTexture(SortedTex, rtSorted);
            
            Graphics.Blit(source, destination, _material, 1);
        
            rtMask.Release();
            rtSorted.Release();
        }
    }
}