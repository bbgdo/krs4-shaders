Shader "Custom/EdgeDetectionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Tags
        {
            "RenderType"="Opaque"
        }
        
        Pass
        {
            Name "Sobel operator"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

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
                        float3 sample = tex2D(_MainTex, shifted_uv);
                        float gray = dot(sample.rgb, float3(0.3, 0.59, 0.11));
                        gradient_x += gx[i + 1][j + 1] * gray;
                        gradient_y += gy[i + 1][j + 1] * gray;
                    }
                }
                float gradient_magnitude = sqrt(gradient_x * gradient_x + gradient_y * gradient_y);

                return float4(gradient_magnitude, gradient_magnitude, gradient_magnitude, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}