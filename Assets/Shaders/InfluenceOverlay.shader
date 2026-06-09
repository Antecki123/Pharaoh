Shader "Custom/InfluenceOverlay"
{
    Properties
    {
        _InfluenceTex  ("Influence Texture", 2D) = "black" {}
        _GradientTex   ("Gradient LUT",      2D) = "white" {}
        _Offset        ("Depth Offset",      Float)        = -1.0
        _EdgeSoftness  ("Edge Softness",     Range(0.0, 0.5)) = 0.08
        _ClipThreshold ("Clip Threshold",    Range(0.0, 0.2)) = 0.3
    }
    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
        }
        Pass
        {
            Name "InfluenceOverlay"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            Offset [_Offset], [_Offset]
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_InfluenceTex);
            SAMPLER(sampler_InfluenceTex);
            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _InfluenceTex_ST;
                float4 _InfluenceTex_TexelSize;
                float4 _GradientTex_ST;
                float  _Offset;
                float  _EdgeSoftness;
                float  _ClipThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _InfluenceTex);
                return OUT;
            }

            float SampleInfluenceSmooth(float2 uv)
            {
                float2 texSize    = _InfluenceTex_TexelSize.zw;
                float2 texelSize  = _InfluenceTex_TexelSize.xy;

                float2 pixel = uv * texSize;

                float2 tileIndex = floor(pixel - 0.5) + 0.5;
                float2 t         = pixel - tileIndex;
                float2 smooth_t = t * t * (3.0 - 2.0 * t);

                float2 uv00 = (tileIndex + float2(0, 0)) * texelSize;
                float2 uv10 = (tileIndex + float2(1, 0)) * texelSize;
                float2 uv01 = (tileIndex + float2(0, 1)) * texelSize;
                float2 uv11 = (tileIndex + float2(1, 1)) * texelSize;

                float v00 = SAMPLE_TEXTURE2D_LOD(_InfluenceTex, sampler_InfluenceTex, uv00, 0).r;
                float v10 = SAMPLE_TEXTURE2D_LOD(_InfluenceTex, sampler_InfluenceTex, uv10, 0).r;
                float v01 = SAMPLE_TEXTURE2D_LOD(_InfluenceTex, sampler_InfluenceTex, uv01, 0).r;
                float v11 = SAMPLE_TEXTURE2D_LOD(_InfluenceTex, sampler_InfluenceTex, uv11, 0).r;

                return lerp(
                    lerp(v00, v10, smooth_t.x),
                    lerp(v01, v11, smooth_t.x),
                    smooth_t.y
                );
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float influence = SampleInfluenceSmooth(IN.uv);

                float alpha = smoothstep(_ClipThreshold, _ClipThreshold + _EdgeSoftness, influence);

                half4 col = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(influence, 0.5));
                col.a *= alpha;
                return col;
            }
            ENDHLSL
        }
    }
}