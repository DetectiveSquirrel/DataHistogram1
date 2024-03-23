using System.Collections.Generic;
using System.Linq;

namespace DataHistogram1.StatData;

public class StatDataParent(string parentName)
{
    public string ParentName { get; set; } = parentName;
    public List<StatDataItem> Items { get; set; } = [];

    public void AddItem(StatDataItem item)
    {
        Items.Add(item);
    }

    public void RemoveItem(string key)
    {
        var itemToRemove = Items.FirstOrDefault(item => item.Key == key);

        if (itemToRemove != null)
        {
            Items.Remove(itemToRemove);
        }
    }
}