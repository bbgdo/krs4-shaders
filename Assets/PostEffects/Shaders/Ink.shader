Shader "Custom/InkShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _StipplingBias("Stippling Bias", Range(0, 1)) = 0
        _NoiseScale("Noise Scale", Range(0, 2)) = 1
        _EdgeThickness("Edge Thickness", Int) = 0
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
            float _StipplingBias;
            int _EdgeThickness;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            Name "Stippling"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                // _StipplingBias is temporary, will be moved to a standalone shader.
                float luminance = saturate(tex2D(_MainTex, interp.uv).a + _StipplingBias);
                float2 screenUV = interp.vertex.xy / _ScreenParams.xy;
                float2 noiseUV = screenUV * _NoiseScale;
                noiseUV = frac(noiseUV);
                float blue_noise = saturate(LinearRgbToLuminance(tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0))).r);

                return luminance <= blue_noise ? 1.0 : 0.0;
            }
            ENDCG
        }

        Pass {
            Name "Combine stipple with edges"
            
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
                
                [loop]
                for (int i = 0; i <= range; i++) {
                    [loop]
                    for (int j = 0; j <= range; j++) {
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
