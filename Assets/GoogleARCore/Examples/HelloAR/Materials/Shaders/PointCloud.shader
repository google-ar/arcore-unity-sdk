// Don't remove the following line. It is used to bypass Unity
// upgrader change. This is necessary to make sure the shader
// continues to compile on Unity 5.2
// UNITY_SHADER_NO_UPGRADE
Shader "ARCore/PointCloud" {
Properties{
        _PointSize("Point Size", Float) = 5.0
        _Color ("PointCloud Color", Color) = (0.121, 0.737, 0.823, 1.0)
}
  SubShader {
     Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
           float4 vertex : POSITION;
        };

        struct v2f
        {
           float4 vertex : SV_POSITION;
           float size : PSIZE;
        };

        float _PointSize;
        fixed4 _Color;

        v2f vert (appdata v)
        {
           v2f o;
           o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
           o.size = _PointSize;

           return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
           return _Color;
        }
        ENDCG
     }
  }
}
