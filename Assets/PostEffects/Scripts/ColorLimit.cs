using UnityEngine;
using System.IO;
using UnityEngine.Serialization;

namespace PostEffects.Scripts {
    public class ColorLimit : MonoBehaviour {
        private static readonly int Colors = Shader.PropertyToID("_Colors");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [FormerlySerializedAs("spread")] [Range(0, 256)]
        public int colors = 256;
        
        private Material _colorLimitMaterial;

        void Start() {
            Shader colorLimitShader = Shader.Find("Custom/ColorLimitShader");
            if (colorLimitShader == null) {
                Debug.LogError("ColorLimit.shader not found");
                return;
            }
            _colorLimitMaterial = new Material(colorLimitShader);
            
            _colorLimitMaterial.SetInt(Colors, colors);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "col-lim_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            RenderTexture rt0 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt0, _colorLimitMaterial, 0);

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