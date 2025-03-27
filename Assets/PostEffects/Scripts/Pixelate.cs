using System;
using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class Pixelate : MonoBehaviour
    {
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0, 10)]
        public int downsamples;
        
        private Material _pixelateMaterial;

        void Start() {
            Shader pixelateShader = Shader.Find("Custom/PixelateShader");
            if (pixelateShader == null) {
                Debug.LogError("Pixelate.shader not found");
                return;
            }
            
            _pixelateMaterial = new Material(pixelateShader);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "pix_" + fileName);

            if (!File.Exists(inputFile)) {
                Debug.LogError("File not found: " + inputFile);
                return;
            }

            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            int width = texture.width;
            int height = texture.height;

            int i = 0;
            RenderTexture[] rt = new RenderTexture[downsamples + 1];
            rt[i] = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(texture, rt[i]);
            for (i = 1; i <= downsamples; i++) {
                width /= 2;
                height /= 2;
                
                rt[i] = RenderTexture.GetTemporary(width, height, 0);
                Graphics.Blit(rt[i-1], rt[i], _pixelateMaterial, 0);
            }
            Texture2D resultTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture.active = rt[--i];
            resultTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            resultTexture.Apply();
            
            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            foreach (RenderTexture curRt in rt) {
                curRt.Release();
            }
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}