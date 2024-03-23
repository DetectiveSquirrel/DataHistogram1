using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DataHistogram1;

public class StatDataItem
{
    public StatDataItem(string key, string displayText, Vector2 size)
    {
        Key = key;
        DisplayText = displayText;
        DisplaySize = size;
        Data = new List<(DateTime, int)>();
    }

    public string Key { get; }
    public string DisplayText { get; }
    public Vector2 DisplaySize { get; }
    public List<(DateTime, int)> Data { get; }

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
}

public class StatTracker
{
    private static readonly DataHistogram1 Main = DataHistogram1.Main;
    private readonly List<StatDataItem> statDataItems = new();

    private DateTime lastUpdateTime = DateTime.Now;
    public double UpdateFrequency { get; set; } = 250; // Update time in ms default
    public double MaxDataAge { get; set; } = 120;      // Maximum age in seconds default

    public void AddFilterKey(string key, string displayText, Vector2 size)
    {
        if (statDataItems.Any(item => item.Key == key))
        {
            return;
        }

        var newDataItem = new StatDataItem(key, displayText, size);
        statDataItems.Add(newDataItem);
    }

    public void Tick()
    {
        var now = DateTime.Now;

        if ((DateTime.Now - lastUpdateTime).Milliseconds >= UpdateFrequency)
        {
            UpdateStats();
            lastUpdateTime = now;
        }

        RemoveOldData(now);
    }

    private void UpdateStats()
    {
        Main.GameController.Game.IngameState.Data.LocalPlayer.TryGetComponent<Stats>(out var playerStatComponent);

        if (playerStatComponent == null)
        {
            return;
        }

        var statHashSet = playerStatComponent.StatDictionary.Keys.ToHashSet();

        foreach (var dataItem in statDataItems)
        {
            if (!TryParseGameStat(dataItem.Key, out var statEnum))
            {
                continue;
            }

            if (!statHashSet.Contains(statEnum))
            {
                continue;
            }

            var value = playerStatComponent.StatDictionary[statEnum];
            dataItem.AddDataPoint(DateTime.Now, value);
        }
    }

    private void RemoveOldData(DateTime currentTime)
    {
        foreach (var dataItem in statDataItems)
            dataItem.RemoveOldData(currentTime, MaxDataAge);
    }

    public bool TryParseGameStat(string statString, out GameStat statEnum)
    {
        if (Enum.TryParse(statString, out statEnum))
        {
            return true;
        }

        return false;
    }

    public void DisplayStatsOverTime()
    {
        foreach (var dataItem in statDataItems)
        {
            var data = dataItem.GetDataValues();
            var (min, max) = dataItem.GetMinMax();
            var overlayText = $"Max: {max}\nMin: {min}";

            ImGui.PlotLines(
                $"{dataItem.DisplayText}##Plot{dataItem.Key}",
                ref data[0],
                data.Length,
                0,
                overlayText,
                float.MaxValue,
                float.MaxValue,
                dataItem.DisplaySize
            );
        }
    }
}