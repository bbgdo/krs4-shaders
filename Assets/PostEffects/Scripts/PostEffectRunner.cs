using System.Collections.Generic;
using UnityEngine;

namespace PostEffects.Scripts
{
    public class PostEffectRunner : MonoBehaviour
    {
        public List<BaseEffect> effectChain;

        public string inputPath = "Assets/PostEffects/Test/Input/";
        public string outputPath = "Assets/PostEffects/Test/Output/";
        public string fileName = "nier.png";
        public string outputFilePrefix = "";
        
        // TODO: finish this for real-time processing
        
        [ContextMenu("Run Chain")]
        public void Run()
        {
            EffectProcessor.RunChain(inputPath, outputPath, fileName, effectChain, "postfx");
        }

    }
}
