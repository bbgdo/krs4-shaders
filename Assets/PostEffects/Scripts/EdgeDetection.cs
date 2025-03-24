using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class EdgeDetection : MonoBehaviour
    {
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0f, 1f)]
        public float highThreshold = 0.4f;
        [Range(0f, 1f)]
        public float lowThreshold = 0.1f;
        [Range(0, 1)]
        public int reduceNoise = 0;

        private Material _edgeDetectionMaterial;

        void Start()
        {
            Shader edShader = Shader.Find("Custom/EdgeDetectionShader");
            if (edShader == null)
            {
                Debug.LogError("EdgeDetection.shader not found");
                return;
            }

            _edgeDetectionMaterial = new Material(edShader);
            _edgeDetectionMaterial.SetFloat("_HighThreshold", highThreshold);
            _edgeDetectionMaterial.SetFloat("_LowThreshold", lowThreshold);
            _edgeDetectionMaterial.SetFloat("_ReduceNoise", reduceNoise);
            ProcessImage();
        }

        void ProcessImage()
        {
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

            RenderTexture rt0 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt1 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt2 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt3 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt4 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt0, _edgeDetectionMaterial, 0);
            _edgeDetectionMaterial.SetTexture("_NoiseReducedTex", rt0);
            Graphics.Blit(rt0, rt1, _edgeDetectionMaterial, 1);
            _edgeDetectionMaterial.SetTexture("_SobelGradientTex", rt1);
            Graphics.Blit(rt1, rt2, _edgeDetectionMaterial, 2);
            _edgeDetectionMaterial.SetTexture("_ThresholdGradientTex", rt2);
            Graphics.Blit(rt2, rt3, _edgeDetectionMaterial, 3);
            _edgeDetectionMaterial.SetTexture("_DoubleThresholdTex", rt3);
            Graphics.Blit(rt3, rt4, _edgeDetectionMaterial, 4);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt4;
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
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}