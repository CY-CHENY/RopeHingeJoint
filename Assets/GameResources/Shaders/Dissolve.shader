Shader "Custom/Dissolve"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _DissolvePercent ("Dissolve Percent", Range(0,1)) = 0.5
        _LocalMinY ("Local Min Y", Float) = 0
        _LocalMaxY ("Local Max Y", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 localY : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _DissolvePercent;
            float _LocalMaxY;
            float _LocalMinY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  // 保留原有UV变换
                o.localY = v.vertex.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               float normalizedY = (i.localY - _LocalMinY) / (_LocalMaxY - _LocalMinY);
               if(normalizedY > _DissolvePercent)
               {
                discard;
               }
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
