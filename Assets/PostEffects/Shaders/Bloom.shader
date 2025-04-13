Shader "Hidden/Custom/BloomShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0, 1)) = 0.9
        _Knee ("Knee", Range(0, 1)) = 0.1
        _Radius ("Radius", Range(0, 10)) = 1.0
        _Intensity ("Intensity", Range(0, 1)) = 1.0
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
        
            sampler2D _MainTex, _BloomTex;
            float4 _MainTex_TexelSize;
            float _Threshold, _Knee, _Radius, _Intensity;
            
            Interpolators vert (MeshData v) {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG
        
        Pass {
            Name "Brightness mask"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float4 tex = tex2D(_MainTex, interp.uv);

                float brightness = max(tex.r, max(tex.g, tex.b));
                float contribution = smoothstep(_Threshold - _Knee, _Threshold + _Knee, brightness);
            
                return tex * contribution;
            }
            ENDCG
        }
        
        Pass {
            Name "Sampling"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                return tex2D(_MainTex, interp.uv);
            }
            ENDCG
        }

        Pass {
            Name "Kawase blur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float4 tex = tex2D(_MainTex, interp.uv);
                float2 offset = _MainTex_TexelSize.xy * _Radius;
                
                tex.rgb += tex2D(_MainTex, interp.uv + offset).rgb;
                tex.rgb += tex2D(_MainTex, interp.uv - offset).rgb;
                tex.rgb += tex2D(_MainTex, interp.uv + float2(offset.x, -offset.y)).rgb;
                tex.rgb += tex2D(_MainTex, interp.uv + float2(-offset.x, offset.y)).rgb;
                tex.rgb /= 5.0;
                
                return tex;
            }
            ENDCG
        }

        Pass {
            Name "Additive Blend"
            Blend One One
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            float4 frag (Interpolators interp) : SV_Target {
                float3 bloom = tex2D(_BloomTex, interp.uv).rgb;
                return float4(bloom, 1.0);
            }
            ENDCG
        }

        Pass {
            Name "Combine"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (Interpolators interp) : SV_Target {
                float4 tex = tex2D(_MainTex, interp.uv);
                float3 bloom = tex2D(_BloomTex, interp.uv).rgb;

                float3 res = lerp(tex.rgb, tex.rgb + bloom, _Intensity);
                
                return float4(res, tex.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}