Shader "Hidden/Custom/SharpenShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 1.0
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
                float3x3 kernel = float3x3(
                    0, -1, 0,
                    -1, 5, -1,
                    0, -1, 0
                );
                float3 color;
                
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        float2 offset = float2(i, j) * _MainTex_TexelSize.xy;
                        float4 tex_color = tex2D(_MainTex, interp.uv + offset);
                        color += tex_color.rgb * kernel[i + 1][j + 1];
                    }
                }
                color = saturate(color);
                color = lerp(tex2D(_MainTex, interp.uv).rgb, color, _Intensity);
                
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}