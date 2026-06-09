Shader "Custom/GridProximity"
{
    Properties
    {
        _Color        ("Color", Color)      = (1,1,1,1)
        _CursorPos    ("Cursor World Pos",  Vector) = (0,0,0,0)
        _Radius       ("Reveal Radius",     Float)  = 5.0
        _EdgeSoftness ("Edge Softness",     Float)  = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float3 _CursorPos;
            float  _Radius;
            float  _EdgeSoftness;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 diff = i.worldPos.xz - _CursorPos.xz;
                float  dist = length(diff);

                float inner = _Radius - _EdgeSoftness;
                float alpha = 1.0 - smoothstep(inner, _Radius, dist);

                clip(alpha - 0.001);

                half4 col = _Color;
                col.a *= alpha;
                return col;
            }
            ENDHLSL
        }
    }
}