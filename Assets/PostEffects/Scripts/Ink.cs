using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class Ink : MonoBehaviour {
        private static readonly int ReduceNoise = Shader.PropertyToID("_ReduceNoise");
        private static readonly int LowThreshold = Shader.PropertyToID("_LowThreshold");
        private static readonly int HighThreshold = Shader.PropertyToID("_HighThreshold");
        private static readonly int NoiseTexture = Shader.PropertyToID("_NoiseTex");
        private static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        private static readonly int StippleTex = Shader.PropertyToID("_StippleTex");
        private static readonly int EdgeThickness = Shader.PropertyToID("_EdgeThickness");
        private static readonly int LuminanceTex = Shader.PropertyToID("_LuminanceTex");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0f, 1f)]
        public float highThreshold = 0.4f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)]
        public int reduceNoise;
        [Range(0f, 2f)]
        public float noiseScale = 1.0f;
        [Range(0, 5)]
        public int edgeThickness = 0;

        private Material _inkMaterial;

        void Start() {
            Debug.Log("Ink.Start() called");
            Shader inkShader = Shader.Find("Custom/InkShader");
            if (inkShader == null) {
                Debug.LogError("Ink.shader not found");
                return;
            }
            _inkMaterial = new Material(inkShader);
            _inkMaterial.SetFloat(HighThreshold, highThreshold);
            _inkMaterial.SetFloat(LowThreshold, lowThreshold);
            _inkMaterial.SetFloat(ReduceNoise, reduceNoise);
            _inkMaterial.SetFloat(NoiseScale, noiseScale);
            _inkMaterial.SetFloat(EdgeThickness, edgeThickness);
            
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
            if (blueNoiseTexture == null) {
                Debug.LogError("Failed to load BlueNoise from Resources!");
            } else {
                _inkMaterial.SetTexture(NoiseTexture, blueNoiseTexture);
            }

            RenderTexture rt0 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt1 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt2 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt3 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt4 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt5 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt6 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt0, _inkMaterial, 0);
            Graphics.Blit(rt0, rt1, _inkMaterial, 1);
            Graphics.Blit(rt1, rt2, _inkMaterial, 2);
            Graphics.Blit(rt2, rt3, _inkMaterial, 3);
            Graphics.Blit(rt3, rt4, _inkMaterial, 4);
            Graphics.Blit(rt0, rt5, _inkMaterial, 5);
            _inkMaterial.SetTexture(LuminanceTex, rt0);
            _inkMaterial.SetTexture(StippleTex, rt5);
            Graphics.Blit(rt4, rt6, _inkMaterial, 6);

            Debug.Log("Apply result");
            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt6;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            rt0.Release();
            rt1.Release();
            rt2.Release();
            rt3.Release();
            rt4.Release();
            rt5.Release();
            rt6.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}