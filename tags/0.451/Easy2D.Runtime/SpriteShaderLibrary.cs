using UnityEngine;
using System.Collections;


namespace Easy2D
{

    public class SpriteShaderLibrary
    {
        static private bool[] transArray = new bool[]{
       false,//AlphaKey,
       false,// ColorKey,
       true,// Additive,
       true,// SoftAdditive,
       true,// AlphaBlend,
    };

        static public bool IsShaderTransparent(SpriteRenderMode rm)
        {
            return transArray[(int)rm];
        }

        public static string AlphaKey =
            "Shader \"SpriteEngine/Alpha Key\" { \n" +
            "	Properties { \n" +
            "		_MainTex (\"Texture\", 2D) = \"white\" {} \n" +
            "		_AlphaKey (\"AlphaKey\", Range (0.0,1.0)) = 0.4 \n" +
            "	} \n" +

            "	Category { \n" +
            "		Tags { \"Queue\"=\"Geometry\" \"IgnoreProjector\"=\"True\" \"RenderType\"=\"Geometry\" } \n" +

            "		Blend One Zero           \n" +

            "		AlphaTest GEqual [_AlphaKey] \n" +
            "		ColorMask RGB \n" +
            "		Cull Off Lighting Off ZWrite On Fog { Color (0,0,0,0) } \n" +
            "		ZTest Less \n" +

            "		BindChannels { \n" +
            "			Bind \"Color\", color \n" +
            "			Bind \"Vertex\", vertex \n" +
            "			Bind \"TexCoord\", texcoord \n" +
            "		} \n" +

            "		// ---- Dual texture cards \n" +
            "		SubShader { \n" +
            "			Pass { \n" +
            "				SetTexture [_MainTex] { \n" +
            "					combine texture * primary \n" +
            "				} \n" +
            "			} \n" +
            "		} \n" +
            "	} \n" +
            "}";


        public static string AlphaBlend =
            "Shader \"SpriteEngine/Alpha Blend\" { \n" +
            "	Properties { \n" +
            "		_MainTex (\"Texture\", 2D) = \"white\" {} \n" +
            "	} \n" +

            "	Category { \n" +
            "		Tags { \"Queue\"=\"Transparent\" \"IgnoreProjector\"=\"True\" \"RenderType\"=\"Transparent\" } \n" +

            "		Blend SrcAlpha OneMinusSrcAlpha \n" +

            "		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) } \n" +
            //"		ZTest Less \n" +
            //"       ColorMask RGB\n"+

            "		BindChannels { \n" +
            "			Bind \"Color\", color \n" +
            "			Bind \"Vertex\", vertex \n" +
            "			Bind \"TexCoord\", texcoord \n" +
            "		} \n" +

            "		// ---- Dual texture cards \n" +
            "		SubShader { \n" +
            "			Pass { \n" +
            "				SetTexture [_MainTex] { \n" +
            "					combine texture * primary \n" +
            "				} \n" +
            "			} \n" +
            "		} \n" +
            "	}\n" +
            "}";


        public static string Additive =
            "Shader \"SpriteEngine/Additive\" { \n" +
            "	Properties { \n" +
            "		_MainTex (\"Texture\", 2D) = \"white\" {} \n" +
            "	} \n" +

            "	Category { \n" +
            "		Tags { \"Queue\"=\"Transparent\" \"IgnoreProjector\"=\"True\" \"RenderType\"=\"Transparent\" } \n" +

            "       Blend SrcAlpha One \n" +

            "		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) } \n" +
            //"		ZTest Less \n" +

            "		BindChannels { \n" +
            "			Bind \"Color\", color \n" +
            "			Bind \"Vertex\", vertex \n" +
            "			Bind \"TexCoord\", texcoord \n" +
            "		} \n" +

            "		// ---- Dual texture cards \n" +
            "		SubShader { \n" +
            "			Pass { \n" +
            "				SetTexture [_MainTex] { \n" +
            "					combine texture * primary \n" +
            "				} \n" +
            "			} \n" +
            "		} \n" +
            "	} \n" +
            "}";


        public static string SoftAdditive =
            "Shader \"SpriteEngine/SoftAdditive\" { \n" +
            "	Properties { \n" +
            "		_MainTex (\"Texture\", 2D) = \"white\" {} \n" +
            "	} \n" +

            "	Category { \n" +
            "		Tags { \"Queue\"=\"Transparent\" \"IgnoreProjector\"=\"True\" \"RenderType\"=\"Transparent\" } \n" +

            "		Blend SrcAlpha OneMinusDstAlpha           \n" +

            "		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) } \n" +
            //"		ZTest Less \n" +

            "		BindChannels { \n" +
            "			Bind \"Color\", color \n" +
            "			Bind \"Vertex\", vertex \n" +
            "			Bind \"TexCoord\", texcoord \n" +
            "		} \n" +

            "		// ---- Dual texture cards \n" +
            "		SubShader { \n" +
            "			Pass { \n" +
            "				SetTexture [_MainTex] { \n" +
            "					combine texture * primary \n" +
            "				} \n" +
            "			} \n" +
            "		} \n" +
            "	} \n" +
            "}";
    }

}