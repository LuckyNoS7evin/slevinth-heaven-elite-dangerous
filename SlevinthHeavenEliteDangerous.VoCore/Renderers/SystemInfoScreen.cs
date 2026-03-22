using SkiaSharp;
using System;

namespace SlevinthHeavenEliteDangerous.VoCore.Renderers;

internal static class SystemInfoScreen
{
    private static readonly SKColor Background    = new(0x08, 0x08, 0x12);
    private static readonly SKColor Accent        = new(0xFF, 0x6A, 0x00);
    private static readonly SKColor TextPrimary   = SKColors.White;
    private static readonly SKColor TextSecondary = new(0x99, 0x99, 0xAA);
    private static readonly SKColor TextDim       = new(0x55, 0x55, 0x66);

    /// <summary>
    /// Renders the current system info screen and returns an RGB565 byte array at the given dimensions.
    /// Designed for portrait orientation (width &lt; height).
    /// </summary>
    public static byte[] Render(string systemName, double distanceFromSol, double lastJumpDist, System.Collections.Generic.IEnumerable<(string Name, string Reason, double Distance)> valuableBodies, int width, int height)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgb565);
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(Background);

        DrawAccentBars(canvas, width, height);

        // Page header — match ExoBio header sizing
        float cx = width / 2f;
        using var headerPaint = new SKPaint { Color = Accent, TextSize = Math.Clamp(height * 0.05f, 24f, 36f), IsAntialias = true, FakeBoldText = true };
        float headerY = height * 0.10f;
        canvas.DrawText("CURRENT SYSTEM", cx - TextWidth(headerPaint, "CURRENT SYSTEM") / 2f, headerY, headerPaint);

        // Compact system info below header
        bool hasSystem = !string.IsNullOrWhiteSpace(systemName);
        string displayName = hasSystem ? systemName : "Awaiting jump...";
        SKColor nameColor = hasSystem ? TextPrimary : TextDim;
        using var namePaint = new SKPaint { Color = nameColor, TextSize = Math.Clamp(height * 0.045f, 14f, 32f), IsAntialias = true, FakeBoldText = true };
        // Add extra gap between header and system name so layout matches ExoBio spacing
        float nameY = headerY + headerPaint.TextSize * 1.4f;
        canvas.DrawText(displayName, cx - TextWidth(namePaint, displayName) / 2f, nameY, namePaint);

        using var metaPaint = new SKPaint { Color = TextSecondary, TextSize = Math.Clamp(height * 0.035f, 12f, 24f), IsAntialias = true };
        string distText = distanceFromSol > 0 ? $"{distanceFromSol:N1} ly from Sol" : "Distance unknown";
        float metaY = nameY + namePaint.TextSize * 0.9f;
        canvas.DrawText(distText, cx - TextWidth(metaPaint, distText) / 2f, metaY, metaPaint);

        // Precompute jump Y so contentTop can account for it
        float jumpY = metaY + metaPaint.TextSize * 0.9f;
        if (lastJumpDist > 0)
        {
            string jumpText = $"Last jump: {lastJumpDist:F1} ly";
            using var jumpPaint = new SKPaint { Color = TextDim, TextSize = Math.Clamp(height * 0.03f, 10f, 20f), IsAntialias = true };
            canvas.DrawText(jumpText, cx - TextWidth(jumpPaint, jumpText) / 2f, jumpY, jumpPaint);
        }

        // Valuable bodies area — occupies the remainder of the screen and is the focus
        // Start below the last meta text (distance or last jump)
        float contentTop = jumpY + metaPaint.TextSize * 1.2f;
        float contentBottom = height - (height * 0.06f);
        float contentHeight = contentBottom - contentTop;

        if (valuableBodies != null)
        {
            using var vbNamePaint = new SKPaint { Color = TextPrimary, TextSize = Math.Clamp(contentHeight * 0.08f, 14f, 28f), IsAntialias = true, FakeBoldText = true };
            using var vbReasonPaint = new SKPaint { Color = TextSecondary, TextSize = Math.Clamp(contentHeight * 0.08f, 14f, 24f), IsAntialias = true };

            // Calculate box/padding sizes and how many items fit
            float padding = 8f;
            float boxSpacing = 8f;
            float boxHeight = vbNamePaint.TextSize + vbReasonPaint.TextSize + padding * 2f;
            int maxItems = Math.Max(1, (int)(contentHeight / (boxHeight + boxSpacing)));
            int idx = 0;
            float y = contentTop + padding;

            using var boxBgPaint = new SKPaint { Color = new SKColor(0x10, 0x10, 0x18), IsAntialias = true };
            using var boxBorderPaint = new SKPaint { Color = new SKColor(0x44, 0x44, 0x55), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };

            float boxLeft = 12f;
            float boxWidth = width - boxLeft * 2f;

            foreach (var vb in valuableBodies)
            {
                if (idx >= maxItems) break;

                // Remove system name prefix from body name if it was included
                string rawName = vb.Name ?? string.Empty;
                string nameText = rawName;
                if (!string.IsNullOrEmpty(systemName) && !string.IsNullOrEmpty(rawName))
                {
                    if (rawName.StartsWith(systemName + " ", StringComparison.OrdinalIgnoreCase))
                        nameText = rawName.Substring(systemName.Length).TrimStart(' ', '-', ':');
                    else if (rawName.StartsWith(systemName + " -", StringComparison.OrdinalIgnoreCase))
                        nameText = rawName.Substring(systemName.Length).TrimStart(' ', '-', ':');
                }

                string reasonText = vb.Reason ?? string.Empty;

                float bx = boxLeft;
                float by = y;

                // Draw box background and border (rounded)
                var rect = new SKRect(bx, by, bx + boxWidth, by + boxHeight);
                canvas.DrawRoundRect(rect, 6f, 6f, boxBgPaint);
                canvas.DrawRoundRect(rect, 6f, 6f, boxBorderPaint);

                // Text positions inside box
                float textX = bx + padding;
                float itemNameY = by + padding + vbNamePaint.TextSize;
                float reasonY = itemNameY + vbReasonPaint.TextSize * 0.9f;

                // Draw name left
                canvas.DrawText(nameText, textX, itemNameY, vbNamePaint);
                // Draw distance right-aligned on the same line
                var distanceText = vb.Distance > 0 ? $"{vb.Distance:F1} ls" : string.Empty;
                if (!string.IsNullOrEmpty(distanceText))
                {
                    using var distPaint = new SKPaint { Color = TextSecondary, TextSize = vbReasonPaint.TextSize, IsAntialias = true };
                    float dx = bx + boxWidth - padding - TextWidth(distPaint, distanceText);
                    canvas.DrawText(distanceText, dx, itemNameY, distPaint);
                }

                if (!string.IsNullOrEmpty(reasonText))
                    canvas.DrawText(reasonText, textX, reasonY, vbReasonPaint);

                y += boxHeight + boxSpacing;
                idx++;
            }
        }

        canvas.Flush();
        return bitmap.Bytes;
    }

    private static void DrawAccentBars(SKCanvas canvas, int w, int h)
    {
        using var paint = new SKPaint { Color = Accent, IsAntialias = false };
        canvas.DrawRect(0, 0, w, 5, paint);
        canvas.DrawRect(0, h - 5, w, 5, paint);
    }

    private static float TextWidth(SKPaint paint, string text)
    {
        var bounds = new SKRect();
        paint.MeasureText(text, ref bounds);
        return bounds.Width;
    }
}
