using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
//using UnityEditor;

namespace EasyMotion2D
{
    /// <summary>
    /// GUI drawing helper class.
    /// All methods must call in OnGUI.
    /// </summary>
    public class Drawing
    {

        static Texture2D _aaLineTex = null;

        static Texture2D _lineTex = null;

        static Texture2D adLineTex
        {
            get
            {
                if (!_aaLineTex)
                {
                    _aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
                    _aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
                    _aaLineTex.SetPixel(0, 1, Color.white);
                    _aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
                    _aaLineTex.Apply();
                }
                return _aaLineTex;
            }
        }

        static Texture2D lineTex
        {
            get
            {
                if (!_lineTex)
                {
                    _lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                    _lineTex.SetPixel(0, 1, Color.white);
                    _lineTex.Apply();
                }
                return _lineTex;
            }
        }


        static void DrawLineMac(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {
            Color savedColor = GUI.color;
            Matrix4x4 savedMatrix = GUI.matrix;
    		
		    float oldWidth = width;
    		
            if (antiAlias) width *= 3;
            float angle = Vector3.Angle(pointB - pointA, Vector2.right) * (pointA.y <= pointB.y?1:-1);
            float m = (pointB - pointA).magnitude;

            if (m > 0.01f)
            {
                Vector3 dz = new Vector3(pointA.x, pointA.y, 0);
                Vector3 center = new Vector3((pointB.x - pointA.x) * 0.5f,
                                           (pointB.y - pointA.y) * 0.5f,
                                           0f);

                Vector3 tmp = Vector3.zero;

                if (antiAlias)
                    tmp = new Vector3( -oldWidth * 1.5f * Mathf.Sin(angle * Mathf.Deg2Rad), oldWidth * 1.5f * Mathf.Cos(angle * Mathf.Deg2Rad) );
                else
                    tmp = new Vector3( -oldWidth * 0.5f * Mathf.Sin(angle * Mathf.Deg2Rad), oldWidth * 0.5f * Mathf.Cos(angle * Mathf.Deg2Rad) );

                GUI.color = color;
                GUI.matrix = translationMatrix(dz) * GUI.matrix;
                GUIUtility.ScaleAroundPivot(new Vector2(m, width), new Vector2(-0.5f, 0));
                GUI.matrix = translationMatrix(-dz) * GUI.matrix;
                GUIUtility.RotateAroundPivot(angle, Vector2.zero);
                GUI.matrix = translationMatrix(dz
                                               - tmp
                                               - center
                                               ) * GUI.matrix;

                GUI.DrawTexture(new Rect(0, 0, 1, 1), antiAlias ? adLineTex :  lineTex);
            }

            GUI.matrix = savedMatrix;

            GUI.color = savedColor;
        }


        static void DrawLineWindows(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {

            Color savedColor = GUI.color;

            Matrix4x4 savedMatrix = GUI.matrix;




            if (antiAlias) width *= 3;

            float angle = Vector3.Angle(pointB - pointA, Vector2.right) * (pointA.y <= pointB.y ? 1 : -1);

            float m = (pointB - pointA).magnitude;

            Vector3 dz = new Vector3(pointA.x, pointA.y, 0);

            GUI.color = color;

            GUI.matrix = translationMatrix(dz) * GUI.matrix;

            GUIUtility.ScaleAroundPivot(new Vector2(m, width), new Vector2(-0.5f, 0));

            GUI.matrix = translationMatrix(-dz) * GUI.matrix;

            GUIUtility.RotateAroundPivot(angle, new Vector2(0, 0));

            GUI.matrix = translationMatrix(dz + new Vector3(width / 2, -m / 2) * Mathf.Sin(angle * Mathf.Deg2Rad)) * GUI.matrix;


            GUI.DrawTexture(new Rect(0, 0, 1, 1), !antiAlias ? lineTex : adLineTex);

            GUI.matrix = savedMatrix;

            GUI.color = savedColor;

        }


        /// <summary>
        /// Draw a line.
        /// </summary>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                DrawLineWindows(pointA, pointB, color, width, antiAlias);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                DrawLineMac(pointA, pointB, color, width, antiAlias);
            }
        }


        /// <summary>
        /// Draw a bezier line.
        /// </summary>
        public static void bezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments)
        {

            Vector2 lastV = cubeBezier(start, startTangent, end, endTangent, 0);

            for (int i = 1; i < segments; ++i)
            {

                Vector2 v = cubeBezier(start, startTangent, end, endTangent, i / (float)segments);



                Drawing.DrawLine(

                    lastV,

                    v,

                    color, width, antiAlias);

                lastV = v;

            }

        }



        private static Vector2 cubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {

            float rt = 1 - t;

            return rt * rt * rt * s + 3 * rt * rt * t * st + 3 * rt * t * t * et + t * t * t * e;

        }



        private static Matrix4x4 translationMatrix(Vector3 v)
        {

            return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);

        }


        /// <summary>
        /// Draw a Arc.
        /// </summary>        
        public static void DrawArc(Vector2 orig, float radius, float start, float end, float width, Color color, bool lineToOrig, float fix, float segments )
        {
            float PI2 = Mathf.PI * 2;
            float t = 0;

            t = (start +fix) * PI2;
            Vector2 s = orig + new Vector2(Mathf.Sin(t), Mathf.Cos(t)) * radius;

            t = (end + fix) * PI2;
            Vector2 e = orig + new Vector2(Mathf.Sin(t), Mathf.Cos(t)) * radius;

            if (lineToOrig)
            {
                Drawing.DrawLine(orig, s, color, width, true);
                Drawing.DrawLine(orig, e, color, width, true);
            }

            float step = 1f / segments;
            for (float i = (start + fix) + step, ie = (end + fix); i <= ie; i += step)
            {
                t = i * PI2;
                e = orig + new Vector2(Mathf.Sin(t), Mathf.Cos(t)) * radius;
                Drawing.DrawLine(s, e, color, width, true);
                s = e;
            }
        }

        /// <summary>
        /// Draw a circle.
        /// </summary>
        public static void DrawCircle(Vector2 orig, float radius, float width, Color color)
        {
            DrawArc(orig, radius, 0, 1, width, color, false, 0, 33f);
        }


        /// <summary>
        /// Draw a black/write square tiling background.
        /// </summary>
        public static void DrawBackground(Rect rc, int size, Texture2D tex, Color col1, Color col2)
        {
            Color col = GUI.color;
            bool isCol1 = false;
            int yc = 0;
            for (int y = 0; y <= rc.height; y+=size, yc++)
            {
                isCol1 = yc % 2 == 0;
                for (int x = 0; x < rc.width; x+=size)
                {
                    GUI.color = isCol1 ? col1 : col2;
                    GUI.DrawTexture(new Rect(x, y, size, size), tex, ScaleMode.StretchToFill);
                    isCol1 = !isCol1;
                }
            }

            GUI.color = col;
        }
















        //public static void DrawSprite( Vector2 parentOffset, Vector2 orig, Vector2 position, float rotation, Vector2 scale, Vector2 shear, Color color, Sprite spr, bool border, string borderText, Color borderColor)
        //{
        //    if ( spr == null && spr.image == null )
        //        return;

        //    DrawTexture(parentOffset, orig, position, rotation, scale, shear, color, spr.image, spr.pivot, spr.imageRect, border, borderText, borderColor);
        //}


        //public static void DrawTexture(Vector2 parentOffset, Vector2 orig, Vector2 position, float rotation, Vector2 scale, Vector2 shear, Color color, Texture2D tex, Vector2 pivot, Rect imageRect, bool border, string borderText, Color borderColor)
        //{
        //    if (tex == null || !tex)
        //        return;

        //    Matrix4x4 mat = GUI.matrix;
        //    Color oldColor = GUI.color;


        //    //GUI.matrix 
        //    Matrix4x4 _mat = Matrix4x4.identity;

        //    Matrix4x4 shearMat = Matrix4x4.identity;

        //    shearMat[1, 0] = shear.x;
        //    shearMat[0, 1] = shear.y;

        //    _mat = shearMat * Matrix4x4.TRS( -new Vector2( pivot.x, pivot.y) - parentOffset, Quaternion.identity, Vector3.one);            
        //    _mat = Matrix4x4.TRS(orig + position + parentOffset, Quaternion.Euler(0, 0, -rotation), new Vector3( scale.x, scale.y, 1f) ) * _mat;

        //    GUI.matrix = _mat;// *GUI.matrix;

        //    Rect borderRect = new Rect(0, 0, imageRect.width, imageRect.height);

        //    GUI.color = borderColor;
        //    if (border)
        //        GUI.BeginGroup(borderRect, "", "selectionrect");
        //    else
        //        GUI.BeginGroup(borderRect, "");

        //    GUI.color = color;
        //    GUI.DrawTexture(new Rect(-imageRect.x, -imageRect.y, tex.width, tex.height), tex);

        //    if (border)
        //    {
        //        GUI.Label(new Rect(0, 0, imageRect.width, imageRect.height), borderText, EditorStyles.whiteBoldLabel);
        //    }

        //    GUI.EndGroup();

        //    GUI.color = oldColor;
        //    GUI.matrix = mat;
        //}











        /// <summary>
        /// Draw a EasyMotion2D sprite.
        /// </summary>
        public static void DrawSprite(Vector2 parentOffset, Vector2 orig, Vector2 position, float rotation, Vector2 scale, Vector2 shear, Color color,
            Sprite spr, SpriteRenderMode renderMode, bool border, string borderText, Color borderColor, GUIStyle borderStyle)
        {
            if (spr == null && spr.image == null)
                return;

            Vector2 textScale = Vector2.one;

            if (spr.useOrigSize)
            {
                textScale = new Vector2(spr.origTextureSize.x > spr.image.width ? spr.origTextureSize.x / spr.image.width : 1f,
                    spr.origTextureSize.y > spr.image.height ? spr.origTextureSize.y / spr.image.height : 1f);

                if (spr.imageRect.x == 0f && spr.imageRect.width != spr.image.width)
                    textScale.x = spr.origTextureSize.x / spr.image.width;

                if (spr.imageRect.y == 0f && spr.imageRect.height != spr.image.height)
                    textScale.y = spr.origTextureSize.y / spr.image.height;
            }

            DrawTexture(parentOffset, orig,
                new Vector2(position.x + spr.position.x, position.y - spr.position.y),
                rotation + spr.rotation, 
                new Vector2( scale.x * spr.scale.x, scale.y * spr.scale.y),
                shear, color, spr.image, renderMode, spr.pivot, spr.imageRect, border, borderText, borderColor, borderStyle,
                textScale
                );
        }



        /// <summary>
        /// Draw a texture with transform.
        /// This function can not make Texture clipping properly.
        /// </summary>
        public static void DrawTexture(Vector2 parentOffset, Vector2 orig, Vector2 position, float rotation, Vector2 scale, Vector2 shear, Color color,
            Texture2D tex, SpriteRenderMode renderMode, Vector2 pivot, Rect imageRect, bool border, string borderText, Color borderColor, GUIStyle borderStyle, Vector2 texScale)
        {
            if (tex == null || !tex)
                return;

            if (scale.x == 0f || scale.y == 0f)
                return;

            Matrix4x4 mat = GUI.matrix;

            GUIUtility.RotateAroundPivot( 90, Vector2.zero);
            parentOffset.y = GUI.matrix.m03;

            Color oldColor = GUI.color;


            //GUI.matrix 
            Matrix4x4 _mat = Matrix4x4.identity;

            Matrix4x4 shearMat = Matrix4x4.identity;

            shearMat[1, 0] = shear.x;
            shearMat[0, 1] = shear.y;

            _mat = Matrix4x4.TRS(-new Vector2(pivot.x, pivot.y) - parentOffset
                , Quaternion.identity, Vector3.one);
            _mat = shearMat * _mat;
            _mat = Matrix4x4.TRS(orig + position + parentOffset
                , Quaternion.Euler(0, 0, -rotation)
                , new Vector3( scale.x, scale.y, 1f) ) * _mat;

            GUI.matrix = _mat;// *GUI.matrix;

            GUI.color = borderColor;
            Rect borderRect = new Rect(0, 0, imageRect.width, imageRect.height);
            if (border)
                GUI.BeginGroup( borderRect, "", "selectionrect");
            else
                GUI.BeginGroup( borderRect, "");

            GUI.color = color;
            GUI.DrawTexture(new Rect(-imageRect.x, -imageRect.y, tex.width * texScale.x, tex.height * texScale.y), tex);

            if (border)
            {
                GUI.Label(new Rect(0, 0, imageRect.width, imageRect.height), borderText, borderStyle );
            }

            GUI.EndGroup();

            GUI.color = oldColor;
            GUI.matrix = mat;
        }








        /// <summary>
        /// Draw a rectangle.
        /// </summary>
        public static void DrawRect(Rect rc, Color color, int width, bool antiAlias )
        {
            DrawLine(new Vector2(rc.xMin, rc.yMin), new Vector2(rc.xMax, rc.yMin), color, width, antiAlias);
            DrawLine(new Vector2(rc.xMax, rc.yMin), new Vector2(rc.xMax, rc.yMax), color, width, antiAlias);
            DrawLine(new Vector2(rc.xMax, rc.yMax), new Vector2(rc.xMin, rc.yMax), color, width, antiAlias);
            DrawLine(new Vector2(rc.xMin, rc.yMax), new Vector2(rc.xMin, rc.yMin), color, width, antiAlias);

        }

        /// <summary>
        /// Draw a Texture.
        /// </summary>
        public static void DrawTexture( Texture2D tex, Rect source, Rect dest, bool border, string borderText, Color color, Color bgColor, GUIStyle borderStyle )
        {
            Color tmpBgColor = GUI.backgroundColor;
            Color tmpColor = GUI.color;

            GUI.backgroundColor = bgColor;
            GUI.color = color;

            if (border)
                GUI.BeginGroup(dest, "", "selectionrect");
            else
                GUI.BeginGroup(dest, "");

            float xs = dest.width / source.width;
            float ys = dest.height / source.height;

            GUI.DrawTexture(new Rect(-source.x * xs, -source.y * ys, tex.width * xs, tex.height * ys), tex);

            if (border)
            {
                GUI.Label(new Rect(0, 0, dest.width, dest.height), borderText, borderStyle);
            }

            GUI.EndGroup();

            GUI.color = tmpColor;
            GUI.backgroundColor = tmpBgColor;
        }

        /// <summary>
        /// Fill color to rectangle in texture.
        /// </summary>
        static public void FillRectInTexture(Texture2D tex, Rect rect, Color color)
        {
            Color[] pixels = tex.GetPixels((int)rect.x, tex.height - (int)(rect.y + rect.height), (int)rect.width, (int)rect.height);

            for( int i = 0, e = pixels.Length; i < e; i++)
                pixels[i] = color;

            tex.SetPixels((int)rect.x, tex.height - (int)(rect.y + rect.height), (int)rect.width, (int)rect.height, pixels);
            tex.Apply();
        }

        /// <summary>
        /// Draw a rectangle in texture.
        /// </summary>
        static public void DrawRectInTexture(Texture2D tex, Rect rect, Color color)
        {
            for( int i = 0; i < rect.width; i++)
            {
                tex.SetPixel((int)rect.x + i, tex.height - (int)rect.yMin, color);
                tex.SetPixel((int)rect.x + i, tex.height - (int)rect.yMax, color);
            }

            for (int i = 0; i < rect.height; i++)
            {
                tex.SetPixel((int)rect.xMin, tex.height - (int)rect.y+i, color);
                tex.SetPixel((int)rect.xMax, tex.height - (int)rect.y + i, color);
            }
            tex.Apply();
        }





        static void DrawAnimationClipInGUI( Rect position, Vector2 offset, SpriteAnimationClip clip, Color color, float time)
        {
        }


        /// <summary>
        /// Internal class.
        /// </summary>
        public class SpriteTransformComparerByLayer : IComparer<SpriteTransform>
        {
            public int Compare(SpriteTransform lhs, SpriteTransform rhs)
            {
                return rhs.layer - lhs.layer;
            }

            public static SpriteTransformComparerByLayer comparer = new SpriteTransformComparerByLayer();
        }


        static List<SpriteTransform> componentTransforms = new List<SpriteTransform>();



        /// <summary>
        /// Draw a EasyMotion2D SpriteAnimationComponent hierarchy in GUI.
        /// Usually use to draw animation clip in a custom editor.
        /// </summary>
        /// <param name="position">The preview rectangle.</param>
        /// <param name="index">Not used.</param>
        /// <param name="frameRate">The framerate of clip.</param>
        /// <param name="root">The root bone you want to draw.</param>
        /// <param name="color">The color multiply to bone color.</param>
        /// <param name="canBeDraw">Not used.</param>
        /// <param name="drawGizmos">Not used.</param>
        /// <param name="drawBone">Not used.</param>
        /// <param name="offset">The offset of animation drawing to.</param>
        /// <param name="time">The current time of clip.</param>
        /// <param name="priviewZoom">The scale factor.</param>
        public static void DrawAnimation( Rect position, int index, float frameRate, SpriteAnimationComponent root,
            Color color, bool canBeDraw, bool drawGizmos, bool drawBone, Vector2 offset, float time, float priviewZoom)
        {
            GUI.BeginGroup(position);

            if (canBeDraw)
            {
                float w = Mathf.Max(position.width, 20f);
                float h = Mathf.Max(position.height, 20f);
                float cx = w * 0.5f;
                float cy = h * 0.5f;

                componentTransforms.Clear();

                Vector2 orig = new Vector2(cx, cy) + offset;
                Matrix4x4 mat = Matrix4x4.TRS(orig, Quaternion.identity, Vector3.one);
                DrawAnimationComponent(orig, frameRate, root, time, mat, 0, Vector3.one, null, 0);

                componentTransforms.Sort(SpriteTransformComparerByLayer.comparer);

                foreach (SpriteTransform trans in componentTransforms)
                {
                    if (trans.component.visible && trans.sprite != null && trans.sprite.image != null)
                    {
                        Drawing.DrawSprite(
                            new Vector2(position.x, position.y),
                            orig,
                            trans.position * priviewZoom,
                            trans.rotation,
                            trans.scale * priviewZoom,
                            trans.shear,
                            trans.color * color,
                            trans.sprite,
                            SpriteRenderMode.Additive,
                            false,
                            trans.layer.ToString(),
                            Color.white,
                            null);
                    }
                }
            }
            GUI.EndGroup();
        }


        static void DrawAnimationComponent(Vector2 orig, float frameRate,
            SpriteAnimationComponent component, float time, Matrix4x4 parentMat, float parentRotation, Vector2 parentScale, SpriteTransform parentTran, int parentLayer)
        {
            SpriteTransform transform = new SpriteTransform();

            transform.parent = parentTran;
            transform.component = component;
            transform.layer = (int)component.layer + parentLayer;

            Sprite spr = null;
            SpriteAnimationClip refClip = null;


            SpriteAnimationKeyFrame kf = component.Evaluate(time,
                ref transform.position,
                ref transform.rotation,
                ref transform.scale,
                ref transform.shear,
                ref transform.color,
                ref spr,
                ref refClip);

            transform.sprite = spr;

            componentTransforms.Add(transform);

            transform.position.y = -transform.position.y;

            Vector2 tranPosition = parentMat.MultiplyPoint3x4(transform.position);
            Matrix4x4 tmpMat = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, 0, -transform.rotation), transform.scale);



            if (refClip != null)
            {
                float l = refClip.length;
                float tick = 1f / refClip.frameRate;
                AnimationLinearCurve clipPlayingCurve = new AnimationLinearCurve(0, 0, l, l);

                clipPlayingCurve.wrapMode = refClip.wrapMode;
                float t = clipPlayingCurve.Evaluate(time - kf.frameIndex * tick);

                DrawAnimationComponent(orig, frameRate, refClip.root, t, parentMat * tmpMat,
                    parentRotation + transform.rotation,
                    new Vector2(transform.scale.x * parentScale.x, transform.scale.y * parentScale.y),
                    transform, transform.layer);
            }



            foreach (int comIdx in component.children)
            {
                SpriteAnimationComponent com = component.clip.subComponents[comIdx];
                DrawAnimationComponent(orig, frameRate, com, time, parentMat * tmpMat,
                    parentRotation + transform.rotation,
                    new Vector2(transform.scale.x * parentScale.x, transform.scale.y * parentScale.y),
                    transform, parentLayer);
            }


            transform.position = tranPosition - orig;
            transform.scale = new Vector2(transform.scale.x * parentScale.x, transform.scale.y * parentScale.y);

            transform.rotation = parentRotation + transform.rotation;
        }
    }


}
