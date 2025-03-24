Shader "Custom/BW_HDRP" {
    Properties {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    float _Intensity;
    TEXTURE2D_X(_MainTex);
    
    struct MeshData
    {
        uint vertexID : SV_VertexID;
    };

    struct Interpolators
    {
        float4 vertex : SV_POSITION;
        float2 uv0   : TEXCOORD0;

    };

    Interpolators Vert(MeshData input)
    {
        Interpolators output;

        output.vertex = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.uv0 = GetFullScreenTriangleTexCoord(input.vertexID);

        return output;
    }


    float4 CustomPostProcess(Interpolators input) : SV_Target
    {
        float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.uv0).xyz;
        float gray = dot(sourceColor.rgb, float3(0.3, 0.59, 0.11));
        float3 result = lerp(sourceColor.rgb, float3(gray, gray, gray), _Intensity);
        return float4(result, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "GrayScale"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment CustomPostProcess
            ENDHLSL
        }
    }

    Fallback Off
}
