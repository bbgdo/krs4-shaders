using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Bloom")]
    public class BloomCameraEffect : MonoBehaviour {
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int Knee = Shader.PropertyToID("_Knee");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int BloomTex = Shader.PropertyToID("_BloomTex");

        [Range(0, 10)] public int downsamples;
        [Range(0, 1)] public float threshold = 0.8f;
        [Range(0, 1)] public float knee = 0.2f;
        [Range(0, 5)] public int radius;
        [Range(0, 1)] public float intensity = 1.0f;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/BloomShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetFloat(Threshold, threshold);
            _material.SetFloat(Knee, knee);
            _material.SetFloat(Radius, radius);
            _material.SetFloat(Intensity, intensity);

            int width = source.width;
            int height = source.height;
            int i = 0;
            RenderTexture[] rtSampler = new RenderTexture[downsamples + 1];
            rtSampler[i] = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rtSampler[i], _material, 0);

            for (i = 1; i <= downsamples; i++) {
                width /= 2;
                height /= 2;
                rtSampler[i] = RenderTexture.GetTemporary(width, height);
                Graphics.Blit(rtSampler[i - 1], rtSampler[i], _material, 1);
            }

            for (i = downsamples - 1; i >= 0; i--) {
                var upsample = RenderTexture.GetTemporary(rtSampler[i].width, rtSampler[i].height);
                Graphics.Blit(rtSampler[i + 1], upsample, _material, 2);
                _material.SetTexture(BloomTex, upsample);
                var temp = RenderTexture.GetTemporary(rtSampler[i].width, rtSampler[i].height);
                Graphics.Blit(rtSampler[i], temp, _material, 3);
                rtSampler[i].Release();
                rtSampler[i] = temp;
                upsample.Release();
            }

            _material.SetTexture(BloomTex, rtSampler[0]);
            Graphics.Blit(source, destination, _material, 4);
            foreach (var r in rtSampler) r.Release();
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}