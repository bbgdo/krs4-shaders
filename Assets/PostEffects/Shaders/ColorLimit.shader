Shader "Custom/ColorLimitShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Colors ("Color limit", Int) = 128
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
        
            Texture2D _MainTex;
            SamplerState point_clamp_sampler;
            int _Colors;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            Name "Color limitting"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            float4 frag (Interpolators interp) : SV_Target {
                float3 color = _MainTex.Sample(point_clamp_sampler, interp.uv).rgb;
                color = floor(color * (_Colors - 1) + 0.5) / _Colors;
                
                return float4(color, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}