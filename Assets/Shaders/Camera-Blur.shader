// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color
//
// My update: implemented some blur algo on top of it

Shader "PZ/CameraBlur" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _BlurX ("Blur X Offset", Range(0.0, 0.005)) = 0.0
    _BlurY ("Blur Y Offset", Range(0.0, 0.005)) = 0.0
    _GreyFactor ("Desaturate", Range(0.0, 1.0)) = 0.0
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct appdata_t members blurcoord)
            #pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;

                float2 blurcoords0 : TEXCOORD0;
                float2 blurcoords1 : TEXCOORD1;
                float2 blurcoords2 : TEXCOORD2;
                float2 blurcoords3 : TEXCOORD3;
                float2 blurcoords4 : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurX;
            float _BlurY;
            float _GreyFactor;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.blurcoords0 = TRANSFORM_TEX(v.texcoord, _MainTex);

                float2 offset = float2(_BlurX, _BlurY);
                o.blurcoords1 = o.blurcoords0 + offset * 1.407333;
                o.blurcoords2 = o.blurcoords0 - offset * 1.407333;
                o.blurcoords3 = o.blurcoords0 + offset * 3.294215;
                o.blurcoords4 = o.blurcoords0 - offset * 3.294215;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.blurcoords0) * 0.204164;
                col += tex2D(_MainTex, i.blurcoords1) * 0.304005;
                col += tex2D(_MainTex, i.blurcoords2) * 0.304005;
                col += tex2D(_MainTex, i.blurcoords3) * 0.093913;
                col += tex2D(_MainTex, i.blurcoords4) * 0.093913;

                float grey = col.r * 0.2126 + col.g * 0.7152 + col.b * 0.0722;
                return float4(grey, grey, grey, 1.0) * _GreyFactor + col * (1.0 - _GreyFactor);
            }
        ENDCG
    }
}

}
