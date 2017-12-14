Shader "ARCore/PlaneGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridColor ("Grid Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _UvRotation ("UV Rotation", float) = 30
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest on
        ZWrite off

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GridColor;
            fixed _UvRotation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                fixed cosr = cos(_UvRotation);
                fixed sinr = sin(_UvRotation);
                fixed2x2 uvrotation = fixed2x2(cosr, -sinr, sinr, cosr);

                float2 uv = mul(UNITY_MATRIX_M, v.vertex).xz * _MainTex_ST.xy;
                o.uv = mul(uvrotation, uv);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return fixed4(_GridColor.rgb, col.r * i.color.a);
            }
            ENDCG
        }
    }
}