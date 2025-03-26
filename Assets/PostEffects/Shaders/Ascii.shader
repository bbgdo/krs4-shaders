Shader "Custom/AsciiShader" {
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
        
            sampler2D _MainTex, _AsciiTex, _DownsampledTex;
            SamplerState sampler_AsciiTex;
            
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
                return tex2D(_MainTex, interp.uv);
            }
            ENDCG
        }

        Pass {
            Name "Ascii fill"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (Interpolators interp) : SV_Target {
                float2 aligned_screen_size = floor(_ScreenParams.xy / 8.0) * 8.0;
                float2 screen_pos = interp.uv * aligned_screen_size;
                
                float2 downsampled_pos = floor(screen_pos / 8.0);
                float2 downsampled_tex_size = aligned_screen_size / 8.0;
                float2 downsampled_uv = (downsampled_pos + 0.5) / downsampled_tex_size;
                
                float4 block_color = tex2D(_DownsampledTex, downsampled_uv);
                float luminance = floor(LinearRgbToLuminance(block_color) * 9.0);
                
                float2 local_uv = fmod(screen_pos, 8.0) / 8.0;
                float2 char_uv = float2(
                (luminance + local_uv.x) / 10.0,
                local_uv.y
                );
                float ascii_color = tex2D(_AsciiTex, char_uv);
                
                return ascii_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}