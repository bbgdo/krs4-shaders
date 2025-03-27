using UnityEngine;
using System.IO;
using UnityEngine.Serialization;

namespace PostEffects.Scripts {
    public class Retro : MonoBehaviour {
        private static readonly int Colors = Shader.PropertyToID("_Colors");
        private static readonly int Spread = Shader.PropertyToID("_Spread");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [Range(0, 256)]
        public int colors = 256;
        [Range(0, 10)]
        public int downsamples;
        [Range(0.0f, 1.0f)]
        public float spread = 0.5f;
        
        private Material _pixelateMaterial;
        private Material _ditherMaterial;

        void Start() {
            Shader pixelateShader = Shader.Find("Custom/PixelateShader");
            if (pixelateShader == null) {
                Debug.LogError("Pixelate.shader not found");
                return;
            }
            _pixelateMaterial = new Material(pixelateShader);

            Shader ditherShader = Shader.Find("Custom/DitherShader");
            if (ditherShader == null) {
                Debug.LogError("Dither.shader not found");
                return;
            }
            _ditherMaterial = new Material(ditherShader);
            
            _ditherMaterial.SetInt(Colors, colors);
            _ditherMaterial.SetFloat(Spread, spread);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "retro_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            int width = texture.width;
            int height = texture.height;

            int i = 0;
            RenderTexture[] rtDownsampler = new RenderTexture[downsamples + 1];
            rtDownsampler[i] = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(texture, rtDownsampler[i]);
            for (i = 1; i <= downsamples; i++) {
                width /= 2;
                height /= 2;
                
                rtDownsampler[i] = RenderTexture.GetTemporary(width, height, 0);
                Graphics.Blit(rtDownsampler[i-1], rtDownsampler[i], _pixelateMaterial, 0);
            }
            
            RenderTexture rtDither = new RenderTexture(texture.width, texture.height, 0);

            Graphics.Blit(rtDownsampler[--i], rtDither, _ditherMaterial, 0);
            
            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rtDither;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            foreach (RenderTexture curRt in rtDownsampler) {
                curRt.Release();
            }
            rtDither.Release();
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}