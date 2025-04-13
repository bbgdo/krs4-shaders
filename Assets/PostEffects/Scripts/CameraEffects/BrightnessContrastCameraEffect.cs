using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Brightness & Contrast")]
    public class BrightnessContrastCameraEffect : MonoBehaviour {
        private static readonly int Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int Contrast = Shader.PropertyToID("_Contrast");

        [Range(0, 2)] public float contrast = 1.0f;
        [Range(-1, 1)] public float brightness;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/BrightnessContrastShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetFloat(Contrast, contrast);
            _material.SetFloat(Brightness, brightness);
            Graphics.Blit(source, destination, _material);
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}