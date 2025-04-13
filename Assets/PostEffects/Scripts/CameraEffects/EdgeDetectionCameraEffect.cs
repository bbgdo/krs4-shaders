using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Canny edge detection (Util)")]
    public class EdgeDetectionCameraEffect : MonoBehaviour {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");

        [Range(0f, 1f)] public float highThreshold = 0.4f;
        [Range(0f, 1f)] public float lowThreshold = 0.1f;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/EdgeDetectionShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetFloat(HighThreshold, highThreshold);
            _material.SetFloat(LowThreshold, lowThreshold);

            RenderTexture rtLuminance = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtSobel = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtGradientMagnitude = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtDoubleThreshold = RenderTexture.GetTemporary(source.width, source.height);

            Graphics.Blit(source, rtLuminance, _material, 0);
            Graphics.Blit(rtLuminance, rtSobel, _material, 1);
            Graphics.Blit(rtSobel, rtGradientMagnitude, _material, 2);
            Graphics.Blit(rtGradientMagnitude, rtDoubleThreshold, _material, 3);
            Graphics.Blit(rtDoubleThreshold, destination, _material, 4);

            rtLuminance.Release(); rtSobel.Release(); rtGradientMagnitude.Release(); rtDoubleThreshold.Release();
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}