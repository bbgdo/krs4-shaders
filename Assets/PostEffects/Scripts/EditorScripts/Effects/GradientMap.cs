using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/GradientMap")]
    public class GradientMap : BaseEffect {
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int GradientTex = Shader.PropertyToID("_GradientTex");

        public Texture2D gradientTexture;
        public Gradient gradient = new Gradient();
        [Range(0, 512)]
        public int gradientResolution = 256;
        [Range(0, 1)]
        public float intensity = 1.0f;
        
        private Texture2D _generatedGradient;
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/GradientMapShader"));

            Texture2D usedTex = gradientTexture ? gradientTexture : GenerateGradientTexture();
            _material.SetTexture(GradientTex, usedTex);
            _material.SetFloat(Intensity, intensity);
            Graphics.Blit(source, destination, _material);
        }
        
        private Texture2D GenerateGradientTexture() {
            _generatedGradient = new Texture2D(gradientResolution, 1, TextureFormat.RGBA32, false) {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int i = 0; i < gradientResolution; i++)
            {
                float t = i / (gradientResolution - 1f);
                Color color = gradient.Evaluate(t);
                _generatedGradient.SetPixel(i, 0, color);
            }

            _generatedGradient.Apply();
            return _generatedGradient;
        }
    }
}