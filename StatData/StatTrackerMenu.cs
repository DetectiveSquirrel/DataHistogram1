using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace DataHistogram1.StatData;

public class StatTrackerMenu(StatTracker tracker)
{
    public Dictionary<int, string> newNameCache = [];

    private static void SwapItems<T>(IList<T> list, int index1, int index2)
    {
        // This swaps the items in place and doesn't need to return anything
        (list[index1], list[index2]) = (list[index2], list[index1]);
    }

    public void Render()
    {
        var itemsToRemove = new List<string>();
        var parentsToRemove = new List<string>();
        var currentItems = tracker.statDataParents;
        var defaultStyle = GetDefaultStyling();

        for (var parentIndex = 0; parentIndex < currentItems.Count; parentIndex++)
        {
            var parentItem = currentItems[parentIndex];

            if (!ImGui.CollapsingHeader($"{parentItem.ParentName}##{parentIndex}"))
            {
                continue;
            }

            ImGui.BeginChild($"##{parentIndex}", Vector2.Zero, ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
            ImGui.Indent();

            if (ImGui.Button($"[X] Remove This Parent##{parentIndex}"))
            {
                parentsToRemove.Add(parentItem.ParentName);
                ImGui.EndChild();
                continue;
            }

            var availableWidth = ImGui.GetContentRegionAvail().X * 0.75f;
            ImGui.SetNextItemWidth(availableWidth);

            var newName = newNameCache.TryGetValue(parentIndex, out var cachedName) ? cachedName
                : parentItem.ParentName;

            if (ImGui.InputTextWithHint($"##{parentIndex}", "Please choose a display name", ref newName, 2048))
            {
                newNameCache[parentIndex] = newName;
            }

            ImGui.SameLine();

            if (ImGui.Button($"Apply Rename##{parentIndex}"))
            {
                if (newNameCache.TryGetValue(parentIndex, out newName))
                {
                    parentItem.ParentName = newName;
                    newNameCache.Remove(parentIndex);
                }
            }

            for (var statIndex = 0; statIndex < parentItem.Items.Count; statIndex++)
            {
                var statItem = parentItem.Items[statIndex];

                ImGui.BeginChild(
                    $"##{parentIndex}_{statIndex}",
                    Vector2.Zero,
                    ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY
                );

                if (ImGui.ArrowButton($"UP_{parentIndex}_{statIndex}", ImGuiDir.Up))
                {
                    if (statIndex > 0)
                    {
                        SwapItems(parentItem.Items, statIndex, statIndex - 1);
                    }
                }

                availableWidth = ImGui.GetContentRegionAvail().X * 0.75f;
                ImGui.Indent();
                var refKey = statItem.Key;
                ImGui.SetNextItemWidth(availableWidth);

                if (ImGui.InputTextWithHint(
                        $"Stat Key##{parentIndex}_{statIndex}",
                        "Please input a GameStat Enum",
                        ref refKey,
                        2048
                    ))
                {
                    statItem.Key = refKey;
                }

                var refKeyDisplay = statItem.DisplayText;
                ImGui.SetNextItemWidth(availableWidth);

                if (ImGui.InputTextWithHint(
                        $"Stat Display Name##{parentIndex}_{statIndex}",
                        "Please chose a display name",
                        ref refKeyDisplay,
                        2048
                    ))
                {
                    statItem.DisplayText = refKeyDisplay;
                }

                var refDisplayWidth = statItem.DisplaySize;
                ImGui.SetNextItemWidth(availableWidth);

                if (ImGui.SliderFloat2($"Width | Height##{parentIndex}_{statIndex}", ref refDisplayWidth, 0, 1800))
                {
                    statItem.DisplaySize = refDisplayWidth;
                }

                var refIsCustom = statItem.IsCustom;

                if (ImGui.Checkbox($"Is Custom Flag##{parentIndex}_{statIndex}", ref refIsCustom))
                {
                    statItem.IsCustom = refIsCustom;
                }

                var refShouldUpdate = statItem.ShouldUpdate;

                if (ImGui.Checkbox($"Should Update Flag##{parentIndex}_{statIndex}", ref refShouldUpdate))
                {
                    statItem.ShouldUpdate = refShouldUpdate;
                }

                var refShouldDisplay = statItem.ShouldDisplay;

                if (ImGui.Checkbox($"Should Display Flag##{parentIndex}_{statIndex}", ref refShouldDisplay))
                {
                    statItem.ShouldDisplay = refShouldDisplay;
                }

                var refPlotLineDisplayText = statItem.PlotLineDisplayText;

                if (ImGui.Checkbox($"Show Display Text##{parentIndex}_{statIndex}", ref refPlotLineDisplayText))
                {
                    statItem.PlotLineDisplayText = refPlotLineDisplayText;
                }

                var refPlotLineMinMaxText = statItem.PlotLineMinMaxText;

                if (ImGui.Checkbox($"Show Min-Max PlotLine Text##{parentIndex}_{statIndex}", ref refPlotLineMinMaxText))
                {
                    statItem.PlotLineMinMaxText = refPlotLineMinMaxText;
                }

                var refCustomStyling = statItem.CustomStyling;

                if (ImGui.Checkbox($"use Custom Styling##{parentIndex}_{statIndex}", ref refCustomStyling))
                {
                    statItem.CustomStyling = refCustomStyling;
                }

                ImGui.Indent();

                if (ImGui.Button($"Apply Default Colors##{parentIndex}_{statIndex}"))
                {
                    statItem.PlotLineColor = defaultStyle.PlotLineColor;
                    statItem.PlotBackgroundColor = defaultStyle.PlotBackgroundColor;
                    statItem.TextColor = defaultStyle.DisplayTextColor;
                }

                var refPlotLineColor = statItem.PlotLineColor.ToImguiVec4();

                if (ImGui.ColorEdit4(
                        $"Plot Line Color##{parentIndex}_{statIndex}",
                        ref refPlotLineColor,
                        ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs |
                        ImGuiColorEditFlags.AlphaPreviewHalf
                    ))
                {
                    statItem.PlotLineColor = refPlotLineColor.ToSharpColor();
                }

                var refPlotBackgroundColor = statItem.PlotBackgroundColor.ToImguiVec4();

                if (ImGui.ColorEdit4(
                        $"Plot Background Color##{parentIndex}_{statIndex}",
                        ref refPlotBackgroundColor,
                        ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs |
                        ImGuiColorEditFlags.AlphaPreviewHalf
                    ))
                {
                    statItem.PlotBackgroundColor = refPlotBackgroundColor.ToSharpColor();
                }

                var refDisplayTextColor = statItem.TextColor.ToImguiVec4();

                if (ImGui.ColorEdit4(
                        $"Display Text Color##{parentIndex}_{statIndex}",
                        ref refDisplayTextColor,
                        ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs |
                        ImGuiColorEditFlags.AlphaPreviewHalf
                    ))
                {
                    statItem.TextColor = refDisplayTextColor.ToSharpColor();
                }

                ImGui.Unindent();
                ImGui.Unindent();

                if (ImGui.ArrowButton($"DOWN_{parentIndex}_{statIndex}", ImGuiDir.Down))
                {
                    if (statIndex < parentItem.Items.Count - 1)
                    {
                        SwapItems(parentItem.Items, statIndex, statIndex + 1);
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button($"[-] Remove This Stat##{parentIndex}_{statIndex}"))
                {
                    itemsToRemove.Add(statItem.Key);
                }

                ImGui.EndChild();
            }

            if (ImGui.Button($"[^] Insert New Stat Above##{parentIndex}"))
            {
                parentItem.AddItem(new StatDataItem("NewItem", "DisplayText", new Vector2(500, 50), false));
            }

            ImGui.Unindent();
            ImGui.EndChild();
            ImGui.Spacing();
        }

        if (ImGui.Button("[=] Add New Parent"))
        {
            tracker.AddFilterParent($"Custom Module [{currentItems.Count + 1}]");
        }

        foreach (var itemName in itemsToRemove)
        foreach (var parent in currentItems)
            parent.RemoveItem(itemName);

        currentItems.RemoveAll(parent => parentsToRemove.Contains(parent.ParentName));
        tracker.statDataParents = currentItems;
    }

    private static Styling GetDefaultStyling()
    {
        var defaultStyling = new Styling
        {
            PlotLineColor = ImGui.GetStyle().Colors[(int)ImGuiCol.PlotLines].ToSharpColor(),
            PlotBackgroundColor = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg].ToSharpColor(),
            DisplayTextColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text].ToSharpColor()
        };

        return defaultStyling;
    }

    private class Styling
    {
        public Color PlotLineColor { get; set; }
        public Color PlotBackgroundColor { get; set; }
        public Color DisplayTextColor { get; set; }
    }
}