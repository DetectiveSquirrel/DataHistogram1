using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace DataHistogram1.StatData;

public class StatDataItem(string key, string displayText, Vector2 size, bool isCustom)
{
    public string Key { get; set; } = key;
    public string DisplayText { get; set; } = displayText;
    public Vector2 DisplaySize { get; set; } = size;
    public List<(DateTime, int)> Data { get; } = [];
    public bool IsCustom { get; set; } = isCustom;
    public bool ShouldUpdate { get; set; } = true;
    public bool ShouldDisplay { get; set; } = true;
    public bool CustomStyling { get; set; } = false;
    public bool PlotLineMinMaxText { get; set; } = true;
    public bool PlotLineDisplayText { get; set; } = true;

    public Color PlotLineColor { get; set; } = ImGui.GetStyle().Colors[(int)ImGuiCol.PlotLines].ToSharpColor();
    public Color PlotBackgroundColor { get; set; } = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg].ToSharpColor();
    public Color TextColor { get; set; } = ImGui.GetStyle().Colors[(int)ImGuiCol.Text].ToSharpColor();

    public void AddDataPoint(DateTime timestamp, int value)
    {
        Data.Add((timestamp, value));
    }

    public void RemoveOldData(DateTime currentTime, double maxDataAge)
    {
        Data.RemoveAll(data => (currentTime - data.Item1).TotalSeconds > maxDataAge);
    }

    public float[] GetDataValues()
    {
        return Data.Select(item => (float)item.Item2).ToArray();
    }

    public (float, float) GetMinMax()
    {
        if (Data.Count == 0)
        {
            return (0, 0);
        }

        var min = Data.Min(item => item.Item2);
        var max = Data.Max(item => item.Item2);
        return (min, max);
    }

    public int GetLastValue() => Data.Count == 0 ? 0 : Data.Last().Item2;
}