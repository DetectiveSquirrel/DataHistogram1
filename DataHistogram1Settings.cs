using DataHistogram1.StatData;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Collections.Generic;

namespace DataHistogram1
{
    public class DataHistogram1Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new(false);

        public ToggleNode AlwaysAutoResize { get; set; } = new(false);
        public ToggleNode NoBackground { get; set; } = new(false);
        public ToggleNode NoMove { get; set; } = new(false);
        public ToggleNode NoTitleBar { get; set; } = new(false);
        public ToggleNode NoInputs { get; set; } = new(false);

        [Menu("Update Frequency in milliseconds")]
        public RangeNode<int> UpdateFrequency { get; set; } = new(500, 1, 10000);

        [Menu("Max Data Age in seconds")]
        public RangeNode<int> MaxDataAge { get; set; } = new(120, 1, 3600);

        public List<StatDataParent> statDataParents = [];

    }
}