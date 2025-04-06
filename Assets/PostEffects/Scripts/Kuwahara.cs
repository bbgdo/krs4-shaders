using UnityEngine;
using System.IO;
using UnityEngine.Serialization;

namespace PostEffects.Scripts {
    public class Kuwahara : MonoBehaviour {
        private static readonly int SectorSize = Shader.PropertyToID("_SectorSize");
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        [FormerlySerializedAs("windowSize")] [Range(0, 20)] 
        public int sectorSize = 1;
        
        private Material _kuwaharaMaterial;

        void Start() {
            Shader kuwaharaShader = Shader.Find("Custom/KuwaharaShader");
            if (kuwaharaShader == null) {
                Debug.LogError("Kuwahara.shader not found");
                return;
            }
            _kuwaharaMaterial = new Material(kuwaharaShader);
            
            _kuwaharaMaterial.SetInt(SectorSize, sectorSize);
            
            ProcessImage();
        }

        private void ProcessImage() {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "kuwahara_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            RenderTexture rt0 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt0, _kuwaharaMaterial, 0);

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