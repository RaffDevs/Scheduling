using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Scheduling.Domain.Entities;

namespace Scheduling.Print.Services;

public class ScheduleSharePngService
{
    private static readonly Color BackgroundColor = Color.ParseHex("#f7f1e7");
    private static readonly Color CardColor = Color.ParseHex("#fffdf9");
    private static readonly Color HeaderColor = Color.ParseHex("#d66f4d");
    private static readonly Color RowColor = Color.ParseHex("#f3e6d6");
    private static readonly Color DarkText = Color.ParseHex("#2f241e");
    private static readonly Color LightText = Color.ParseHex("#fff5ef");

    public async Task<byte[]> GenerateAsync(int month, int year, IReadOnlyList<ScheduleEntry> entries, IReadOnlyList<Person> people)
    {
        var items = entries
            .OrderBy(x => x.Date)
            .Select(x => new ShareItem
            {
                DateText = x.Date.ToString("ddd, dd/MM", new System.Globalization.CultureInfo("pt-BR")),
                PersonName = people.FirstOrDefault(person => person.Id == x.PersonId)?.Name ?? "-"
            })
            .ToList();

        var height = Math.Max(560, 280 + (items.Count * 120));

        using var image = new Image<Rgba32>(1080, height, BackgroundColor);

        image.Mutate(context =>
        {
            context.Fill(CardColor, new RectangleF(40, 40, 1000, height - 80));
            context.Fill(HeaderColor, new RectangleF(40, 40, 1000, 170));

            DrawCenteredText(context, "Escala de Introdutores", GetFont(52, FontStyle.Bold), Color.White, 540, 110);
            DrawCenteredText(context, GetMonthYearText(month, year), GetFont(32, FontStyle.Bold), LightText, 540, 165);

            if (items.Count == 0)
            {
                DrawCenteredText(context, "Nenhuma escala disponivel", GetFont(34, FontStyle.Regular), DarkText, 540, 320);
                return;
            }

            for (var index = 0; index < items.Count; index++)
            {
                var y = 250 + (index * 120);

                context.Fill(RowColor, new RectangleF(90, y - 38, 900, 88));
                DrawLeftText(context, items[index].DateText, GetFont(30, FontStyle.Bold), DarkText, 125, y);
                DrawRightText(context, items[index].PersonName, GetFont(30, FontStyle.Regular), DarkText, 955, y);
            }
        });

        await using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream, new PngEncoder());
        return stream.ToArray();
    }

    private static string GetMonthYearText(int month, int year)
    {
        if (month < 1 || month > 12 || year <= 0)
        {
            return string.Empty;
        }

        var date = new DateTime(year, month, 1);
        return date.ToString("MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));
    }

    private static Font GetFont(float size, FontStyle style)
    {
        foreach (var name in new[] { "Aptos", "Segoe UI", "Arial", "Helvetica", "DejaVu Sans" })
        {
            if (SystemFonts.TryGet(name, out var family))
            {
                return family.CreateFont(size, style);
            }
        }

        return SystemFonts.Collection.Families.First().CreateFont(size, style);
    }

    private static void DrawCenteredText(IImageProcessingContext context, string text, Font font, Color color, float x, float y)
    {
        context.DrawText(new RichTextOptions(font)
        {
            Origin = new PointF(x, y),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        }, text, color);
    }

    private static void DrawLeftText(IImageProcessingContext context, string text, Font font, Color color, float x, float y)
    {
        context.DrawText(new RichTextOptions(font)
        {
            Origin = new PointF(x, y),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        }, text, color);
    }

    private static void DrawRightText(IImageProcessingContext context, string text, Font font, Color color, float x, float y)
    {
        context.DrawText(new RichTextOptions(font)
        {
            Origin = new PointF(x, y),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = 380
        }, text, color);
    }

    private class ShareItem
    {
        public string DateText { get; set; } = string.Empty;

        public string PersonName { get; set; } = string.Empty;
    }
}
