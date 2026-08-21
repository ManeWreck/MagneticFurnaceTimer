using ClosedXML.Excel;
using MagneticFurnaceTimer.Models;
using MagneticFurnaceTimer.Services;

namespace MagneticFurnaceTimer.Tests;

public sealed class ProfileAndStorageTests
{
    [Fact]
    public void StandardProfile_IsReadWithExpectedTimeline()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"furnace-profile-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "D50609M022.xlsx");
        Directory.CreateDirectory(directory);

        try
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.AddWorksheet("Profile Designer");
                sheet.Cell("B10").Value = "Step";
                sheet.Cell("C10").Value = "Segment Label";
                sheet.Cell("D10").Value = "Rates (C°/min)";
                sheet.Cell("E10").Value = "Set Temp (C°)";
                sheet.Cell("F10").Value = "Time to set Temp (min)";

                var labels = new[] { "Initial Temp", "1st Heating", "1st Thermalization", "2nd Heating", "2nd Thermalization", "Heating to Tmax", "Holding on Tmax", "Cooling to QFT", "", "" };
                var temperatures = new[] { 200, 380, 380, 470, 470, 575, 575, 550, 550, 300 };
                var durations = new[] { 0, 90, 30, 60, 30, 60, 170, 30, 120, 60 };

                for (var index = 0; index < labels.Length; index++)
                {
                    var row = 11 + index;
                    sheet.Cell(row, 2).Value = index;
                    sheet.Cell(row, 3).Value = labels[index];
                    sheet.Cell(row, 5).Value = temperatures[index];
                    sheet.Cell(row, 6).Value = durations[index];
                }

                workbook.SaveAs(path);
            }

            var profile = new ExcelProfileReader().Read(path);

            Assert.Equal("D50609M022", profile.Name);
            Assert.Equal(10, profile.Stages.Count);
            Assert.Equal(650, profile.TotalMinutes);
            Assert.Equal("1st Heating", profile.Stages[1].Label);
            Assert.Equal(90, profile.Stages[1].DurationMinutes);
            Assert.Equal("Этап 8", profile.Stages[8].Label);
            Assert.Equal(120, profile.Stages[8].DurationMinutes);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void RunState_RoundTripsUsingAbsoluteUtcTime()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"furnace-timer-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "active-run.json");
        var storage = new RunStorage(path);
        var start = new DateTimeOffset(2026, 8, 21, 8, 0, 0, TimeSpan.Zero);
        var profile = new FurnaceProfile(
            "Test",
            "test.xlsx",
            [new FurnaceStage(1, "Heating", 500, 2, 90)]);

        try
        {
            storage.Save(new SavedRun(profile, start, start));
            var restored = storage.Load();

            Assert.NotNull(restored);
            Assert.Equal(start, restored.StartUtc);
            Assert.Equal(90, restored.Profile.TotalMinutes);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
