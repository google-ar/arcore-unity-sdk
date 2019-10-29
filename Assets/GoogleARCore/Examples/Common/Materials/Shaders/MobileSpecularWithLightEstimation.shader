Shader "ARCore/SpecularWithLightEstimation" 
{
    Properties 
    {
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 250

        CGPROGRAM
        #pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview interpolateview finalcolor:lightEstimation

        inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
        {
            fixed diff = max (0, dot (s.Normal, lightDir));
            fixed nh = max (0, dot (s.Normal, halfDir));
            fixed spec = pow (nh, s.Specular*128) * s.Gloss;

            fixed4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
            UNITY_OPAQUE_ALPHA(c.a);
            return c;
        }

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Shininess;
        fixed3 _GlobalColorCorrection;

        struct Input
        {
            float2 uv_MainTex;
        };

        void lightEstimation(Input IN, SurfaceOutput o, inout fixed4 color)
        {
            color.rgb *= _GlobalColorCorrection;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb;
            o.Gloss = tex.a;
            o.Alpha = tex.a;
            o.Specular = _Shininess;
            o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_MainTex));
        }
        ENDCG
    }

    FallBack "Mobile/VertexLit"
}
