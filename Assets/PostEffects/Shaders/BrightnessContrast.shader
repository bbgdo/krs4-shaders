Shader "Custom/BrightnessContrastShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Contrast ("Contrast", Range(0, 2)) = 1.0
        _Brightness ("Brightness", Range(-1, 1)) = 0.0
    }
    SubShader {
        Cull Off
        ZWrite Off
        ZTest Always
        Tags
        {
            "RenderType"="Opaque"
        }
        
        CGINCLUDE
            #include "UnityCG.cginc"
            struct MeshData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
        
            struct Interpolators {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
        
            sampler2D _MainTex;
            float _Contrast, _Brightness;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float4 tex = tex2D(_MainTex, interp.uv);
                // contrast
                tex.rgb = (tex.rgb - 0.5) * _Contrast + 0.5;
                // brightness
                tex.rgb = saturate(tex.rgb + _Brightness);

                return tex;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}