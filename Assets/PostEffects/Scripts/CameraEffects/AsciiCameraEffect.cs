using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    
    [AddComponentMenu("Custom PostFX/ASCII")]
    public class AsciiCameraEffect : MonoBehaviour {
        private static readonly int AsciiTex = Shader.PropertyToID("_AsciiTex");
        private static readonly int DownsampledTex = Shader.PropertyToID("_DownsampledTex");
        private static readonly int GaussianKernelSize = Shader.PropertyToID("_GaussianKernelSize");
        private static readonly int Sigma1 = Shader.PropertyToID("_Sigma1");
        private static readonly int Sigma2 = Shader.PropertyToID("_SigmaK");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int AsciiEdgeTex = Shader.PropertyToID("_AsciiEdgeTex");

        [Range(0, 10)] public int kernelSize = 5;
        [Range(0f, 5f)] public float sigma1 = 1.0f;
        [Range(0f, 5f)] public float sigma2 = 1.5f;
        [Range(0f, 1f)] public float threshold;

        private Material _asciiMaterial;
        private Material _dogMaterial;
        private static Texture2D _asciiLuT;
        private static Texture2D _asciiEdgeLuT;

        void OnEnable() {
            _asciiMaterial ??= new Material(Shader.Find("Custom/AsciiShader"));
            _dogMaterial ??= new Material(Shader.Find("Custom/DoGShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_asciiMaterial == null || _dogMaterial == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _asciiLuT ??= Resources.Load<Texture2D>("1x0 8x8 3");
            _asciiEdgeLuT ??= Resources.Load<Texture2D>("edges 8x40");
            _asciiMaterial.SetTexture(AsciiTex, _asciiLuT);
            _asciiMaterial.SetTexture(AsciiEdgeTex, _asciiEdgeLuT);
            _dogMaterial.SetInt(GaussianKernelSize, kernelSize);
            _dogMaterial.SetFloat(Sigma1, sigma1);
            _dogMaterial.SetFloat(Sigma2, sigma2);
            _dogMaterial.SetFloat(Threshold, threshold);

            RenderTexture rtD1 = RenderTexture.GetTemporary(source.width / 2, source.height / 2);
            RenderTexture rtD2 = RenderTexture.GetTemporary(source.width / 4, source.height / 4);
            RenderTexture rtDFill3 = RenderTexture.GetTemporary(source.width / 8, source.height / 8);
            RenderTexture rtDEdge3 = RenderTexture.GetTemporary(source.width / 8, source.height / 8);

            Graphics.Blit(source, rtD1, _asciiMaterial, 0);
            Graphics.Blit(rtD1, rtD2, _asciiMaterial, 0);
            Graphics.Blit(rtD2, rtDFill3, _asciiMaterial, 0);
            _asciiMaterial.SetTexture(DownsampledTex, rtDFill3);

            RenderTexture rtHorizontalBlur = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtVerticalBlur = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtDoG = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtEdges = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture rtEdgesDownsampled = RenderTexture.GetTemporary(source.width, source.height);

            Graphics.Blit(source, rtHorizontalBlur, _dogMaterial, 0);
            Graphics.Blit(rtHorizontalBlur, rtVerticalBlur, _dogMaterial, 1);
            Graphics.Blit(rtVerticalBlur, rtDoG, _dogMaterial, 2);
            Graphics.Blit(rtDoG, rtEdges, _asciiMaterial, 1);

            Graphics.Blit(rtEdges, rtD1, _asciiMaterial, 0);
            Graphics.Blit(rtD1, rtD2, _asciiMaterial, 0);
            Graphics.Blit(rtD2, rtDEdge3, _asciiMaterial, 0);
            Graphics.Blit(rtDEdge3, rtEdgesDownsampled, _asciiMaterial, 0);

            Graphics.Blit(rtEdgesDownsampled, destination, _asciiMaterial, 2);

            rtD1.Release(); 
            rtD2.Release(); 
            rtDFill3.Release(); 
            rtDEdge3.Release();
            rtHorizontalBlur.Release(); 
            rtVerticalBlur.Release(); 
            rtDoG.Release(); 
            rtEdges.Release(); 
            rtEdgesDownsampled.Release();
        }

        void OnDisable() {
            if (_asciiMaterial != null) DestroyImmediate(_asciiMaterial);
            if (_dogMaterial != null) DestroyImmediate(_dogMaterial);
        }
    }
}