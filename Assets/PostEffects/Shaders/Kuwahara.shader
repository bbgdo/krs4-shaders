Shader "Custom/KuwaharaShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _SectorSize ("Sector Size", Int) = 2
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
            int _SectorSize;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 sector_color(int index, float2 uv, int sectorSize) {
                float2 sector;
                float3 color_sum = 0;
                float mean = 0;
                float diviation = 0;
                int n = sectorSize * sectorSize;
                switch (index) {
                    case 0: sector = float2(-1, -1); break;
                    case 1: sector = float2(1, -1); break;
                    case 2: sector = float2(-1, 1); break;
                    case 3: sector = float2(1, 1); break;
                }

                [loop]
                for (int i = 0; i < sectorSize; i++) {
                    for (int j = 0; j < sectorSize; j++) {
                        float2 sampleUV = uv + float2(i, j) * sector * _MainTex_TexelSize;
                        
                        float3 sample = tex2D(_MainTex, saturate(sampleUV)).rgb;
                        float luminance = LinearRgbToLuminance(sample);

                        color_sum += sample;
                        mean += luminance;
                        diviation += luminance * luminance;
                    }
                }
                mean /= n;
                diviation = diviation / n - mean * mean;
                
                return float4(color_sum / n, diviation);
            }
        ENDCG

        Pass {
            Name "Kuwahara operator"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            float4 frag (Interpolators interp) : SV_Target {
                float2 uv = interp.uv;

                float min_div = 99999.0;
                float3 result_color;

                [unroll]
                for (int i = 0; i < 4; i++) {
                    float4 sector = sector_color(i, uv, _SectorSize);
                    if (sector.a < min_div) {
                        min_div = sector.a;
                        result_color = sector.rgb;
                    }
                }
                
                
                return float4(result_color, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}