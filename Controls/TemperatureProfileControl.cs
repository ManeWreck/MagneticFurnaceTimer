using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.Controls;

public sealed class TemperatureProfileControl : Control
{
    public static readonly StyledProperty<IReadOnlyList<TemperaturePoint>?> PointsProperty =
        AvaloniaProperty.Register<TemperatureProfileControl, IReadOnlyList<TemperaturePoint>?>(nameof(Points));

    public static readonly StyledProperty<double> CurrentMinuteProperty =
        AvaloniaProperty.Register<TemperatureProfileControl, double>(nameof(CurrentMinute));

    public static readonly StyledProperty<double> TotalMinutesProperty =
        AvaloniaProperty.Register<TemperatureProfileControl, double>(nameof(TotalMinutes));

    public static readonly StyledProperty<double> CurrentTemperatureProperty =
        AvaloniaProperty.Register<TemperatureProfileControl, double>(nameof(CurrentTemperature));

    static TemperatureProfileControl()
    {
        AffectsRender<TemperatureProfileControl>(PointsProperty, CurrentMinuteProperty, TotalMinutesProperty, CurrentTemperatureProperty);
    }

    public IReadOnlyList<TemperaturePoint>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public double CurrentMinute
    {
        get => GetValue(CurrentMinuteProperty);
        set => SetValue(CurrentMinuteProperty, value);
    }

    public double TotalMinutes
    {
        get => GetValue(TotalMinutesProperty);
        set => SetValue(TotalMinutesProperty, value);
    }

    public double CurrentTemperature
    {
        get => GetValue(CurrentTemperatureProperty);
        set => SetValue(CurrentTemperatureProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Points is not { Count: > 0 } points || Bounds.Width < 220 || Bounds.Height < 150)
            return;

        var plot = new Rect(58, 18, Math.Max(1, Bounds.Width - 78), Math.Max(1, Bounds.Height - 54));
        var total = Math.Max(TotalMinutes, points.Max(point => point.Minute));
        if (total <= 0) total = 1;

        var minimum = Math.Floor((points.Min(point => point.TemperatureC) - 20) / 50) * 50;
        var maximum = Math.Ceiling((points.Max(point => point.TemperatureC) + 20) / 50) * 50;
        if (Math.Abs(maximum - minimum) < 1) maximum = minimum + 100;

        var gridPen = new Pen(Brush.Parse("#26364E"), 1);
        var axisBrush = Brush.Parse("#8191A9");
        var profilePen = new Pen(Brush.Parse("#38BDF8"), 3);
        var markerBrush = Brush.Parse("#FBBF24");
        var markerPen = new Pen(markerBrush, 2, new DashStyle([5, 4], 0));

        for (var index = 0; index <= 4; index++)
        {
            var y = plot.Top + plot.Height * index / 4;
            context.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
            var temperature = maximum - (maximum - minimum) * index / 4;
            DrawText(context, $"{temperature:0}°", new Point(8, y - 8), 11, axisBrush);
        }

        for (var index = 0; index <= 5; index++)
        {
            var x = plot.Left + plot.Width * index / 5;
            context.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
            var minutes = total * index / 5;
            DrawText(context, FormatMinutes(minutes), new Point(x - 16, plot.Bottom + 8), 10, axisBrush);
        }

        Point Map(TemperaturePoint point) => new(
            plot.Left + plot.Width * Math.Clamp(point.Minute / total, 0, 1),
            plot.Bottom - plot.Height * Math.Clamp((point.TemperatureC - minimum) / (maximum - minimum), 0, 1));

        var geometry = new StreamGeometry();
        using (var geometryContext = geometry.Open())
        {
            geometryContext.BeginFigure(Map(points[0]), false);
            foreach (var point in points.Skip(1)) geometryContext.LineTo(Map(point));
        }
        context.DrawGeometry(null, profilePen, geometry);

        var currentMinute = Math.Clamp(CurrentMinute, 0, total);
        var markerX = plot.Left + plot.Width * currentMinute / total;
        context.DrawLine(markerPen, new Point(markerX, plot.Top), new Point(markerX, plot.Bottom));

        if (!double.IsNaN(CurrentTemperature))
        {
            var markerY = plot.Bottom - plot.Height * Math.Clamp((CurrentTemperature - minimum) / (maximum - minimum), 0, 1);
            context.DrawEllipse(markerBrush, new Pen(Brushes.White, 2), new Point(markerX, markerY), 6, 6);
        }
    }

    private static void DrawText(DrawingContext context, string text, Point origin, double size, IBrush brush)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            size,
            brush);
        context.DrawText(formatted, origin);
    }

    private static string FormatMinutes(double minutes)
    {
        var span = TimeSpan.FromMinutes(minutes);
        return span.TotalHours >= 1 ? $"{(int)span.TotalHours}ч{span.Minutes:00}" : $"{span.Minutes}м";
    }
}
