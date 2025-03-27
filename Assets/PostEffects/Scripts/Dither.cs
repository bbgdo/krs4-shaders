using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class Dither : MonoBehaviour {
        private static readonly int Spread = Shader.PropertyToID("_Spread");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0.0f, 1.0f)]
        public float spread = 0.5f;
        
        private Material _ditherMaterial;

        void Start() {
            Shader ditherShader = Shader.Find("Custom/DitherShader");
            if (ditherShader == null) {
                Debug.LogError("Dither.shader not found");
                return;
            }
            _ditherMaterial = new Material(ditherShader);
            
            _ditherMaterial.SetFloat(Spread, spread);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "dither_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            RenderTexture rt0 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt0, _ditherMaterial, 0);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt0;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            rt0.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}