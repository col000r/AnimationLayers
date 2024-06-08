// Copyright (c) 2020 Markus Hofer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEditor;
using UnityEngine;

public class IMGUIToolkit : MonoBehaviour {

    private static ToolkitStylesCollection StylesCollection;
    public static ToolkitStylesCollection Styles {
        get {
            if( !StylesCollection ) {
                var guids = AssetDatabase.FindAssets( "ToolkitStyles t:ToolkitStylesCollection" );
                foreach( var guid in guids ) {
                    //Debug.Log( guid );
                    string path = AssetDatabase.GUIDToAssetPath( guid );
                    //Debug.Log( path );
                    StylesCollection = AssetDatabase.LoadAssetAtPath<ToolkitStylesCollection>( path );
                    break;
                }
            }

            return StylesCollection;
        }
    }

    private static Texture2D s_WhiteTex;
    public static Texture2D WhiteTexture {
        get {
            if( s_WhiteTex == null ) {
                s_WhiteTex = new Texture2D( 1, 1, TextureFormat.RGB24, false );
                s_WhiteTex.SetPixel( 0, 0, Color.white );
                s_WhiteTex.Apply();
            }
            return s_WhiteTex;
        }
    }
    

    public enum Corners {
        TopLeft,
        Top, 
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        All,
        None
    } 
    
    public static void DrawRect( Rect rect, Corners roundedCorners = Corners.All ) {
        GUIStyle style = Styles.RoundedRect.Style;
        if( roundedCorners == Corners.TopLeft ) style = Styles.RoundedRectTL.Style;
        else if( roundedCorners == Corners.Top ) style = Styles.RoundedRectTop.Style;
        else if( roundedCorners == Corners.TopRight ) style = Styles.RoundedRectTR.Style;
        else if( roundedCorners == Corners.Right ) style = Styles.RoundedRectRight.Style;
        else if( roundedCorners == Corners.BottomRight ) style = Styles.RoundedRectBR.Style;
        else if( roundedCorners == Corners.Bottom ) style = Styles.RoundedRectBottom.Style;
        else if( roundedCorners == Corners.BottomLeft ) style = Styles.RoundedRectBL.Style;
        else if( roundedCorners == Corners.Left ) style = Styles.RoundedRectLeft.Style;
        else if( roundedCorners == Corners.None ) style = Styles.Rect.Style;
        GUI.Box( rect, GUIContent.none, style );
    }
    
}
