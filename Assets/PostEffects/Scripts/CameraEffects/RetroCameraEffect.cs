using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Retro")]
    public class RetroCameraEffect : MonoBehaviour {
        private static readonly int Colors = Shader.PropertyToID("_Colors");
        private static readonly int Spread = Shader.PropertyToID("_Spread");

        [Range(0, 255)] public int colors = 255;
        [Range(0, 10)] public int downsamples;
        [Range(0f, 1f)] public float spread = 0.5f;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/RetroShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetInt(Colors, colors);
            _material.SetFloat(Spread, spread);

            int width = source.width;
            int height = source.height;
            int i = 0;
            RenderTexture[] rtDownsampler = new RenderTexture[downsamples + 1];
            rtDownsampler[i] = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rtDownsampler[i]);

            for (i = 1; i <= downsamples; i++) {
                width /= 2;
                height /= 2;
                rtDownsampler[i] = RenderTexture.GetTemporary(width, height);
                Graphics.Blit(rtDownsampler[i - 1], rtDownsampler[i], _material, 0);
            }

            Graphics.Blit(rtDownsampler[--i], destination, _material, 1);
            foreach (var curRt in rtDownsampler) RenderTexture.ReleaseTemporary(curRt);
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}