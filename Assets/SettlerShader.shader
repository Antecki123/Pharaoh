Shader "Custom/TextureColorTint" {
    Properties {
        _ColorA ("ColorA", Color) = (0,0,0,1)
        _ColorB ("ColorB", Color) = (1,1,1,1)
        _TimeFactor ("TimeFactor", float) = 1
    }

    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass {

            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define TAU 6.283185307179586

            float4 _ColorA;
            float4 _ColorB;
            float _TimeFactor;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            float InverseLerp (float a, float b, float v) {
                return (v-a)/(b-a);
            }

            v2f vert (appdata v) {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                o.uv = v.uv0;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float offset = cos(i.uv.x * TAU * 8) * 0.01;
                float t = cos((i.uv.y+ offset - _Time * 10) * TAU * 5 ) * 0.5 + 0.5;

                return t * _ColorA * abs(i.normal.x);
            }
            ENDCG
        }
    }
}
