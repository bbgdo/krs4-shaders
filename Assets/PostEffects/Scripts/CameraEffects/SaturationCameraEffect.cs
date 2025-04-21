using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Saturation")]
    public class SaturationCameraEffect : MonoBehaviour {
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");

        [Range(0, 2)] 
        public float intensity = 1.0f;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/SaturationShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetFloat(Intensity, intensity);
            Graphics.Blit(source, destination, _material);
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}