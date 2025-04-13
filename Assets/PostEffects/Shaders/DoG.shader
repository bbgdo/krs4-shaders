// Difference of Gaussians
Shader "Hidden/Custom/DoGShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _GaussianKernelSize("Kernel Size", Int) = 5
        _Sigma("Sigma", Range(0, 5)) = 1
        _SigmaK("Sigma scale", Range(0, 5)) = 1.5
        _Threshold("Threshold", Range(0, 1)) = 0
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
            int _GaussianKernelSize;
            float _Sigma1, _SigmaK, _Threshold;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            #define PI 3.14159265358979323846f
            
            float gaussian(float sigma, float mu) {
                return exp(-mu * mu / (2 * sigma * sigma)) / (sqrt(2 * PI) * sigma);
            }
            
        ENDCG
        

        Pass {
            Name "horizontal blur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float2 result = float2(0.0, 0.0);
                float sum1 = 0.0;
                float sum2 = 0.0;
                
                for (int i = -_GaussianKernelSize; i <= _GaussianKernelSize; i++ ) {
                    float color = LinearRgbToLuminance(tex2D(_MainTex, interp.uv + float2(i, 0) * _MainTex_TexelSize.xy));
                    sum1 += color * gaussian(_Sigma1, i);
                    sum2 += color * gaussian(_Sigma1 * _SigmaK, i);
                    result.r += sum1 / _GaussianKernelSize;
                    result.g += sum2 / _GaussianKernelSize;
                }
                                    
                return float4(result, 0.0, 1.0);
            }
            ENDCG
        }

        Pass {
            Name "vertical blur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float2 result = float2(0.0, 0.0);
                float sum1 = 0.0;
                float sum2 = 0.0;
                
                for (int i = -_GaussianKernelSize; i <= _GaussianKernelSize; i++ ) {
                    float color = LinearRgbToLuminance(tex2D(_MainTex, interp.uv + float2(0, i) * _MainTex_TexelSize.xy));
                    sum1 += color * gaussian(_Sigma1, i);
                    sum2 += color * gaussian(_Sigma1 * _SigmaK, i);
                    result.r += sum1 / _GaussianKernelSize;
                    result.g += sum2 / _GaussianKernelSize;
                }
                                    
                return float4(result, 0.0, 1.0);
            }
            ENDCG
        }

        Pass {
            Name "Difference of Gaussians"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float2 gaussians = float2(tex2D(_MainTex, interp.uv).rg);

                float diff = gaussians.r - gaussians.g;

                // later implement levels
                if (_Threshold > 0) {
                    diff = diff > _Threshold ?  1 : 0;
                }

                return diff;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}