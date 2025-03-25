Shader "Custom/InkShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _HighThreshold("High Threshold", Range(0, 1)) = 0.4
        _LowThreshold("Low Threshold", Range(0, 1)) = 0.1
        _ReduceNoise("Reduce Noise", Range(0, 1)) = 0
        _NoiseScale("Noise Scale", Range(0, 2)) = 1
        _EdgeThickness("Edge Thickness", Range(0, 5)) = 0
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
            sampler2D _LuminanceTex;
            sampler2D _NoiseTex;
            float4 _NoiseTex_TexelSize;
            float _NoiseScale;
            sampler2D _StippleTex;
            float _HighThreshold;
            float _LowThreshold;
            float _ReduceNoise;
            float _EdgeThickness;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG
        
        
        Pass {
            Name "Gaussian filter"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                if (_ReduceNoise == 0) {
                    float luminance =  LinearRgbToLuminance(tex2D(_MainTex, interp.uv));
                    
                    return luminance;
                }

                float2 texel_size = _MainTex_TexelSize.xy;
                int index = 0;
                float blur = 0;
                float gauss[25] = {
                    2, 4, 5, 4, 2,
                    4, 9, 12, 9, 4,
                    5, 12, 15, 12, 5,
                    4, 9, 12, 9, 4,
                    2, 4, 5, 4, 2
                };
            
                for (int i = -2; i <= 2; i++) {
                    for (int j = -2; j <= 2; j++) {
                        float2 shifted_uv = interp.uv + float2(i, j) * texel_size;
                        float3 sample = tex2D(_MainTex, shifted_uv).rgb;
                        float luminance = LinearRgbToLuminance(sample);
                        blur += luminance * gauss[index];
                        index++;
                    }
                }
                float result = blur / 159.0;

                return result;
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
                        float sample = tex2D(_MainTex, shifted_uv).a;
                        gradient_x += gx[i + 1][j + 1] * sample;
                        gradient_y += gy[i + 1][j + 1] * sample;
                    }
                }
                float gradient_magnitude = sqrt(gradient_x * gradient_x + gradient_y * gradient_y);
                float direction = atan2(gradient_y, gradient_x);
                
                return float4(direction, 0, 0, gradient_magnitude);
            }
            ENDCG
        }
        
        Pass {
            Name "Gradient magnitude thresholding"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                float2 texel_size = _MainTex_TexelSize.xy;
                float magnitude = tex2D(_MainTex, interp.uv).a;
                float thresholded_gradient = 0;
                float2 dirA;
                float2 dirB;
                float angle = degrees(tex2D(_MainTex, interp.uv).r);

                if (angle < 0) angle += 180;

                /* east and west - */
                if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180)) {
                    dirA = float2(-1, 0);
                    dirB = float2(1, 0);
                }
                /* north-east and south-west / */
                else if (angle >= 22.5 && angle < 67.5) {
                    dirA = float2(-1, -1);
                    dirB = float2(1, 1);
                }
                /* north and south | */
                else if (angle >= 67.5 && angle < 112.5) {
                    dirA = float2(0, -1);
                    dirB = float2(0, 1);
                }
                /* north-west and south-east \ */ 
                else if (angle >= 112.5 && angle < 157.5) {
                    dirA = float2(-1, 1);
                    dirB = float2(1, -1);
                }

                float2 shifted_uv_A = interp.uv + dirA * texel_size;
                float2 shifted_uv_B = interp.uv + dirB * texel_size;

                float fragA = tex2D(_MainTex, shifted_uv_A).a;
                float fragB = tex2D(_MainTex, shifted_uv_B).a;
                        
                if (magnitude > fragA && magnitude > fragB) {
                    thresholded_gradient += magnitude;
                }

                return float4(angle, 0, 0, thresholded_gradient);
            }
            ENDCG
        }

        Pass {
            Name "Double threshold"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                float magnitude = tex2D(_MainTex, interp.uv).a;
                float threshold_res = 0;
                
                if (magnitude > _HighThreshold) {
                    threshold_res+=1;
                } else if (magnitude > _LowThreshold) {
                    threshold_res+=0.167;
                }
                threshold_res = saturate(threshold_res);
                
                return threshold_res;
            }
            ENDCG
        }
        
        Pass {
            Name "Hysteresis"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                float2 texel_size = _MainTex_TexelSize.xy;
                float magnitude = tex2D(_MainTex, interp.uv).a;
                if (magnitude == 1) {
                    return magnitude;
                }
                if (magnitude == 0) {
                    return 0;
                }
                
                for(int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        float2 shifted_uv = interp.uv + float2(i, j) * texel_size;
                        shifted_uv = clamp(shifted_uv, 0.001, 0.999);
                        float sample = tex2D(_MainTex, shifted_uv).a;
                        if (sample == 1) {
                            return 1.0;
                        }
                    }
                }
                
                return 0.0;
            }
            ENDCG
        }

        Pass {
            Name "Stippling"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                float luminance = tex2D(_MainTex, interp.uv).a;
                float2 screenUV = interp.vertex.xy / _ScreenParams.xy;
                float2 noiseUV = screenUV * _NoiseScale;
                noiseUV = frac(noiseUV);
                float blue_noise = saturate(LinearRgbToLuminance(tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0))).r);

                return luminance <= blue_noise ? 1.0 : 0.0;
            }
            ENDCG
        }

        Pass {
            Name "Combine"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                float2 texel_size = _MainTex_TexelSize.xy;
                float stippling = tex2D(_StippleTex, interp.uv).a;
                float luminance = tex2D(_LuminanceTex, interp.uv).r;
                luminance = saturate((luminance - 0.6) * 2.5);
                float adaptiveRange = lerp(_EdgeThickness, 0, luminance);
                int range = (int)floor(adaptiveRange);
                float edge = 0.0;
            
                const int MAX_RANGE = 5; 
            
                [unroll]
                for (int i = 0; i <= MAX_RANGE; i++) {
                    [unroll]
                    for (int j = 0; j <= MAX_RANGE; j++) {
                        if (abs(i) > range || abs(j) > range) continue;
            
                        float2 shifted_uv = interp.uv + float2(i, j) * texel_size;
                        shifted_uv = clamp(shifted_uv, 0.001, 0.999);
                        float sample = tex2D(_MainTex, shifted_uv).a;
                        edge = max(edge, sample);
                    }
                }
                float result = 1.0 - saturate(edge + stippling);

                return float4(result, result, result, 1);
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
