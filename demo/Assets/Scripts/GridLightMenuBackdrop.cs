using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Home-screen patterned backdrop: flat lamp + grid-cell icons tiled with spacing,
/// tilted, and scrolled via UV repeat so the loop is seamless.
/// </summary>
public sealed class GridLightMenuBackdrop : MonoBehaviour
{
    private const int TileWidthPx = 168;
    private const int TileHeightPx = 118;
    private const float IconSizePx = 52f;
    private const float PairGapPx = 22f;
    private const float TiltDegrees = -16f;
    private const float ScrollTilesPerSecond = 0.18f;
    private const float PatternSize = 3200f;
    private const float UvSpan = 18f;

    private RawImage _rawImage;
    private float _scrollOffset;
    private static Texture2D _tileTexture;

    public static GridLightMenuBackdrop Create(Transform parent)
    {
        var go = new GameObject("MenuBackdrop", typeof(RectTransform), typeof(RectMask2D));
        go.transform.SetParent(parent, false);
        go.transform.SetAsFirstSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var backdrop = go.AddComponent<GridLightMenuBackdrop>();
        backdrop.BuildPattern();
        return backdrop;
    }

    private void BuildPattern()
    {
        EnsureTileTexture();

        var patternGo = new GameObject("Pattern", typeof(RectTransform), typeof(RawImage));
        patternGo.transform.SetParent(transform, false);

        var rt = patternGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(PatternSize, PatternSize);
        rt.localEulerAngles = new Vector3(0f, 0f, TiltDegrees);

        _rawImage = patternGo.GetComponent<RawImage>();
        _rawImage.texture = _tileTexture;
        _rawImage.color = new Color32(176, 132, 88, 78);
        _rawImage.raycastTarget = false;
        _rawImage.uvRect = new Rect(0f, 0f, UvSpan, UvSpan);
    }

    private void Update()
    {
        if (_rawImage == null || !isActiveAndEnabled) return;

        // UV Repeat never teleports geometry — only the sample window slides.
        _scrollOffset = Mathf.Repeat(_scrollOffset + ScrollTilesPerSecond * Time.unscaledDeltaTime, 1f);
        _rawImage.uvRect = new Rect(_scrollOffset, 0f, UvSpan, UvSpan);
    }

    private static void EnsureTileTexture()
    {
        if (_tileTexture != null) return;

        _tileTexture = new Texture2D(TileWidthPx, TileHeightPx, TextureFormat.RGBA32, false)
        {
            name = "MenuPatternTile",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat,
            hideFlags = HideFlags.HideAndDontSave
        };

        Clear(_tileTexture);

        var bulb = CreateBulbSpriteTexture();
        var cell = CreateCellSpriteTexture();
        var icon = Mathf.RoundToInt(IconSizePx);
        var gap = Mathf.RoundToInt(PairGapPx);
        var y = (TileHeightPx - icon) / 2;

        Blit(bulb, _tileTexture, 8, y, icon, icon);
        Blit(cell, _tileTexture, 8 + icon + gap, y, icon, icon);

        Object.Destroy(bulb);
        Object.Destroy(cell);
        _tileTexture.Apply(false, true);
    }

    private static void Blit(Texture2D src, Texture2D dst, int dstX, int dstY, int w, int h)
    {
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
        {
            var sx = Mathf.Clamp(Mathf.RoundToInt((x + 0.5f) * src.width / (float)w - 0.5f), 0, src.width - 1);
            var sy = Mathf.Clamp(Mathf.RoundToInt((y + 0.5f) * src.height / (float)h - 0.5f), 0, src.height - 1);
            var c = src.GetPixel(sx, sy);
            if (c.a < 0.02f) continue;
            var dx = dstX + x;
            var dy = dstY + y;
            if (dx < 0 || dy < 0 || dx >= dst.width || dy >= dst.height) continue;
            var under = dst.GetPixel(dx, dy);
            dst.SetPixel(dx, dy, Color.Lerp(under, c, c.a));
        }
    }

    private static Texture2D CreateBulbSpriteTexture()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        Clear(tex);
        FillCircle(tex, 32, 36, 16, Color.white);
        FillRect(tex, 26, 14, 12, 10, Color.white);
        FillRect(tex, 24, 8, 16, 5, Color.white);
        DrawCircleOutline(tex, 32, 36, 20, 2, new Color(1f, 1f, 1f, 0.85f));
        FillRect(tex, 31, 56, 2, 5, Color.white);
        FillRect(tex, 48, 44, 5, 2, Color.white);
        FillRect(tex, 11, 44, 5, 2, Color.white);
        tex.Apply(false, false);
        return tex;
    }

    private static Texture2D CreateCellSpriteTexture()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        Clear(tex);
        FillRoundedRect(tex, 8, 8, 48, 48, 8, Color.white);
        FillRoundedRect(tex, 16, 16, 32, 32, 5, Color.clear);
        DrawRoundedRectOutline(tex, 16, 16, 32, 32, 5, 2, Color.white);
        tex.Apply(false, false);
        return tex;
    }

    private static void Clear(Texture2D tex)
    {
        var clear = new Color(0, 0, 0, 0);
        var pixels = new Color[tex.width * tex.height];
        for (var i = 0; i < pixels.Length; i++) pixels[i] = clear;
        tex.SetPixels(pixels);
    }

    private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color color)
    {
        for (var py = y; py < y + h; py++)
        for (var px = x; px < x + w; px++)
            if (InBounds(tex, px, py)) tex.SetPixel(px, py, color);
    }

    private static void FillCircle(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        var r2 = radius * radius;
        for (var y = cy - radius; y <= cy + radius; y++)
        for (var x = cx - radius; x <= cx + radius; x++)
        {
            var dx = x - cx;
            var dy = y - cy;
            if (dx * dx + dy * dy <= r2 && InBounds(tex, x, y))
                tex.SetPixel(x, y, color);
        }
    }

    private static void DrawCircleOutline(Texture2D tex, int cx, int cy, int radius, int thickness, Color color)
    {
        var outer2 = radius * radius;
        var inner = Mathf.Max(0, radius - thickness);
        var inner2 = inner * inner;
        for (var y = cy - radius; y <= cy + radius; y++)
        for (var x = cx - radius; x <= cx + radius; x++)
        {
            var d = (x - cx) * (x - cx) + (y - cy) * (y - cy);
            if (d <= outer2 && d >= inner2 && InBounds(tex, x, y))
                tex.SetPixel(x, y, color);
        }
    }

    private static void FillRoundedRect(Texture2D tex, int x, int y, int w, int h, int radius, Color color)
    {
        for (var py = y; py < y + h; py++)
        for (var px = x; px < x + w; px++)
        {
            if (!InBounds(tex, px, py)) continue;
            if (InsideRoundedRect(px + 0.5f, py + 0.5f, x, y, w, h, radius))
                tex.SetPixel(px, py, color);
        }
    }

    private static void DrawRoundedRectOutline(Texture2D tex, int x, int y, int w, int h, int radius, int thickness, Color color)
    {
        for (var py = y; py < y + h; py++)
        for (var px = x; px < x + w; px++)
        {
            if (!InBounds(tex, px, py)) continue;
            var inside = InsideRoundedRect(px + 0.5f, py + 0.5f, x, y, w, h, radius);
            var inner = InsideRoundedRect(
                px + 0.5f, py + 0.5f,
                x + thickness, y + thickness,
                w - thickness * 2, h - thickness * 2,
                Mathf.Max(0, radius - thickness));
            if (inside && !inner)
                tex.SetPixel(px, py, color);
        }
    }

    private static bool InsideRoundedRect(float px, float py, int x, int y, int w, int h, int radius)
    {
        if (w <= 0 || h <= 0) return false;
        var minX = x + radius;
        var maxX = x + w - radius;
        var minY = y + radius;
        var maxY = y + h - radius;
        if (px >= minX && px <= maxX && py >= y && py <= y + h) return true;
        if (py >= minY && py <= maxY && px >= x && px <= x + w) return true;

        var cx = px < minX ? minX : maxX;
        var cy = py < minY ? minY : maxY;
        var dx = px - cx;
        var dy = py - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static bool InBounds(Texture2D tex, int x, int y) =>
        x >= 0 && y >= 0 && x < tex.width && y < tex.height;
}
