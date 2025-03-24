using UnityEngine;
using System.IO;

namespace PostEffects.Scripts {
    public class EdgeDetection : MonoBehaviour
    {
        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";

        private Material _edgeDetectionMaterial;

        void Start()
        {
            Shader bwShader = Shader.Find("Custom/EdgeDetectionShader");
            if (bwShader == null)
            {
                Debug.LogError("EdgeDetection.shader not found");
                return;
            }

            _edgeDetectionMaterial = new Material(bwShader);
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

            RenderTexture rt1 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt2 = new RenderTexture(texture.width, texture.height, 0);
            RenderTexture rt3 = new RenderTexture(texture.width, texture.height, 0);
        
            Graphics.Blit(texture, rt1, _edgeDetectionMaterial, 0);
            _edgeDetectionMaterial.SetTexture("_SobelGradientTex", rt1);
            Graphics.Blit(rt1, rt2, _edgeDetectionMaterial, 1);
            _edgeDetectionMaterial.SetTexture("_TresholdGradientTex", rt2);
            Graphics.Blit(rt2, rt3, _edgeDetectionMaterial, 1);

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt3;
            resultTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            resultTexture.Apply();

            byte[] outputBytes = resultTexture.EncodeToPNG();
            File.WriteAllBytes(outputFile, outputBytes);

            Debug.Log("Saved in: " + outputFile);
        
            RenderTexture.active = null;
            Destroy(rt1);
            Destroy(rt2);
            Destroy(rt3);
            Destroy(resultTexture);
            Destroy(texture);
        }
    }
}