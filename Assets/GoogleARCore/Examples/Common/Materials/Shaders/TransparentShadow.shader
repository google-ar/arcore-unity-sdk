Shader "ARCore/TransparentShadow"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }

        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200
        ZWrite off

        Pass
        {
            Name "SHADOW_ONLY"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_fwdbase
                #pragma multi_compile_fog

                #include "UnityCG.cginc"
                #include "AutoLight.cginc"

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 worldPos : TEXCOORD0;
                    float2 uv : TEXCOORD1;
                    LIGHTING_COORDS(2,3)
                    UNITY_FOG_COORDS(4)
                };

                v2f vert (appdata_full v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.uv = v.texcoord1.xy;
                    TRANSFER_VERTEX_TO_FRAGMENT(o);
                    UNITY_TRANSFER_FOG(o,o.pos);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                    fixed alpha = 1 - atten;
                    if (alpha == 0.0)
                    {
                        // pixel is not in shadow.
                        return 0.0;
                    }

                    const fixed3 _ShadowColor = fixed3(0, 0, 0);
                    const fixed _ShadowIntensity = 0.24;
                    const fixed _FadeRadius = 0;
                    fixed ratio = distance(i.uv, fixed2(0.5, 0.5)) * 2;
                    if (ratio >= 1.0)
                    {
                        // pixel is out of shadow range.
                        return 0.0;
                    }
                    else if (ratio > _FadeRadius)
                    {
                        // pixel is in fade out range.
                        alpha *= ((1 - ratio) / (1 - _FadeRadius));
                    }

                    fixed4 col;
                    col.rgb = _ShadowColor.rgb;
                    col.a = _ShadowIntensity * alpha;
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
                }
            ENDCG
        }
    }
}
