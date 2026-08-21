using ClosedXML.Excel;
using MagneticFurnaceTimer.Models;

namespace MagneticFurnaceTimer.Services;

public sealed class ExcelProfileReader
{
    public FurnaceProfile Read(string path)
    {
        using var workbook = new XLWorkbook(path);

        foreach (var sheet in workbook.Worksheets)
        {
            var used = sheet.RangeUsed();
            if (used is null) continue;

            var firstRow = used.RangeAddress.FirstAddress.RowNumber;
            var lastRow = used.RangeAddress.LastAddress.RowNumber;
            var firstColumn = used.RangeAddress.FirstAddress.ColumnNumber;
            var lastColumn = used.RangeAddress.LastAddress.ColumnNumber;

            for (var row = firstRow; row <= Math.Min(lastRow, firstRow + 100); row++)
            {
                var columns = FindColumns(sheet, row, firstColumn, lastColumn);
                if (columns is null) continue;

                var stages = ReadStages(sheet, row + 1, lastRow, columns.Value);
                if (stages.Count == 0) continue;

                return new FurnaceProfile(
                    Path.GetFileNameWithoutExtension(path),
                    Path.GetFullPath(path),
                    stages);
            }
        }

        throw new InvalidDataException(
            "Не найдена таблица этапов. Ожидаются столбцы Step и Time to set Temp (min)." );
    }

    private static (int Step, int Label, int Temperature, int Rate, int Duration)? FindColumns(
        IXLWorksheet sheet, int row, int firstColumn, int lastColumn)
    {
        var step = 0;
        var label = 0;
        var temperature = 0;
        var rate = 0;
        var duration = 0;

        for (var column = firstColumn; column <= lastColumn; column++)
        {
            var header = Normalize(sheet.Cell(row, column).GetFormattedString());
            if (header == "step") step = column;
            else if (header.Contains("segmentlabel")) label = column;
            else if (header.Contains("settemp") && !header.Contains("time")) temperature = column;
            else if (header.Contains("rate")) rate = column;
            else if (header.Contains("timetosettemp")) duration = column;
        }

        return step > 0 && duration > 0
            ? (step, label, temperature, rate, duration)
            : null;
    }

    private static List<FurnaceStage> ReadStages(
        IXLWorksheet sheet,
        int firstDataRow,
        int lastRow,
        (int Step, int Label, int Temperature, int Rate, int Duration) columns)
    {
        var stages = new List<FurnaceStage>();
        var consecutiveEmptyRows = 0;

        for (var row = firstDataRow; row <= lastRow; row++)
        {
            if (!TryReadNumber(sheet.Cell(row, columns.Step), out var stepNumber))
            {
                if (stages.Count > 0 && ++consecutiveEmptyRows >= 2) break;
                continue;
            }

            if (!TryReadNumber(sheet.Cell(row, columns.Duration), out var duration) || duration < 0)
                continue;

            consecutiveEmptyRows = 0;
            var step = (int)Math.Round(stepNumber);
            var label = columns.Label > 0
                ? sheet.Cell(row, columns.Label).GetFormattedString().Trim()
                : string.Empty;

            stages.Add(new FurnaceStage(
                step,
                string.IsNullOrWhiteSpace(label) ? $"Этап {step}" : label,
                ReadOptionalNumber(sheet, row, columns.Temperature),
                ReadOptionalNumber(sheet, row, columns.Rate),
                duration));
        }

        return stages;
    }

    private static double? ReadOptionalNumber(IXLWorksheet sheet, int row, int column)
        => column > 0 && TryReadNumber(sheet.Cell(row, column), out var value) ? value : null;

    private static bool TryReadNumber(IXLCell cell, out double value)
    {
        if (cell.TryGetValue(out value)) return true;
        return double.TryParse(
            cell.GetFormattedString(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    private static string Normalize(string value)
        => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
