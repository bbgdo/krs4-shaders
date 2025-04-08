using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PostEffects.Scripts
{
    public static class EffectProcessor
    {
        public static void RunChain(string inputPath, string outputPath, string fileName, List<BaseEffect> effects, string prefix = "out")
        {
            string inputFile = Path.Combine(inputPath, fileName);
            if (!File.Exists(inputFile))
            {
                Debug.LogError("Input file not found: " + inputFile);
                return;
            }

            byte[] fileData = File.ReadAllBytes(inputFile);
            Texture2D inputTexture = new Texture2D(2, 2);
            inputTexture.LoadImage(fileData);

            var context = new EffectContext
            {
                inputPath = inputPath,
                outputPath = outputPath,
                fileName = fileName,
                outputPrefix = prefix,
                width = inputTexture.width,
                height = inputTexture.height
            };

            RenderTexture rtA = new RenderTexture(context.width, context.height, 0);
            RenderTexture rtB = new RenderTexture(context.width, context.height, 0);
            Graphics.Blit(inputTexture, rtA);

            foreach (var effect in effects)
            {
                if (effect == null) continue;
                effect.Apply(rtA, rtB, context);
                (rtA, rtB) = (rtB, rtA);
            }

            SaveRenderTexture(rtA, context, "final");

            RenderTexture.active = null;
            rtA.Release();
            rtB.Release();
            Object.DestroyImmediate(inputTexture);
        }

        private static void SaveRenderTexture(RenderTexture rt, EffectContext context, string label)
        {
            Texture2D outputTexture = new Texture2D(context.width, context.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            outputTexture.ReadPixels(new Rect(0, 0, context.width, context.height), 0, 0);
            outputTexture.Apply();

            string fileName = $"{context.outputPrefix}_{label}_{context.fileName}";
            string outputFile = Path.Combine(context.outputPath, fileName);
            File.WriteAllBytes(outputFile, outputTexture.EncodeToPNG());
            Debug.Log($"Saved: {outputFile}");

            Object.DestroyImmediate(outputTexture);
        }
    }
}
