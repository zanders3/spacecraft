using UnityEngine;

public class GUIHelper
{
    private static Texture2D texture;
    private static Color lastTexColor;

    public static void DrawQuad(Rect position, Color color) 
    {
        if (texture == null)
            texture = new Texture2D(1, 1);
        if (lastTexColor != color)
        {
            lastTexColor = color;
            texture.SetPixel(0, 0, color);
            texture.Apply();
        }

        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }
}

