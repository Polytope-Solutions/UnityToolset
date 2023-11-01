using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
    public static partial class ObjectHelpers {
        public static Color[] SampleTexture(this Texture2D texture, int sampleResolutionX, int sampleResolutionY) {
            int sampleCount = (sampleResolutionX + 1) * (sampleResolutionY + 1);
            Color[] sampleColors = new Color[sampleCount];
            Color[] colors = texture.GetPixels();
            int textrueWidth = texture.width;
            int textrueHeight = texture.height;
            Color color1 = Color.black;
            Color color2 = Color.black;
            Color color = Color.black;
            for (int y = 0; y <= sampleResolutionY; y++) {
                for (int x = 0; x <= sampleResolutionX; x++) {
                    int i = x + y * (sampleResolutionX + 1);
                    Vector2 uv = new Vector2(x / (float)sampleResolutionX, y / (float)sampleResolutionY);
                    int x1 = Mathf.FloorToInt(uv.x * (textrueWidth - 1));
                    int x2 = Mathf.CeilToInt(uv.x * (textrueWidth - 1));
                    int y1 = Mathf.FloorToInt(uv.y * (textrueHeight - 1));
                    int y2 = Mathf.CeilToInt(uv.y * (textrueHeight - 1));
                    int colorI1 = Mathf.Clamp(x1 + y1 * textrueWidth, 0, colors.Length - 1);
                    int colorI2 = Mathf.Clamp(x2 + y1 * textrueWidth, 0, colors.Length - 1);
                    int colorI3 = Mathf.Clamp(x1 + y2 * textrueWidth, 0, colors.Length - 1);
                    int colorI4 = Mathf.Clamp(x2 + y2 * textrueWidth, 0, colors.Length - 1);
                    if (x1 != x2) {
                        float lerpX = Mathf.Clamp01((uv.x * (textrueWidth - 1) - x1) / (x2 - x1));
                        color1 = Color.Lerp(colors[colorI1], colors[colorI2], lerpX);
                        color2 = Color.Lerp(colors[colorI3], colors[colorI4], lerpX);
                    } else {
                        color1 = colors[colorI1];
                        color2 = colors[colorI3];
                    }
                    if (y1 != y2) {
                        float lerpY = Mathf.Clamp01((uv.y * (textrueHeight - 1) - y1) / (y2 - y1));
                        color = Color.Lerp(color1, color2, lerpY);
                    } else {
                        color = color1;
                    }
                    sampleColors[i] = color;
                }
            }
            return sampleColors;
        }
    }
}