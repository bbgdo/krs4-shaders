using UnityEngine;

namespace PostEffects.Scripts {
    public abstract class BaseEffect : ScriptableObject {
        public abstract void Apply(RenderTexture source, RenderTexture destination, EffectContext context);
    }
}
