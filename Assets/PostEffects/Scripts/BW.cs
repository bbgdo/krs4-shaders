using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class BW : MonoBehaviour
    {
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";

        private Material bwMaterial;

        void Start()
        {
            Shader bwShader = Shader.Find("Custom/BWShader");
            if (bwShader == null)
            {
                Debug.LogError("BW.shader not found");
                return;
            }

            bwMaterial = new Material(bwShader);
            ProcessImage();
        }

        void ProcessImage()
        {
            string inputFile = Path.Combine(inputPath, fileName);
            string outputFile = Path.Combine(outputPath, "bw_" + fileName);

            if (!File.Exists(inputFile))
            {
                Debug.LogError("File not found: " + inputFile);
                return;
            }
        
            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            RenderTexture rt = new RenderTexture(texture.width, texture.height, 0);
            Graphics.Blit(texture, rt, bwMaterial);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}