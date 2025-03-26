using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class DifferenceOfGaussians : MonoBehaviour
    {
        private static readonly int GaussianKernelSize = Shader.PropertyToID("_GaussianKernelSize");
        private static readonly int Sigma1 = Shader.PropertyToID("_Sigma1");
        private static readonly int Sigma2 = Shader.PropertyToID("_SigmaK");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
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
        
        private Material _dogMaterial;

        void Start() {
            Shader dogShader = Shader.Find("Custom/DoGShader");
            if (dogShader == null) {
                Debug.LogError("DoG.shader not found");
                return;
            }

            _dogMaterial = new Material(dogShader);
            
            _dogMaterial.SetInt(GaussianKernelSize, kernelSize);
            _dogMaterial.SetFloat(Sigma1, sigma1);
            _dogMaterial.SetFloat(Sigma2, sigma2);
            _dogMaterial.SetFloat(Threshold, threshold);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "dog_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            RenderTexture rtHorizontalBlur = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtVerticalBlur = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtDoG = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rtHorizontalBlur, _dogMaterial, 0);
            Graphics.Blit(rtHorizontalBlur, rtVerticalBlur, _dogMaterial, 1);
            Graphics.Blit(rtVerticalBlur, rtDoG, _dogMaterial, 2);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rtDoG;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            rtHorizontalBlur.Release();
            rtVerticalBlur.Release();
            rtDoG.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}