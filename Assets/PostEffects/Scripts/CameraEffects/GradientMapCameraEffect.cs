using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Gradient map")]
    public class GradientMapCameraEffect : MonoBehaviour {
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int GradientTex = Shader.PropertyToID("_GradientTex");

        public Texture2D gradientTexture;
        public Gradient gradient = new Gradient();
        [Range(0, 512)]
        public int gradientResolution = 256;
        [Range(0, 1)] 
        public float intensity = 1.0f;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/GradientMapShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            Texture2D usedTex = gradientTexture ? gradientTexture : GenerateGradientTexture();
            _material.SetTexture(GradientTex, usedTex);
            _material.SetFloat(Intensity, intensity);
            Graphics.Blit(source, destination, _material);
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
        
        private Texture2D GenerateGradientTexture() {
            var generatedGradient = new Texture2D(gradientResolution, 1, TextureFormat.RGBA32, false) {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int i = 0; i < gradientResolution; i++)
            {
                float t = i / (gradientResolution - 1f);
                Color color = gradient.Evaluate(t);
                generatedGradient.SetPixel(i, 0, color);
            }

            generatedGradient.Apply();
            return generatedGradient;
        }
    }
}