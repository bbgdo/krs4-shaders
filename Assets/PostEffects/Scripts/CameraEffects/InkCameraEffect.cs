using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Ink")]
    public class InkCameraEffect : MonoBehaviour {
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int StipplingBias = Shader.PropertyToID("_StipplingBias");
        private static readonly int NoiseTexture = Shader.PropertyToID("_NoiseTex");
        private static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        private static readonly int StippleTex = Shader.PropertyToID("_StippleTex");
        private static readonly int EdgeThickness = Shader.PropertyToID("_EdgeThickness");
        private static readonly int LuminanceTex = Shader.PropertyToID("_LuminanceTex");

        [Range(0f, 1f)]
        public float highThreshold = 0.3f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)] 
        public float stipplingBias;
        [Range(0f, 2f)] 
        public float noiseScale = 1.0f;
        [Range(0, 5)]
        public int edgeThickness;

        private Material _inkMaterial;
        private Material _edgeMaterial;
        private static Texture2D _noise;

        void OnEnable() {
            _inkMaterial ??= new Material(Shader.Find("Custom/InkShader"));
            _edgeMaterial ??= new Material(Shader.Find("Custom/EdgeDetectionShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_inkMaterial == null || _edgeMaterial == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _edgeMaterial.SetFloat(HighThreshold, highThreshold);
            _edgeMaterial.SetFloat(LowThreshold, lowThreshold);
            _inkMaterial.SetFloat(StipplingBias, stipplingBias);
            _inkMaterial.SetFloat(NoiseScale, noiseScale);
            _inkMaterial.SetInt(EdgeThickness, edgeThickness);
            _noise ??= Resources.Load<Texture2D>("LDR_RGBA_0");
            _inkMaterial.SetTexture(NoiseTexture, _noise);

            RenderTexture rtLuminance = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtSobel = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtGradientMagnitude = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtDoubleThreshold = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtHysteresis = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtStipple = RenderTexture.GetTemporary(source.width, source.height);

            Graphics.Blit(source, rtLuminance, _edgeMaterial, 0);
            Graphics.Blit(rtLuminance, rtSobel, _edgeMaterial, 1);
            Graphics.Blit(rtSobel, rtGradientMagnitude, _edgeMaterial, 2);
            Graphics.Blit(rtGradientMagnitude, rtDoubleThreshold, _edgeMaterial, 3);
            Graphics.Blit(rtDoubleThreshold, rtHysteresis, _edgeMaterial, 4);
            Graphics.Blit(rtLuminance, rtStipple, _inkMaterial, 0);
            _inkMaterial.SetTexture(LuminanceTex, rtLuminance);
            _inkMaterial.SetTexture(StippleTex, rtStipple);
            Graphics.Blit(rtHysteresis, destination, _inkMaterial, 1);

            rtLuminance.Release(); 
            rtSobel.Release(); 
            rtGradientMagnitude.Release(); 
            rtDoubleThreshold.Release(); 
            rtHysteresis.Release(); 
            rtStipple.Release();
        }

        void OnDisable() {
            if (_inkMaterial != null) DestroyImmediate(_inkMaterial);
            if (_edgeMaterial != null) DestroyImmediate(_edgeMaterial);
        }
    }
}