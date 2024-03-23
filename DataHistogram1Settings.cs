using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace DataHistogram1
{
    public class DataHistogram1Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        public ToggleNode AlwaysAutoResize { get; set; } = new ToggleNode(false);
        public ToggleNode NoBackground { get; set; } = new ToggleNode(false);
        public ToggleNode NoMove { get; set; } = new ToggleNode(false);
        public ToggleNode NoTitleBar { get; set; } = new ToggleNode(false);
        public ToggleNode NoInputs { get; set; } = new ToggleNode(false);
    }
}