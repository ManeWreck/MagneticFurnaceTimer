using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.Services;

public static class TemperatureTimeline
{
    public static IReadOnlyList<TemperaturePoint> BuildPoints(FurnaceProfile profile)
    {
        var points = new List<TemperaturePoint>();
        var minute = 0d;
        double? currentTemperature = null;

        foreach (var stage in profile.Stages)
        {
            var target = stage.SetTemperatureC ?? currentTemperature;
            if (target is null) continue;

            if (currentTemperature is null)
            {
                currentTemperature = target;
                points.Add(new TemperaturePoint(minute, target.Value));
            }

            minute += stage.DurationMinutes;
            currentTemperature = target;

            var point = new TemperaturePoint(minute, target.Value);
            if (points.Count == 0 || points[^1] != point)
            {
                points.Add(point);
            }
        }

        return points;
    }

    public static double? GetExpectedTemperature(FurnaceProfile profile, double elapsedMinutes)
    {
        var minute = 0d;
        double? startTemperature = null;

        foreach (var stage in profile.Stages)
        {
            var targetTemperature = stage.SetTemperatureC ?? startTemperature;
            if (targetTemperature is null) continue;

            if (startTemperature is null || stage.DurationMinutes <= 0)
            {
                startTemperature = targetTemperature;
                if (elapsedMinutes <= minute) return startTemperature;
                continue;
            }

            var stageEnd = minute + stage.DurationMinutes;
            if (elapsedMinutes <= stageEnd)
            {
                var fraction = Math.Clamp((elapsedMinutes - minute) / stage.DurationMinutes, 0, 1);
                return startTemperature.Value + (targetTemperature.Value - startTemperature.Value) * fraction;
            }

            minute = stageEnd;
            startTemperature = targetTemperature;
        }

        return startTemperature;
    }
}
