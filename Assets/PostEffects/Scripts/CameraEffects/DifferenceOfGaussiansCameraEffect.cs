using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Difference of Gaussians (Util)")]
    public class DifferenceOfGaussiansCameraEffect : MonoBehaviour {
        private static readonly int GaussianKernelSize = Shader.PropertyToID("_GaussianKernelSize");
        private static readonly int Sigma1 = Shader.PropertyToID("_Sigma1");
        private static readonly int Sigma2 = Shader.PropertyToID("_SigmaK");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");

        [Range(0, 10)] 
        public int kernelSize = 5;
        [Range(0f, 5f)] 
        public float sigma1 = 1.0f;
        [Range(0f, 5f)] 
        public float sigma2 = 1.5f;
        [Range(0f, 1f)] 
        public float threshold;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/DoGShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetInt(GaussianKernelSize, kernelSize);
            _material.SetFloat(Sigma1, sigma1);
            _material.SetFloat(Sigma2, sigma2);
            _material.SetFloat(Threshold, threshold);

            RenderTexture rtHorizontalBlur = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtVerticalBlur = RenderTexture.GetTemporary(source.width, source.height);

            Graphics.Blit(source, rtHorizontalBlur, _material, 0);
            Graphics.Blit(rtHorizontalBlur, rtVerticalBlur, _material, 1);
            Graphics.Blit(rtVerticalBlur, destination, _material, 2);

            RenderTexture.ReleaseTemporary(rtHorizontalBlur);
            RenderTexture.ReleaseTemporary(rtVerticalBlur);
        }

        void OnDisable() {
            if (_material != null) DestroyImmediate(_material);
        }
    }
}