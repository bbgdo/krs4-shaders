Shader "Custom/AsciiShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
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
            sampler2D _AsciiTex, _DownsampledTex;
            float4 _MainTex_TexelSize;
            
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
            Name "Ascii fill"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            float4 frag (Interpolators interp) : SV_Target {
                float2 screen_pos = float2(
                    interp.uv.x * _MainTex_TexelSize.z,
                    interp.uv.y * _MainTex_TexelSize.w);

                float2 tile_pos = floor(screen_pos  / 8.0);
                float2 tile_uv = fmod(screen_pos , 8.0) / 8.0;

                float2 downsampled_size = float2(
                    _MainTex_TexelSize.z,
                    _MainTex_TexelSize.w) / 8.0;
                float2 downsampled_uv = (tile_pos + 0.5) / downsampled_size;
                
                float4 block_color = tex2D(_DownsampledTex, downsampled_uv);
                float luminance = floor(LinearRgbToLuminance(block_color) * 10.0);

                float2 ascii_uv = float2((luminance + tile_uv.x) / 10.0, tile_uv.y);
                float ascii_color = tex2D(_AsciiTex, ascii_uv);
                
                return ascii_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}