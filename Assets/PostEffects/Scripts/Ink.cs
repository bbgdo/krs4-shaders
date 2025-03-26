using System.IO;
using UnityEngine;

namespace PostEffects.Scripts {
    public class Ink : MonoBehaviour {
        private static readonly int ReduceNoise = Shader.PropertyToID("_ReduceNoise");
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int StipplingBias = Shader.PropertyToID("_StipplingBias");
        private static readonly int NoiseTexture = Shader.PropertyToID("_NoiseTex");
        private static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        private static readonly int StippleTex = Shader.PropertyToID("_StippleTex");
        private static readonly int EdgeThickness = Shader.PropertyToID("_EdgeThickness");
        private static readonly int LuminanceTex = Shader.PropertyToID("_LuminanceTex");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0f, 1f)]
        public float highThreshold = 0.3f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)]
        public int reduceNoise;
        [Range(0, 1)] 
        public float stipplingBias;
        [Range(0f, 2f)]
        public float noiseScale = 1.0f;
        [Range(0, 5)]
        public int edgeThickness;

        private Material _inkMaterial;
        private Material _edgeDetectionMaterial;

        void Start() {
            Shader inkShader = Shader.Find("Custom/InkShader");
            if (inkShader == null) {
                Debug.LogError("Ink.shader not found");
                return;
            }
            Shader edShader = Shader.Find("Custom/EdgeDetectionShader");
            if (edShader == null) {
                Debug.LogError("EdgeDetection.shader not found");
                return;
            }
            
            _inkMaterial = new Material(inkShader);
            _edgeDetectionMaterial = new Material(edShader);
            
            _edgeDetectionMaterial.SetFloat(HighThreshold, highThreshold);
            _edgeDetectionMaterial.SetFloat(LowThreshold, lowThreshold);
            _edgeDetectionMaterial.SetInt(ReduceNoise, reduceNoise);
            _inkMaterial.SetFloat(StipplingBias, stipplingBias);
            _inkMaterial.SetFloat(NoiseScale, noiseScale);
            _inkMaterial.SetInt(EdgeThickness, edgeThickness);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "ink_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            Texture2D blueNoiseTexture = Resources.Load<Texture2D>("LDR_RGBA_0");
            _inkMaterial.SetTexture(NoiseTexture, blueNoiseTexture);

            RenderTexture rtLuminanceGauss = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtSobel = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtGradientMagnitude = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtDoubleThreshold = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtHysteresis = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtStipple = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rtCombine = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rtLuminanceGauss, _edgeDetectionMaterial, 0);
            Graphics.Blit(rtLuminanceGauss, rtSobel, _edgeDetectionMaterial, 1);
            Graphics.Blit(rtSobel, rtGradientMagnitude, _edgeDetectionMaterial, 2);
            Graphics.Blit(rtGradientMagnitude, rtDoubleThreshold, _edgeDetectionMaterial, 3);
            Graphics.Blit(rtDoubleThreshold, rtHysteresis, _edgeDetectionMaterial, 4);
            Graphics.Blit(rtLuminanceGauss, rtStipple, _inkMaterial, 0);
            _inkMaterial.SetTexture(LuminanceTex, rtLuminanceGauss);
            _inkMaterial.SetTexture(StippleTex, rtStipple);
            Graphics.Blit(rtHysteresis, rtCombine, _inkMaterial, 1);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rtCombine;
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
            rtStipple.Release();
            rtCombine.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}