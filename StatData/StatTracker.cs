using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DataHistogram1.StatData;

public class StatTracker
{
    private static readonly DataHistogram1 Main = DataHistogram1.Main;

    private static readonly Dictionary<string, GameStat> CachedStats = [];

    private DateTime lastUpdateTime = DateTime.Now;
    public List<StatDataParent> statDataParents = Main.Settings.statDataParents;

    public void AddFilterParent(string parentMenu)
    {
        FindOrCreateParent(parentMenu);
    }

    public void Tick()
    {
        var now = DateTime.Now;

        if ((DateTime.Now - lastUpdateTime).Milliseconds >= Main.Settings.UpdateFrequency)
        {
            UpdateStats();
            lastUpdateTime = now;
        }

        RemoveOldData(now);
    }

    private void UpdateStats()
    {
        UpdateStatsFromComponent();
        FetchAndAddCustomStats();
    }

    private void UpdateStatsFromComponent()
    {
        var player = Main.GameController.Game.IngameState.Data.LocalPlayer;

        if (player?.Stats == null)
        {
            return;
        }

        var playerStats = player.Stats;

        foreach (var dataItem in statDataParents.SelectMany(
                     parent => parent.Items.Where(x => !x.IsCustom && x.ShouldUpdate)
                 ))
        {
            if (!TryParseGameStat(dataItem.Key, out var statEnum))
            {
                continue;
            }

            dataItem.AddDataPoint(DateTime.Now, playerStats[statEnum]);
        }
    }

    private void FetchAndAddCustomStats()
    {
        var player = Main.GameController.Game.IngameState.Data.LocalPlayer;
        player.TryGetComponent<Life>(out var lifeComponent);

        if (lifeComponent == null)
        {
            return;
        }

        AddLifeManaCustomModule(lifeComponent, "Life - Hardcoded Module", "playerLife", "Life + ES");
        AddLifeManaCustomModule(lifeComponent, "Mana - Hardcoded Module", "playerMana", "Mana");
    }

    private void AddLifeManaCustomModule(Life lifeComponent, string parentModule, string key, string displayText)
    {
        var customParent = FindOrCreateParent(parentModule);

        var statValue = key switch
        {
            "playerLife" => lifeComponent.CurHP + lifeComponent.CurES,
            "playerMana" => lifeComponent.CurMana,
            _ => 0
        };

        var customDataItem = FindOrCreateItem(customParent, key, displayText, Vector2.Zero, true);

        if (customDataItem?.ShouldUpdate ?? false)
        {
            customDataItem.AddDataPoint(DateTime.Now, statValue);
        }
    }

    private static StatDataItem FindOrCreateItem(StatDataParent parent, string statKey, string displayText,
        Vector2 size, bool isCustom)
    {
        var dataItem = parent?.Items.FirstOrDefault(item => item.Key == statKey);

        if (dataItem != null)
        {
            return dataItem;
        }

        dataItem = new StatDataItem(statKey, displayText, size, isCustom);
        parent?.AddItem(dataItem);
        return dataItem;
    }

    private StatDataParent FindOrCreateParent(string parentName)
    {
        var parent = statDataParents.FirstOrDefault(p => p.ParentName == parentName);

        if (parent != null)
        {
            return parent;
        }

        parent = new StatDataParent(parentName);
        statDataParents.Add(parent);
        return parent;
    }

    private void RemoveOldData(DateTime currentTime)
    {
        foreach (var dataItem in statDataParents.SelectMany(parent => parent.Items.Where(x => x.ShouldUpdate)))
            dataItem.RemoveOldData(currentTime, Main.Settings.MaxDataAge);
    }

    public static bool TryParseGameStat(string statString, out GameStat statEnum)
    {
        if (CachedStats.TryGetValue(statString, out statEnum))
        {
            return true;
        }

        if (!Enum.TryParse(statString, out statEnum))
        {
            return false;
        }

        CachedStats[statString] = statEnum;
        return true;
    }

    private static ImGuiWindowFlags GetWindowFlags()
    {
        ImGuiWindowFlags flags = 0;

        if (Main.Settings.AlwaysAutoResize)
        {
            flags |= ImGuiWindowFlags.AlwaysAutoResize;
        }

        if (Main.Settings.NoBackground)
        {
            flags |= ImGuiWindowFlags.NoBackground;
        }

        if (Main.Settings.NoMove)
        {
            flags |= ImGuiWindowFlags.NoMove;
        }

        if (Main.Settings.NoTitleBar)
        {
            flags |= ImGuiWindowFlags.NoTitleBar;
        }

        if (Main.Settings.NoInputs)
        {
            flags |= ImGuiWindowFlags.NoInputs;
        }

        return flags;
    }

    public static void SetCustomStyling(StatDataItem itemData)
    {
        if (!itemData.CustomStyling)
        {
            return;
        }

        ImGui.PushStyleColor(ImGuiCol.PlotLines, itemData.PlotLineColor.ToImguiVec4());
        ImGui.PushStyleColor(ImGuiCol.FrameBg, itemData.PlotBackgroundColor.ToImguiVec4());
        ImGui.PushStyleColor(ImGuiCol.Text, itemData.TextColor.ToImguiVec4());
    }

    public static void PopStyleColors(int count)
    {
        ImGui.PopStyleColor(count);
    }

    public void DisplayStatsOverTime()
    {
        foreach (var parent in statDataParents)
        {
            ImGui.Begin($"{parent.ParentName}##ParentStatWindow-{parent.ParentName}", GetWindowFlags());

            foreach (var dataItem in parent.Items)
            {
                if (!dataItem.ShouldDisplay)
                {
                    continue;
                }

                var data = dataItem.GetDataValues();
                var (min, max) = dataItem.GetMinMax();
                var overlayText = dataItem.PlotLineMinMaxText ? $"Max: {max}\nMin: {min}" : "";

                var plotLineDisplayText = dataItem.PlotLineDisplayText
                    ? $"{dataItem.DisplayText}\n[{dataItem.GetLastValue()}]##StatPlot-{dataItem.Key}"
                    : $"##StatPlot-{dataItem.Key}";

                if (data.Length <= 0)
                {
                    continue;
                }

                if (dataItem.CustomStyling)
                {
                    SetCustomStyling(dataItem);
                }

                ImGui.PlotLines(
                    plotLineDisplayText,
                    ref data[0],
                    data.Length,
                    0,
                    overlayText,
                    float.MaxValue,
                    float.MaxValue,
                    dataItem.DisplaySize
                );

                if (dataItem.CustomStyling)
                {
                    PopStyleColors(3);
                }
            }

            ImGui.EndChild();
        }
    }
}