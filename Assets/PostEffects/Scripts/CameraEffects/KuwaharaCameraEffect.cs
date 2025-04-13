using UnityEngine;

namespace PostEffects.Scripts.CameraEffects {
    [AddComponentMenu("Custom PostFX/Kuwahara")]
    public class KuwaharaCameraEffect : MonoBehaviour {
        private static readonly int SectorSize = Shader.PropertyToID("_SectorSize");
    
        [Range(0, 20)]
        public int sectorSize = 5;

        private Material _material;

        void OnEnable() {
            _material ??= new Material(Shader.Find("Custom/KuwaharaShader"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_material == null) {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetInt(SectorSize, sectorSize);
            Graphics.Blit(source, destination, _material);
        }

        void OnDisable() {
            if (_material != null) {
                DestroyImmediate(_material);
            }
        }
    }
}