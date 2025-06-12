Shader "Custom/AnaglyphRedCyan_URP"
{
    Properties
    {
        _LeftTex ("Left Eye Texture", 2D) = "white" {}
        _RightTex ("Right Eye Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_LeftTex);
            SAMPLER(sampler_LeftTex);
            TEXTURE2D(_RightTex);
            SAMPLER(sampler_RightTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 left = SAMPLE_TEXTURE2D(_LeftTex, sampler_LeftTex, input.uv);
                half4 right = SAMPLE_TEXTURE2D(_RightTex, sampler_RightTex, input.uv);
                
                // Red-cyan anaglyph
                half4 col;
                col.r = left.r * 0.299 + left.g * 0.587 + left.b * 0.114;
                col.g = right.g;
                col.b = right.b;
                col.a = 1.0;
                
                return col;
            }
            ENDHLSL
        }
    }
}