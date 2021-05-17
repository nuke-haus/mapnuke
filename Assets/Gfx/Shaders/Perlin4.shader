Shader "Custom/Perlin4"
{
    Properties
    {
        _GridSize ("GridSize", float) = 1.0
        _X ("Seed X", float) = 1.0
        _Y ("Seed Y", float) = 1.0
		_ScaleX ("Scale X", float) = 1.0
        _ScaleY ("Scale Y", float) = 1.0
		_Threshold1 ("Color 1-2 Threshold", float) = 0.25
		_Threshold2 ("Color 2-3 Threshold", float) = 0.50
		_Threshold3 ("Color 3-4 Threshold", float) = 0.75
		_Color1 ("Color1", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (1,1,1,1)
		_Color3 ("Color3", Color) = (1,1,1,1)
		_Color4 ("Color4", Color) = (1,1,1,1)
        [IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 10
    }
 
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref[_StencilRef]
            Comp Always
            Pass Replace
        }
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            float4 permute(float4 x)
            {
                return fmod(34.0 * pow(x, 2) + x, 289.0);
            }
 
            float2 fade(float2 t) {
                return 6.0 * pow(t, 5.0) - 15.0 * pow(t, 4.0) + 10.0 * pow(t, 3.0);
            }
 
            float4 taylorInvSqrt(float4 r) {
                return 1.79284291400159 - 0.85373472095314 * r;
            }
 
            #define DIV_289 0.00346020761245674740484429065744f
 
            float mod289(float x) 
			{
                return x - floor(x * DIV_289) * 289.0;
            }
 
            float PerlinNoise2D(float2 P)
            {
                  float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
                  float4 Pf = frac (P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
 
                  float4 ix = Pi.xzxz;
                  float4 iy = Pi.yyww;
                  float4 fx = Pf.xzxz;
                  float4 fy = Pf.yyww;
 
                  float4 i = permute(permute(ix) + iy);
 
                  float4 gx = frac(i / 41.0) * 2.0 - 1.0 ;
                  float4 gy = abs(gx) - 0.5 ;
                  float4 tx = floor(gx + 0.5);
                  gx = gx - tx;
 
                  float2 g00 = float2(gx.x,gy.x);
                  float2 g10 = float2(gx.y,gy.y);
                  float2 g01 = float2(gx.z,gy.z);
                  float2 g11 = float2(gx.w,gy.w);
 
                  float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
                  g00 *= norm.x;
                  g01 *= norm.y;
                  g10 *= norm.z;
                  g11 *= norm.w;
 
                  float n00 = dot(g00, float2(fx.x, fy.x));
                  float n10 = dot(g10, float2(fx.y, fy.y));
                  float n01 = dot(g01, float2(fx.z, fy.z));
                  float n11 = dot(g11, float2(fx.w, fy.w));
 
                  float2 fade_xy = fade(Pf.xy);
                  float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
                  float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
                  return 2.3 * n_xy;
            }
 
            float _GridSize;
            float _X;
            float _Y;
			float _ScaleX;
            float _ScaleY;
			float _Threshold1;
			float _Threshold2;
			float _Threshold3;

			uniform float4 _Color1;
			uniform float4 _Color2;
			uniform float4 _Color3;
			uniform float4 _Color4;
 
            fixed4 frag (v2f_img i) : SV_Target
            {
                i.uv *= _GridSize;
                i.uv += float2(_X, _Y);
				i.uv /= float2(_ScaleX, _ScaleY);
                float ns = PerlinNoise2D(i.uv) / 2 + 0.5f;

				if (ns < _Threshold1)
				{
					return _Color1;
				}
				if (ns < _Threshold2)
				{
					return _Color2;
				}
				if (ns < _Threshold3)
				{
					return _Color3;
				}
				return _Color4;
                //return float4(ns, ns, ns, 1.0f);
            }
            ENDCG
        }
    }
}