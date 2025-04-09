Shader "Custom/SaturationShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 2)) = 1.0
    }
    SubShader {
        Cull Off
        ZWrite Off
        ZTest Always
        Tags {
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
            float4 _MainTex_TexelSize;
            float _Intensity;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            Name "Point sampling"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float4 tex = tex2D(_MainTex, interp.uv);
                float luminance = LinearRgbToLuminance(tex);
                
                return float4(saturate(lerp(
                            float3(luminance, luminance, luminance),
                            tex.rgb,
                            _Intensity))
                        ,tex.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}