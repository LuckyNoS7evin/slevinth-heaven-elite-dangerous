using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.VoCore.Renderers;

internal static class ExoBioScreen
{

    private static readonly SKColor Background    = new(0x08, 0x08, 0x12);
    private static readonly SKColor Accent        = new(0x00, 0xCC, 0x66);  // green for bio
    private static readonly SKColor TextPrimary   = SKColors.White;
    private static readonly SKColor TextSecondary = new(0x99, 0x99, 0xAA);
    private static readonly SKColor TextDim       = new(0x55, 0x55, 0x66);
    private static readonly SKColor DotFilled     = new(0x00, 0xCC, 0x66);
    private static readonly SKColor DotEmpty      = new(0x33, 0x33, 0x44);

    private static readonly SKColor[] ScanTypeColors =
    [
        new(0xAA, 0x44, 0x00), // Log     — muted orange
        new(0xFF, 0x99, 0x00), // Sample  — bright amber
        new(0x00, 0xCC, 0x66), // Analyse — green (complete)
    ];

    private static readonly string[] ScanTypeLabels = ["LOG", "SAMPLE", "ANALYSED"];

    /// <summary>
    /// Renders the ExoBiology progress screen and returns an RGB565 byte array at the given dimensions.
    /// Designed for portrait orientation (width &lt; height).
    /// </summary>
    public static byte[] Render(IReadOnlyList<ActiveDiscovery> discoveries, int width, int height)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgb565);
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(Background);

        DrawAccentBars(canvas, width, height);
        DrawHeader(canvas, width, height, discoveries.Count);

        if (discoveries.Count == 0)
            DrawNoScans(canvas, width, height);
        else
            DrawDiscoveries(canvas, discoveries, width, height);

        canvas.Flush();
        return bitmap.Bytes;
    }

    /// <summary>
    /// Render a single full discovery with all available details.
    /// </summary>
    public static byte[] RenderLastDiscovery(ActiveDiscovery d, int width, int height)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgb565);
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(Background);
        DrawAccentBars(canvas, width, height);
        DrawHeader(canvas, width, height, 1);

        // Dynamic sizing based on display height to use full portrait area
        float cx = width / 2f;
        float top = height * 0.12f; // leave room for header
        float bottom = height * 0.06f; // leave room for footer accent
        float available = height - top - bottom;

        // Font sizes scale with height
        float titleSize = Math.Clamp(available * 0.12f, 28f, 72f);
        float metaSize = Math.Clamp(available * 0.06f, 16f, 36f);
        float detailsSize = Math.Clamp(available * 0.045f, 14f, 24f);
        float valueSize = Math.Clamp(available * 0.05f, 14f, 28f);

        float y = top + titleSize;

        // Species / Title (large)
        using var titlePaint = new SKPaint { Color = Accent, TextSize = titleSize, IsAntialias = true, FakeBoldText = true };
        string title = string.IsNullOrEmpty(d.Name) ? d.Species : d.Name;
        canvas.DrawText(title, cx - TextWidth(titlePaint, title) / 2f, y, titlePaint);

        // System and body
        y += titleSize * 0.9f;
        using var metaPaint = new SKPaint { Color = TextSecondary, TextSize = metaSize, IsAntialias = true };
        string sys = string.IsNullOrEmpty(d.SystemName) ? "Unknown system" : d.SystemName;
        string body = string.IsNullOrEmpty(d.BodyName) ? "" : $" • {d.BodyName}";
        string meta = sys + body;
        canvas.DrawText(meta, cx - TextWidth(metaPaint, meta) / 2f, y, metaPaint);

        // Details area: occupy the central portion
        y += metaSize * 1.2f;
        using var detailsPaint = new SKPaint { Color = TextPrimary, TextSize = detailsSize, IsAntialias = true };
        float maxWidth = width - 24f;
        var details = d.Details ?? string.Empty;

        // Estimate number of lines that will fit in the remaining space
        float used = y + valueSize * 3 + 40f; // room for values and spacing
        float remaining = Math.Max(available - (used - top), available * 0.4f);
        int estimatedLines = Math.Max(2, (int)(remaining / (detailsSize * 1.2f)));
        estimatedLines = Math.Min(estimatedLines, 12);

        var lines = WrapText(details, detailsPaint, maxWidth, estimatedLines);
        foreach (var line in lines)
        {
            canvas.DrawText(line, 12f, y, detailsPaint);
            y += detailsSize * 1.2f;
        }

        y += 10f;

        // Values area: show either a single Sample box (when not analysed)
        // or two boxes (Estimated and Bonus) when analysed.
        string sampleText = d.SampleValue > 0 ? $"{d.SampleValue:N0} CR" : "—";
        string estimatedText = d.EstimatedValue > 0 ? $"{d.EstimatedValue:N0} CR" : "—";
        // Bonus converted to CR per request
        string bonusText = d.EstimatedBonus > 0 ? $"{d.EstimatedBonus:N0} CR" : "—";

        // Reserve a box area near the lower portion of the content area
        float boxAreaTop = Math.Min(y, height * 0.55f);
        float boxAreaBottom = height - bottom - 8f;
        float boxAreaHeight = Math.Max((boxAreaBottom - boxAreaTop), valueSize * 2.5f);
        float boxLeft = 12f;
        float boxRight = width - 12f;
        float boxWidth = boxRight - boxLeft;

        using var boxPaint = new SKPaint { IsAntialias = true };
        using var boxTextPaint = new SKPaint { Color = TextPrimary, TextSize = valueSize * 1.1f, IsAntialias = true, FakeBoldText = true };

        int progress = d.ScanProgress;
        if (progress == 3)
        {
            // Analysed: show Estimated (top) and Bonus (bottom) — two equal boxes
            float boxHeight = boxAreaHeight / 2f;

            // Estimated box
            float bx = boxLeft;
            float by = boxAreaTop;
            boxPaint.Color = new SKColor(0x33, 0x99, 0xFF);
            canvas.DrawRect(bx, by, boxWidth, boxHeight - 4f, boxPaint);
            var txt = estimatedText;
            canvas.DrawText(txt, bx + boxWidth / 2f - TextWidth(boxTextPaint, txt) / 2f, by + boxHeight / 2f + boxTextPaint.TextSize / 3f, boxTextPaint);

            // Bonus box
            by += boxHeight;
            boxPaint.Color = Accent;
            canvas.DrawRect(bx, by, boxWidth, boxHeight - 4f, boxPaint);
            txt = bonusText;
            canvas.DrawText(txt, bx + boxWidth / 2f - TextWidth(boxTextPaint, txt) / 2f, by + boxHeight / 2f + boxTextPaint.TextSize / 3f, boxTextPaint);
        }
        else
        {
            // Not analysed: show single Sample box occupying the area
            float boxHeight = boxAreaHeight;
            float bx = boxLeft;
            float by = boxAreaTop;
            boxPaint.Color = ScanTypeColors.Length > 1 ? ScanTypeColors[1] : new SKColor(0xFF, 0x99, 0x00);
            canvas.DrawRect(bx, by, boxWidth, boxHeight - 4f, boxPaint);
            var txt = sampleText;
            canvas.DrawText(txt, bx + boxWidth / 2f - TextWidth(boxTextPaint, txt) / 2f, by + boxHeight / 2f + boxTextPaint.TextSize / 3f, boxTextPaint);
        }

        // Progress dots removed for single-discovery layout (clean, box-focused design)

        canvas.Flush();
        return bitmap.Bytes;
    }

    private static void DrawAccentBars(SKCanvas canvas, int w, int h)
    {
        using var paint = new SKPaint { Color = Accent, IsAntialias = false };
        canvas.DrawRect(0, 0, w, 5, paint);
        canvas.DrawRect(0, h - 5, w, 5, paint);
    }

    private static void DrawHeader(SKCanvas canvas, int w, int h, int count)
    {
        float cx = w / 2f;
        // Use a slightly smaller max so header doesn't grow too large on tall displays.
        float textSize = Math.Clamp(h * 0.05f, 24f, 36f);
        using var titlePaint = new SKPaint { Color = Accent, TextSize = textSize, IsAntialias = true, FakeBoldText = true };
        float y = h * 0.10f;
        canvas.DrawText("EXOBIOLOGY", cx - TextWidth(titlePaint, "EXOBIOLOGY") / 2f, y, titlePaint);
    }

    private static void DrawNoScans(SKCanvas canvas, int w, int h)
    {
        using var paint = new SKPaint { Color = TextDim, TextSize = 28f, IsAntialias = true };
        canvas.DrawText("No active scans", w / 2f - TextWidth(paint, "No active scans") / 2f, h / 2f, paint);
    }

    private static void DrawDiscoveries(SKCanvas canvas, IReadOnlyList<ActiveDiscovery> discoveries, int w, int h)
    {
        float rowStart  = 110f;
        float rowEnd    = h - 20f;
        int   maxRows   = Math.Min(discoveries.Count, 4);
        float rowHeight = (rowEnd - rowStart) / maxRows;

        for (int i = 0; i < maxRows; i++)
        {
            float rowY = rowStart + i * rowHeight;
            DrawDiscoveryRow(canvas, discoveries[i], rowY, rowHeight, w);
        }

        if (discoveries.Count > 4)
        {
            using var morePaint = new SKPaint { Color = TextDim, TextSize = 18f, IsAntialias = true, TextAlign = SKTextAlign.Right };
            canvas.DrawText($"+{discoveries.Count - 4} more", w - 12f, rowEnd + 20f, morePaint);
        }
    }

    private static void DrawDiscoveryRow(SKCanvas canvas, ActiveDiscovery discovery, float rowY, float rowHeight, int w)
    {
        float centerY = rowY + rowHeight / 2f;

        using var sepPaint = new SKPaint { Color = new SKColor(0x22, 0x22, 0x33), IsAntialias = false };
        canvas.DrawLine(12f, rowY, w - 12f, rowY, sepPaint);

        int     progress    = Math.Clamp(discovery.ScanProgress, 0, 3);
        SKColor statusColor = progress > 0 ? ScanTypeColors[progress - 1] : TextDim;
        string  statusLabel = progress > 0 ? ScanTypeLabels[progress - 1] : "UNKNOWN";

        // Progress dots (top-right of row)
        DrawProgressDots(canvas, progress, w - 16f, rowY + 20f);

        // Species name
        float nameMaxWidth = w - 24f;
        using var namePaint = new SKPaint { Color = TextPrimary, TextSize = 22f, IsAntialias = true };
        string displayName = TruncateText(discovery.Species, namePaint, nameMaxWidth);
        canvas.DrawText(displayName, 12f, centerY, namePaint);

        // Scan type label below species name
        using var statusPaint = new SKPaint { Color = statusColor, TextSize = 18f, IsAntialias = true, FakeBoldText = true };
        canvas.DrawText(statusLabel, 12f, centerY + 26f, statusPaint);
    }

    private static void DrawProgressDots(SKCanvas canvas, int filled, float rightX, float centerY)
    {
        const float dotRadius  = 7f;
        const float dotSpacing = 20f;

        for (int d = 2; d >= 0; d--)
        {
            float cx = rightX - d * dotSpacing;
            using var paint = new SKPaint { Color = (2 - d) < filled ? DotFilled : DotEmpty, IsAntialias = true };
            canvas.DrawCircle(cx, centerY, dotRadius, paint);
        }
    }

    private static void DrawProgressDotsLarge(SKCanvas canvas, int filled, float rightX, float centerY)
    {
        const float dotRadius  = 10f;
        const float dotSpacing = 26f;

        for (int d = 2; d >= 0; d--)
        {
            float cx = rightX - d * dotSpacing;
            using var paint = new SKPaint { Color = (2 - d) < filled ? DotFilled : DotEmpty, IsAntialias = true };
            canvas.DrawCircle(cx, centerY, dotRadius, paint);
        }
    }

    private static float TextWidth(SKPaint paint, string text)
    {
        var bounds = new SKRect();
        paint.MeasureText(text, ref bounds);
        return bounds.Width;
    }

    private static string TruncateText(string text, SKPaint paint, float maxWidth)
    {
        if (TextWidth(paint, text) <= maxWidth) return text;
        while (text.Length > 3 && TextWidth(paint, text + "…") > maxWidth)
            text = text[..^1];
        return text + "…";
    }

    private static IEnumerable<string> WrapText(string text, SKPaint paint, float maxWidth, int maxLines)
    {
        var paragraphs = (text ?? string.Empty).Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        var lines = new List<string>();

        foreach (var para in paragraphs)
        {
            if (string.IsNullOrEmpty(para))
            {
                if (lines.Count < maxLines) lines.Add(string.Empty);
                continue;
            }

            var words = para.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var current = string.Empty;

            foreach (var w in words)
            {
                var trial = string.IsNullOrEmpty(current) ? w : current + " " + w;
                if (TextWidth(paint, trial) <= maxWidth)
                {
                    current = trial;
                }
                else
                {
                    lines.Add(current);
                    current = w;
                    if (lines.Count >= maxLines) break;
                }
            }

            if (!string.IsNullOrEmpty(current) && lines.Count < maxLines)
                lines.Add(current);

            // After each paragraph, insert a blank line if space permits to preserve explicit breaks
            if (lines.Count < maxLines)
                lines.Add(string.Empty);

            if (lines.Count >= maxLines) break;
        }

        // Trim trailing empty line if it ended up at end
        if (lines.Count > 0 && string.IsNullOrEmpty(lines[^1]))
            lines.RemoveAt(lines.Count - 1);

        // If truncated, add ellipsis to last line
        if (lines.Count == maxLines && !string.IsNullOrEmpty(lines[^1]))
        {
            var last = lines[^1];
            while (TextWidth(paint, last + "…") > maxWidth && last.Length > 1)
                last = last[..^1];
            lines[^1] = last + "…";
        }

        return lines;
    }
}
