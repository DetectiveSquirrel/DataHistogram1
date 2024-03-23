using DataHistogram1.StatData;
using ExileCore;

namespace DataHistogram1;

public class DataHistogram1 : BaseSettingsPlugin<DataHistogram1Settings>
{
    public static DataHistogram1 Main;
    private StatTrackerMenu StatMenu;
    private StatTracker statTracker;

    public override bool Initialise()
    {
        Main = this;
        statTracker = new StatTracker();
        StatMenu = new StatTrackerMenu(statTracker);
        return true;
    }

    public override Job Tick()
    {
        statTracker.Tick();
        return null;
    }

    public override void DrawSettings()
    {
        base.DrawSettings();
        StatMenu.Render();
    }

    public override void Render()
    {
        // Draw all tracked stats from StatTracker
        statTracker.DisplayStatsOverTime();
    }
}