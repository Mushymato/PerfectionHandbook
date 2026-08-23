using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Reminders;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed record BuildingsConstructedDisplay(string Id, BuildingData Data, int Count) : IPageDisplayEntry
{
    public override int GetHashCode() => Id.GetHashCode();

    public readonly string DisplayName = TokenParser.ParseText(Data.Name) ?? Id;
    public readonly SDUISprite Sprite = GetBuildingSprite(Data);

    public readonly Color DisplayTint = Count == 0 ? HandbookContext.InactiveColor : HandbookContext.ActiveColor;
    public bool Needed { get; } = true;
    public ReminderEntry? Reminder { get; } =
        MenuHandler.Reminders.GetOrCreateEntry(ReminderEntryFactory.Kind_BuildingsConstructed, Id);
    public bool HasShadow => Data.DrawShadow;
    public string ShadowLayout => $"{48 * Data.Size.X}px 48px";

    public bool SearchMatch(string txt)
    {
        return DisplayName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who) { }

    private static SDUISprite GetBuildingSprite(BuildingData Data)
    {
        Texture2D buildingTx = DrawHelper.SafeLoad(Data.Texture, Game1.content.Load<Texture2D>("Buildings/Error"));
        return new(
            buildingTx,
            Data.SourceRect.IsEmpty ? buildingTx.Bounds : Data.SourceRect,
            FixedEdges: new(0),
            SliceSettings: new(Scale: 3)
        );
    }

    public void ToggleReminder() => MenuHandler.Reminders.ToggleEntryKeyChecked(Reminder);
}

public sealed class GoalBuildingsConstructedContext(IGoalContext goalCtx)
    : AbstractPageListContext<BuildingsConstructedDisplay>(goalCtx, canToggleNeeded: false, canPaginate: false)
{
    private static readonly string[] ObelisksAndClock =
    [
        "Water Obelisk",
        "Earth Obelisk",
        "Desert Obelisk",
        "Island Obelisk",
        "Gold Clock",
    ];

    protected override IReadOnlyList<BuildingsConstructedDisplay> MakeAllDisplay()
    {
        List<BuildingsConstructedDisplay> builtDisplay = [];
        foreach (string buildingId in ObelisksAndClock)
        {
            if (!Game1.buildingData.TryGetValue(buildingId, out BuildingData? buildingData))
            {
                continue;
            }
            int builtCount = Game1.GetNumberBuildingsConstructed(buildingId);
            builtDisplay.Add(new(buildingId, buildingData, builtCount));
        }
        // foreach ((string buildingId, BuildingData buildingData) in Game1.buildingData)
        // {
        //     // look for an ObeliskWarp for modded obelisks
        //     if (ObelisksAndClock.Contains(buildingId))
        //         continue;
        //     if (!(buildingData.DefaultAction?.StartsWith("ObeliskWarp") ?? false))
        //         continue;
        //     int builtCount = Game1.GetNumberBuildingsConstructed(buildingId);
        //     builtDisplay.Add(new(buildingId, buildingData, builtCount));
        // }
        // other obelisks
        return builtDisplay;
    }
}
