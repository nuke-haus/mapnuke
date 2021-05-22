Shader "Custom/AIMERIC_seashoreMask"
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
                    //WriteMask 1
                    //Pass Invert // 10 is 1010 in bit, writemask takes the last bit (0) and invert it: in short sets the stencil buffer to 1
                }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                fixed4 frag(v2f_img i) : SV_Target
                {
                    return fixed4(0,1,0,1); //debug: (0,1,0,1) opaque green
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
                    return fixed4(1,0,0,1); //debug: (1,0,0,0) opaque red
                }

                ENDCG
            }

            /*Pass //test
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
                    return fixed4(0,0,1,1); //debug: (1,0,0,0) opaque blue
                }

                ENDCG
            }*/
        }
}
