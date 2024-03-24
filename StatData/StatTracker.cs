using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace DataHistogram1.StatData;

public class StatTracker
{
    private static readonly DataHistogram1 Main = DataHistogram1.Main;
    private static readonly Dictionary<string, GameStat> CachedStats = new();
    private DateTime lastUpdateTime = DateTime.Now;
    public List<StatDataParent> statDataParents = Main.Settings.statDataParents;

    #region Standard Stat Methods

    public void AddFilterParent(string parentMenu)
    {
        FindOrCreateParent(parentMenu);
    }

    public void Tick()
    {
        var now = DateTime.Now;

        if ((now - lastUpdateTime).Milliseconds >= Main.Settings.UpdateFrequency)
        {
            UpdateStats();
            lastUpdateTime = now;
        }

        RemoveOldData(now);
    }

    private void UpdateStats()
    {
        UpdateStatsFromComponent();
        FetchAndAddCustomModules();
        //RighteousFireModule();
    }

    private void UpdateStatsFromComponent()
    {
        var player = Main.GameController.Game.IngameState.Data.LocalPlayer;

        if (player?.Stats == null)
        {
            return;
        }

        foreach (var dataItem in statDataParents.SelectMany(
                     parent => parent.Items.Where(x => !x.IsCustom && x.ShouldUpdate)
                 ))
        {
            if (!TryParseGameStat(dataItem.Key, out var statEnum))
            {
                continue;
            }

            dataItem.AddDataPoint(DateTime.Now, player.Stats[statEnum]);
        }
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

    private StatDataParent FindOrCreateParent(string parentName)
    {
        var parent = statDataParents.FirstOrDefault(p => p.ParentName == parentName);

        if (parent == null)
        {
            parent = new StatDataParent(parentName);
            statDataParents.Add(parent);
        }

        return parent;
    }

    #endregion

    #region Custom Module Methods

    public bool TryGetValue(Dictionary<GameStat, int> statDictionary, GameStat stat, out int value,
        int defaultValue = 0)
    {
        if (statDictionary.TryGetValue(stat, out value))
        {
            return true;
        }

        value = defaultValue;
        return false;
    }

    private void FetchAndAddCustomModules()
    {
        var player = Main.GameController.Game.IngameState.Data.LocalPlayer;

        if (player.TryGetComponent<Life>(out var lifeComponent))
        {
            AddLifeComponentModule(lifeComponent, "Life - Hardcoded Module", "playerLife", "Life + ES");
            AddLifeComponentModule(lifeComponent, "Mana - Hardcoded Module", "playerMana", "Mana");
        }
    }

    private void RighteousFireModule()
    {
        var player = Main.GameController.Game.IngameState.Data.LocalPlayer;

        // Initialize stat values with default of 0 if they do not exist
        TryGetValue(player.Stats, GameStat.TotalNonlethalFireDamageTakenPerMinute, out var finalDegenCalculation, 0);
        TryGetValue(player.Stats, GameStat.TotalDamageTakenPerMinuteToLife, out var finalOtherSourceDegen, 0);
        TryGetValue(player.Stats, GameStat.TotalLifeRecoveryPerMinuteFromRegeneration, out var finalLifeRegen, 0);

        // Calculate the final combined degeneration and total regeneration
        var finalCombinedDegen = -finalDegenCalculation / 60 + -finalOtherSourceDegen / 60;
        var finalTotalRegen = finalLifeRegen / 60 + -finalDegenCalculation / 60 + -finalOtherSourceDegen / 60;
        AddCustomModule("Regeneration Stats", "totalDegen", "Total Combined Degen", finalCombinedDegen);
        AddCustomModule("Regeneration Stats", "totalRegen", "Total Regeneration", finalTotalRegen);
    }

    private void AddCustomModule(string parentModule, string key, string displayText, int value)
    {
        var customParent = FindOrCreateParent(parentModule);
        var customDataItem = FindOrCreateItem(customParent, key, displayText, Vector2.Zero, true);

        if (customDataItem?.ShouldUpdate ?? false)
        {
            customDataItem.AddDataPoint(DateTime.Now, value);
        }
    }

    private void AddLifeComponentModule(Life lifeComponent, string parentModule, string key, string displayText)
    {
        var statValue = key switch
        {
            "playerLife" => lifeComponent.CurHP + lifeComponent.CurES,
            "playerMana" => lifeComponent.CurMana,
            _ => 0
        };

        var customParent = FindOrCreateParent(parentModule);
        var customDataItem = FindOrCreateItem(customParent, key, displayText, Vector2.Zero, true);

        if (customDataItem?.ShouldUpdate ?? false)
        {
            customDataItem.AddDataPoint(DateTime.Now, statValue);
        }
    }

    #endregion

    #region Utility Methods

    private static StatDataItem FindOrCreateItem(StatDataParent parent, string statKey, string displayText,
        Vector2 size, bool isCustom)
    {
        var dataItem = parent?.Items.FirstOrDefault(item => item.Key == statKey) ??
                       new StatDataItem(statKey, displayText, size, isCustom);

        if (!parent.Items.Contains(dataItem))
        {
            parent.AddItem(dataItem);
        }

        return dataItem;
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

    #endregion

    #region Display Methods

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

                if (data.Length <= 0)
                {
                    continue;
                }

                var (min, max) = dataItem.GetMinMax();
                var overlayText = dataItem.PlotLineMinMaxText ? $"Max: {max}\nMin: {min}" : "";

                var plotLineDisplayText = dataItem.PlotLineDisplayText
                    ? $"{dataItem.DisplayText}\n[{dataItem.GetLastValue()}]##StatPlot-{dataItem.Key}"
                    : $"##StatPlot-{dataItem.Key}";

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

            ImGui.End();
        }
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

    #endregion
}