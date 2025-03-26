Shader "Custom/SmartPixelShader" {
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
        
            sampler2D _MainTex;
            sampler2D _Downsampled;
            sampler2D _EdgeMap;
            float4 _MainTex_TexelSize;
            SamplerState point_clamp_sampler;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG
        
        Pass {
            Name "Sobel operator"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag(Interpolators interp) : SV_Target {
                float2 texel_size = _MainTex_TexelSize.xy;

                // Sobel kernels
                float3x3 gx = float3x3(
                    -1, 0, 1,
                    -2, 0, 2,
                    -1, 0, 1);
                float3x3 gy = float3x3(
                    -1, -2, -1,
                     0,  0,  0,
                     1,  2,  1);
    
                float gradient_x = 0;
                float gradient_y = 0;
    
                // Collect neighborhood samples and compute gradients
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        float2 shifted_uv = interp.uv + float2(i, j) * texel_size;
                        float luminance = LinearRgbToLuminance(tex2D(_MainTex, shifted_uv).r);
                        gradient_x += gx[i + 1][j + 1] * luminance;
                        gradient_y += gy[i + 1][j + 1] * luminance;
                    }
                }
    
                float gradient_magnitude = sqrt(gradient_x * gradient_x + gradient_y * gradient_y);
                float direction = atan2(gradient_y, gradient_x);
                float angle = degrees(direction);
                if (angle < 0) angle += 180;
    
                // Non-maximum suppression
                float2 dirA, dirB;
    
                if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180)) {
                    dirA = float2(-1, 0);
                    dirB = float2(1, 0);
                } else if (angle >= 22.5 && angle < 67.5) {
                    dirA = float2(-1, -1);
                    dirB = float2(1, 1);
                } else if (angle >= 67.5 && angle < 112.5) {
                    dirA = float2(0, -1);
                    dirB = float2(0, 1);
                } else {
                    dirA = float2(-1, 1);
                    dirB = float2(1, -1);
                }
    
                float2 shifted_uv_A = interp.uv + dirA * texel_size;
                float2 shifted_uv_B = interp.uv + dirB * texel_size;
    
                float sampleA = LinearRgbToLuminance(tex2D(_MainTex, shifted_uv_A).r);
                float sampleB = LinearRgbToLuminance(tex2D(_MainTex, shifted_uv_B).r);
                float magA = sampleA;
                float magB = sampleB;
    
                float suppressed = 0;
                if (gradient_magnitude > magA && gradient_magnitude > magB) {
                    suppressed = gradient_magnitude;
                }
    
                // Double threshold
                float edge_strength;
                if (suppressed > 0.3) {
                    edge_strength = 1.0;
                } else if (suppressed > 0.1) {
                    edge_strength = 0.167; // weak edge
                } else {
                    edge_strength = 0.0;
                }
    
                // Hysteresis
                if (edge_strength == 0.167) {
                    for (int i = -1; i <= 1; i++) {
                        for (int j = -1; j <= 1; j++) {
                            float2 neighbor_uv = interp.uv + float2(i, j) * texel_size;
                            neighbor_uv = clamp(neighbor_uv, 0.001, 0.999);
                            float neighbor_gradient_x = 0;
                            float neighbor_gradient_y = 0;
    
                            for (int m = -1; m <= 1; m++) {
                                for (int n = -1; n <= 1; n++) {
                                    float2 uv_offset = neighbor_uv + float2(m, n) * texel_size;
                                    float lum = LinearRgbToLuminance(tex2D(_MainTex, uv_offset).r);
                                    neighbor_gradient_x += gx[m + 1][n + 1] * lum;
                                    neighbor_gradient_y += gy[m + 1][n + 1] * lum;
                                }
                            }
    
                            float neighbor_mag = sqrt(neighbor_gradient_x * neighbor_gradient_x + neighbor_gradient_y * neighbor_gradient_y);
                            if (neighbor_mag > 0.3) {
                                edge_strength = 1.0;
                                break;
                            }
                        }
                    }
                }
    
                return float4(edge_strength, edge_strength, edge_strength, 1);
            }
            ENDCG
        }
        
        Pass {
            Name "Point sampling"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                return tex2D(_MainTex, interp.uv);
            }
            ENDCG
        }

        Pass {
            Name "Define edges"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float edge = tex2D(_EdgeMap, interp.uv).r;
                float4 original_color = tex2D(_MainTex, interp.uv);
                float4 pixel_color = tex2D(_Downsampled, interp.uv);
                
                return edge > 0 ? float4(original_color.rgb*0.5, 1) : pixel_color;
            }
            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
