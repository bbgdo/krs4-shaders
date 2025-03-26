using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class Ascii : MonoBehaviour
    {
        private static readonly int GaussianKernelSize = Shader.PropertyToID("_GaussianKernelSize");
        private static readonly int Sigma1 = Shader.PropertyToID("_Sigma1");
        private static readonly int Sigma2 = Shader.PropertyToID("_SigmaK");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int AsciiTex = Shader.PropertyToID("_AsciiTex");
        private static readonly int DownsampledTex = Shader.PropertyToID("_DownsampledTex");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0, 10)]
        public int kernelSize = 5;
        [Range(0.0f, 5.0f)]
        public float sigma1 = 1.0f;
        [Range(0.0f, 5.0f)]
        public float sigma2 = 1.5f;
        [Range(0.0f, 1.0f)] 
        public float threshold;
        
        private Material _asciiMaterial;
        private Material _dogMaterial;

        void Start() {
            Shader asciiShader = Shader.Find("Custom/AsciiShader");
            if (asciiShader == null) {
                Debug.LogError("Ascii.shader not found");
                return;
            }
            Shader dogShader = Shader.Find("Custom/DoGShader");
            if (dogShader == null) {
                Debug.LogError("DoG.shader not found");
                return;
            }
            
            _asciiMaterial = new Material(asciiShader);
            _dogMaterial = new Material(dogShader);
            
            Texture2D asciiLod = Resources.Load<Texture2D>("1x0 8x8 3");
            _asciiMaterial.SetTexture(AsciiTex, asciiLod);
            Debug.Log($"ASCII Texture Size: {asciiLod.width}x{asciiLod.height}");
            
            _dogMaterial.SetInt(GaussianKernelSize, kernelSize);
            _dogMaterial.SetFloat(Sigma1, sigma1);
            _dogMaterial.SetFloat(Sigma2, sigma2);
            _dogMaterial.SetFloat(Threshold, threshold);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "ascii_" + fileName);

            if (!File.Exists(inputFile)) {
                Debug.LogError("File not found: " + inputFile);
                return;
            }

            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            RenderTexture rtD1 = new RenderTexture(texture.width / 2, texture.height / 2, 0) {
                filterMode = FilterMode.Point
            };
            RenderTexture rtD2 = new RenderTexture(texture.width / 4, texture.height / 4, 0) {
                filterMode = FilterMode.Point
            };
            RenderTexture rtD3 = new RenderTexture(texture.width / 8, texture.height / 8, 0) {
                filterMode = FilterMode.Point
            };
            RenderTexture rt1 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtHorizontalBlur = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtVerticalBlur = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtDoG = new RenderTexture(texture.width, texture.height, 0);
            Graphics.Blit(texture, rtD1, _asciiMaterial, 0);
            Graphics.Blit(rtD1, rtD2, _asciiMaterial, 0);
            Graphics.Blit(rtD2, rtD3, _asciiMaterial, 0);
            _asciiMaterial.SetTexture(DownsampledTex, rtD3);
            Graphics.Blit(texture, rt1, _asciiMaterial, 1);
            Graphics.Blit(texture, rtHorizontalBlur, _dogMaterial, 0);
            Graphics.Blit(rtHorizontalBlur, rtVerticalBlur, _dogMaterial, 1);
            Graphics.Blit(rtVerticalBlur, rtDoG, _dogMaterial, 2);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt1;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            // resultTexture.ReadPixels(new Rect(0, 0, texture.width / 8, texture.height / 8), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            rtD1.Release();
            rtD2.Release();
            rtD3.Release();
            rtHorizontalBlur.Release();
            rtVerticalBlur.Release();
            rtDoG.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}