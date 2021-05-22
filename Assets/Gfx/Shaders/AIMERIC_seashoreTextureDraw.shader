Shader "Custom/AIMERIC_seashoreTextureDraw"
{
    Properties
    {
        _ScaleUV("Scale UV", float) = 0.0244140625
        _Tex_Sea("TextureSea", 2D) = "white" {}
        _Tex_Shore("TextureShore", 2D) = "white" {}
        //_Tex_River("TextureRiver", 2D) = "white" {}

        //[IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 1
    }

        SubShader
        {
         
            Tags{ "RenderType" = "Opaque" "Queue" = "Overlay"}//Overlay""}
            //Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
                Stencil
                {
                    Ref 0
                    Comp NotEqual
                }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"


                float _ScaleUV;
                sampler2D _Tex_Shore;

                fixed4 frag(v2f_img i) : SV_Target
                {
                    return tex2D(_Tex_Shore, i.uv * _ScaleUV);
                }

                ENDCG
            }

            Pass
            {
                Stencil
                {
                    Ref 0
                    Comp Equal
                }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"


                float _ScaleUV;
                sampler2D _Tex_Sea;

                fixed4 frag(v2f_img i) : SV_Target
                {
                    return tex2D(_Tex_Sea, i.uv * _ScaleUV);
                }

                ENDCG
            }
        }

}