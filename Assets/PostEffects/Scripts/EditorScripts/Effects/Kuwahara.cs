using UnityEngine;

namespace PostEffects.Scripts.EditorScripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/Kuwahara")]
    public class Kuwahara : BaseEffect {
        private static readonly int SectorSize = Shader.PropertyToID("_SectorSize");
        [Range(0, 20)] 
        public int sectorSize = 5;
        
        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            _material = new Material(Shader.Find("Hidden/Custom/KuwaharaShader"));

            _material.SetInt(SectorSize, sectorSize);
            Graphics.Blit(source, destination, _material);
        }
    }
}