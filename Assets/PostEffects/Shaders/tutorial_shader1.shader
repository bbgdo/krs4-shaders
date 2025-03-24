Shader "Unlit/tutorial_shader1" {
    Properties {
        _ColorA ("Color_A", Color) = (1, 1, 1, 1)
        _ColorB ("Color_B", Color) = (1, 1, 1, 1)
        _ColorStart ("Color_Start", Range(0, 1)) = 0
        _ColorEnd ("Color_End", Range(0, 1)) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass {
            // Pass tags
            
            /* Blending mode
               [src * A ± dst * B] */
            Blend One One // additive
//            Blend DstColor Zero // Multiply
            
            ZWrite off // to skip the write to depth buffer 
//            ZTest Always
            Cull off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            float4 _ColorA;
            float4 _ColorB;
            float _ColorStart;
            float _ColorEnd;

            struct MeshData { // per-vertex mesh data
                float4 vertex : POSITION; // vertex position
                float3 normals : NORMAL; // vertex normal
                // float3 tanget : TANGET; // tangent direction
                // float4 color : COLOR; // vertex color
                float2 uv0 : TEXCOORD0; // uv0 coordinates
                // float2 uv1 : TEXCOORD1; // uv1 coordinates
            };

            struct Interpolators {
                float4 vertex : SV_POSITION; // clip space position
                float3 normal : TEXCOORD0; 
                float2 uv : TEXCOORD1; 
            };

            Interpolators vert (MeshData v) // foreach vertex
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex); // local space to clip space (conerts using MVP matrix)
                // o.vertex = v.vertex; // for post procesing effects
                // o.normal =  v.normals;
                o.normal = UnityObjectToWorldNormal( v.normals );
                // same as above
                // o.normal = mul( (float3x3)unity_ObjectToWorld, v.normals );
                o.uv = v.uv0; // (v.uv0 + _Offset) * _Scale;
                return o;
            }

            // float (32 bits float)
            // half (16 bits float)
            // fixed4 (lower precision) => -1 to 1 range
            // float4 -> half4 -> fixed4
            // (matrices) float4x4 -> half4x4 -> fixed4x4
            // (bool bool2 bool3 bool4) 0 and 1
            // int int2 int3 int4
            float inverceLerp(float start, float end , float value) {
                    return (value - start) / (end - start);
            }
            
            float4 frag (Interpolators i) : SV_Target // foreach fragment (pixel)
            {
                // float4 myValue;
                // float2 otherValue = myValue.xy; // (or myValue.rg (or even myValue.gr)) swizzling 
                // return _Color;
                // return float4(i.uv, 0,  1.0);
                // return float4(UnityObjectToWorldNormal(i.normal), 1.0);
                /**
                 * usually same operation can be done in vertex shader and fragment shader
                 * also usually you will have more pixels(fragments) than vertices
                 * thats why it is better to do calculations in vertex shader
                 * but sometimes you will have less pixels(fragments) than vertices (for exampple object is far away)
                 * so you will want to do calculations in fragment shader
                 */
                // blend between two colors based on uv.x
                
                // float t = saturate(inverceLerp(_ColorStart, _ColorEnd, i.uv.x));
                // float t = abs(frac(i.uv.x * 5) * 2 - 1);
                float offsetX = (cos(i.uv.y * TAU * 0.5 + 0.6)) + i.uv.y*2;
                float t = cos((i.uv.x + offsetX) * TAU * 5) * 0.5 + 0.5;
                return t*(1-i.uv.y);
                // float4 outputColor = lerp(_ColorA, _ColorB, t);
                //
                // return outputColor;
            }
            
            

            ENDCG
        }
    }
}
