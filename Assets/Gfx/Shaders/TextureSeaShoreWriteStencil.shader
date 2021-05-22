Shader "Custom/TextureSeaShoreWriteStencil"
{
    Properties
    {
    }

        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Geometry+100" "IgnoreProjector" = "True"}
            //ZWrite Off
            //Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                Stencil
                {
                    Ref 10
                    Comp NotEqual
                    Pass IncrSat
                }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                fixed4 frag(v2f_img i) : SV_Target
                {
                    return fixed4(0,1,0,1); //debug: opaque green ; set Queue to Overlay if want to visualise
                }

                ENDCG
            }

            Pass //edges had a circle area where stencil buffer was at 2 after previous pass ; set it to 1
            {
                Stencil
                {
                    Ref 2
                    Comp Equal
                    Pass DecrSat
                }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                fixed4 frag(v2f_img i) : SV_Target
                {
                    return fixed4(1,0,0,1); //debug: opaque red
                }

                ENDCG
            }

            /*Pass //debug test
            {
                Stencil
                {
                    Ref 1
                    Comp Equal
                }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                fixed4 frag(v2f_img i) : SV_Target
                {
                    return fixed4(0,0,1,1); //debug: (1,0,0,0) blue
                }

                ENDCG
            }*/
        }
}
