using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class SmartPixel : MonoBehaviour {
        private static readonly int Downsampled = Shader.PropertyToID("_Downsampled");
        private static readonly int EdgeMap = Shader.PropertyToID("_EdgeMap");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(1, 100)] 
        public int downScale;
        
        private Material _smartPixelMaterial;

        void Start() {
            Shader smartPixelShader = Shader.Find("Custom/SmartPixelShader");
            if (smartPixelShader == null) {
                Debug.LogError("SmartPixel.shader not found");
                return;
            }
            _smartPixelMaterial = new Material(smartPixelShader);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "sp_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            int width = texture.width / downScale;
            int height = texture.height / downScale;

            RenderTexture rt0 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt1 = new RenderTexture(width, height, 0) {
                filterMode = FilterMode.Point
            };
            rt1.Create();
            RenderTexture rt2 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt0, _smartPixelMaterial, 0);
            Graphics.Blit(texture, rt1, _smartPixelMaterial, 1);
            _smartPixelMaterial.SetTexture(Downsampled, rt1);
            _smartPixelMaterial.SetTexture(EdgeMap, rt0);
            Graphics.Blit(texture, rt2, _smartPixelMaterial, 2);

            Debug.Log("Apply result");
            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt2;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            // resultTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            rt0.Release();
            rt1.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}