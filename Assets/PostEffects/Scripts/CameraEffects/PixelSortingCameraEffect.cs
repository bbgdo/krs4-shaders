using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Pixel sorting")]
    public class PixelSortingCameraEffect : MonoBehaviour {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int SortedTex = Shader.PropertyToID("_SortedTex");
        private static readonly int SortAxis = Shader.PropertyToID("sort_axis");
        private static readonly int Direction = Shader.PropertyToID("sort_direction");
        private static readonly int ComputeInput = Shader.PropertyToID("input");
        private static readonly int ComputeOutput = Shader.PropertyToID("output");

        [Range(0, 1)] public float lowThreshold = 0.5f;
        [Range(0, 1)] public float highThreshold = 0.65f;
        public enum SortDirection { Up, Down, Left, Right }
        public SortDirection direction;
        public ComputeShader pixelSorter;

        private Material _material;
        private int _kernel;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/PixelSortingShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null || pixelSorter == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetFloat(LowThreshold, lowThreshold);
            _material.SetFloat(HighThreshold, highThreshold);

            RenderTexture rtMask = new RenderTexture(source.width, source.height, 0) {
                enableRandomWrite = true
            };
            rtMask.Create();
            RenderTexture rtSorted = new RenderTexture(source.width, source.height, 0) {
                enableRandomWrite = true
            };
            rtSorted.Create();

            Graphics.Blit(source, rtMask, _material, 0);

            _kernel = pixelSorter.FindKernel("CS_pixel_sorting");
            int axis = direction is SortDirection.Left or SortDirection.Right ? 1 : 0;
            int dir = direction is SortDirection.Down or SortDirection.Left ? 0 : 1;

            pixelSorter.SetInt(SortAxis, axis);
            pixelSorter.SetInt(Direction, dir);
            pixelSorter.SetTexture(_kernel, ComputeInput, rtMask);
            pixelSorter.SetTexture(_kernel, ComputeOutput, rtSorted);

            if (axis == 0)
                pixelSorter.Dispatch(_kernel, source.width, 1, 1);
            else
                pixelSorter.Dispatch(_kernel, 1, source.height, 1);

            _material.SetTexture(SortedTex, rtSorted);
            Graphics.Blit(source, destination, _material, 1);

            RenderTexture.ReleaseTemporary(rtMask);
            RenderTexture.ReleaseTemporary(rtSorted);
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}