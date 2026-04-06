using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public interface IPageDisplayEntry
{
    public bool Needed { get; }
    public void SetStatus(Farmer who);
    public bool SearchMatch(string txt);
}
