Shader "Custom/MagneticFieldBuiltIn"
{
    Properties
    {
        _MainColor ("Field Color", Color) = (0, 1, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 10.0)) = 2.0
        _VoronoiScale ("Voronoi Scale", Float) = 5.0
        _Speed ("Movement Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            fixed4 _MainColor;
            float _FresnelPower;
            float _VoronoiScale;
            float _Speed;

            // Función simple para simular el ruido Voronoi
            float hash(float2 p) {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float voronoi(float2 x) {
                float2 n = floor(x);
                float2 f = frac(x);
                float m = 8.0;
                for(int j=-1; j<=1; j++)
                for(int i=-1; i<=1; i++) {
                    float2 g = float2(float(i),float(j));
                    float2 o = hash(n + g);
                    o = 0.5 + 0.5 * sin(_Time.y * _Speed + 6.2831 * o);
                    float2 r = g + o - f;
                    float d = dot(r, r);
                    if(d < m) m = d;
                }
                return sqrt(m);
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Efecto Fresnel (borde brillante)
                float fresnel = 1.0 - saturate(dot(normalize(i.worldNormal), normalize(i.viewDir)));
                fresnel = pow(fresnel, _FresnelPower);

                // Efecto Voronoi moviéndose
                float v = voronoi(i.uv * _VoronoiScale);
                
                // Combinar
                fixed4 col = _MainColor;
                col.a = fresnel * (1.0 - v); // La transparencia depende del fresnel y el ruido
                return col;
            }
            ENDCG
        }
    }
}