Shader "Custom/RetroShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Spread ("Spread", Range(0, 1)) = 0.5
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
            float4 _MainTex_TexelSize;
            int _Colors;
            float _Spread;
            
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
                return _MainTex.Sample(point_clamp_sampler, interp.uv);
            }
            ENDCG
        }

        Pass {
            Name "Dithering"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            float4 frag (Interpolators interp) : SV_Target {
                float2 screen_pos = interp.uv * _MainTex_TexelSize.zw;
                float4x4 bayer4x4 = float4x4(
                    0, 8, 2, 10,
                    12, 4, 14, 6,
                    3, 11, 1, 9,
                    15, 7, 13, 5
                );
                float2 bayer4x4_pos = floor(fmod(screen_pos, 4));
                float bayer4x4_val = bayer4x4[bayer4x4_pos.x][bayer4x4_pos.y] / 16 - 0.5;

                float3 color = _MainTex.Sample(point_clamp_sampler, interp.uv).rgb;
                color.r = floor((color.r + _Spread * bayer4x4_val) * (_Colors - 1.0) + 0.5) / (_Colors - 1.0);
                color.g = floor((color.g + _Spread * bayer4x4_val) * (_Colors - 1.0) + 0.5) / (_Colors - 1.0);
                color.b = floor((color.b + _Spread * bayer4x4_val) * (_Colors - 1.0) + 0.5) / (_Colors - 1.0);
                
                return float4(color, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}