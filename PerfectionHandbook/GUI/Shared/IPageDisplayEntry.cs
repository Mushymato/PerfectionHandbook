using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public interface IPageDisplayEntry
{
    public ItemInfo Info { get; }
    public bool Needed { get; }
    public void SetStatus(Farmer who);
}
