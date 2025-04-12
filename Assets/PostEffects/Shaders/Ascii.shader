Shader "Custom/AsciiShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
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
        
            Texture2D _MainTex;
            float4 _MainTex_TexelSize;
            SamplerState point_clamp_sampler;
            sampler2D _AsciiTex, _AsciiEdgeTex, _DownsampledTex;
            
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
            Name "Sobel operator"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float2 texel_size = _MainTex_TexelSize.xy;
                float3x3 gx = float3x3(
                    -1, 0, 1,
                    -2, 0, 2,
                    -1, 0, 1);
                float3x3 gy = float3x3(
                    -1, -2, -1,
                    0, 0, 0,
                    1, 2, 1);

                float gradient_x = 0;
                float gradient_y = 0;
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        float2 shifted_uv = interp.uv + float2(i, j) * texel_size;
                        float luminance = _MainTex.Sample(point_clamp_sampler, shifted_uv).r;
                        gradient_x += gx[i + 1][j + 1] * luminance;
                        gradient_y += gy[i + 1][j + 1] * luminance;
                    }
                }
                float angle = degrees(atan2(gradient_y, gradient_x));

                if (angle < 0) angle += 180;

                /* east and west - */
                if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180)) {
                    return float4(0.4, 0, 0, 1);
                }
                /* north-east and south-west / */
                if (angle >= 22.5 && angle < 67.5) {
                    return float4(0.8, 0, 0, 1);
                }
                /* north and south | */
                if (angle >= 67.5 && angle < 112.5) {
                    return float4(0.2, 0, 0, 1);
                }
                /* north-west and south-east \ */
                if (angle >= 112.5 && angle < 157.5) {
                    return float4(0.6, 0, 0, 1);
                }

                return float4(0, 0, 0, 1);
            }
            ENDCG
        }

        Pass {
            Name "Ascii"
            
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

                float edge_shift = _MainTex.Sample(point_clamp_sampler, downsampled_uv).r;
                if(edge_shift > 0) {
                    float2 ascii_edge_uv = float2((edge_shift * 5 + tile_uv.x) / 5.0, tile_uv.y);
                    float ascii_edge_color = tex2D(_AsciiEdgeTex, ascii_edge_uv);
                    
                    return ascii_edge_color;
                }

                float luminance_shift = floor(
                    LinearRgbToLuminance(
                        tex2D(_DownsampledTex, downsampled_uv).rgb
                    ) * 10.0);
                
                float2 ascii_uv = float2((luminance_shift + tile_uv.x) / 10.0, tile_uv.y);
                float ascii_color = tex2D(_AsciiTex, ascii_uv);
                
                return ascii_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}