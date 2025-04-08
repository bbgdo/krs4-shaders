using UnityEngine;

namespace PostEffects.Scripts.Effects {
    [CreateAssetMenu(menuName = "PostEffects/Effects/BW")]
    public class BW : BaseEffect
    {

        private Material _material;

        public override void Apply(RenderTexture source, RenderTexture destination, EffectContext context) {
            if (_material is null) {
                Shader bwShader = Shader.Find("Custom/BWShader");
                _material = new Material(bwShader);
            }

            Graphics.Blit(source, destination, _material);
        }
    }
}