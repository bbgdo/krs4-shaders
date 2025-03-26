using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class EdgeDetection : MonoBehaviour
    {
        private static readonly int ReduceNoise = Shader.PropertyToID("_ReduceNoise");
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0f, 1f)]
        public float highThreshold = 0.4f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)]
        public int reduceNoise;

        private Material _edgeDetectionMaterial;

        void Start() {
            Shader edShader = Shader.Find("Custom/EdgeDetectionShader");
            if (edShader == null) {
                Debug.LogError("EdgeDetection.shader not found");
                return;
            }

            _edgeDetectionMaterial = new Material(edShader);
            
            _edgeDetectionMaterial.SetFloat(HighThreshold, highThreshold);
            _edgeDetectionMaterial.SetFloat(LowThreshold, lowThreshold);
            _edgeDetectionMaterial.SetInt(ReduceNoise, reduceNoise);
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "ed_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            RenderTexture rtLuminanceGauss = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtSobel = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtGradientMagnitude = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtDoubleThreshold = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtHysteresis = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rtLuminanceGauss, _edgeDetectionMaterial, 0);
            Graphics.Blit(rtLuminanceGauss, rtSobel, _edgeDetectionMaterial, 1);
            Graphics.Blit(rtSobel, rtGradientMagnitude, _edgeDetectionMaterial, 2);
            Graphics.Blit(rtGradientMagnitude, rtDoubleThreshold, _edgeDetectionMaterial, 3);
            Graphics.Blit(rtDoubleThreshold, rtHysteresis, _edgeDetectionMaterial, 4);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rtHysteresis;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            rtLuminanceGauss.Release();
            rtSobel.Release();
            rtGradientMagnitude.Release();
            rtDoubleThreshold.Release();
            rtHysteresis.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}