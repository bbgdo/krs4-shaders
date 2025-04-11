Shader "Custom/PixelSortingShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _LowThreshold ("Lower Threshold", Range(0, 2)) = 1.0
        _HighThreshold ("Higher Threshold", Range(-1, 1)) = 0.0
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
        
            sampler2D _MainTex, _SortedTex;
            float _LowThreshold, _HighThreshold;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            Name "Mask"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float3 color = tex2D(_MainTex, interp.uv).rgb;
                float luminance = LinearRgbToLuminance(color);

                if (luminance > _LowThreshold && luminance < _HighThreshold) {
                    return float4(color, 1);
                }

                return float4(0, 0, 0, 1);
            }
            ENDCG
        }

        Pass {
            Name "Combine"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float3 sorted_color = tex2D(_SortedTex, interp.uv).rgb;
                if(LinearRgbToLuminance(sorted_color) > 0) {
                    return float4(sorted_color, 1);
                    // return float4(1,0,0,1);
                }               
                
                return tex2D(_MainTex, interp.uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}