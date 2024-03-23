using ExileCore;
using ImGuiNET;
using System.Numerics;

namespace DataHistogram1;

public class DataHistogram1 : BaseSettingsPlugin<DataHistogram1Settings>
{
    public static DataHistogram1 Main;
    private StatTracker statTracker;

    public override bool Initialise()
    {
        Main = this;
        statTracker = new StatTracker();
        statTracker.AddFilterKey("BaseColdDamageResistancePct", "Cold Res", new Vector2(500, 40));
        statTracker.AddFilterKey("BaseChaosDamageResistancePct", "Chaos Res", new Vector2(500, 40));
        statTracker.AddFilterKey("AttackSpeedPct", "Attack Speed", new Vector2(500, 70));
        statTracker.AddFilterKey("MainHandAccuracyRating", "Accuracy", new Vector2(500, 80));
        return true;
    }

    public override void AreaChange(AreaInstance area) { }

    public override Job Tick()
    {
        statTracker.Tick();
        return null;
    }

    public override void DrawSettings()
    {
        base.DrawSettings();
    }

    public static ImGuiWindowFlags GetWindowFlags()
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

    public override void Render()
    {
        ImGui.Begin("Tracked Stats Child", GetWindowFlags());

        // Draw all tracked stats from StatTracker
        statTracker.DisplayStatsOverTime();
        ImGui.EndChild();
    }
}