using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed record BuildingMaterialDisplay(ItemInfo Info, int NeededCount, int OwnedCount)
{
    public SDUITooltipData Tooltip
    {
        get { return new SDUITooltipData(DisplayText, Info.Datum.DisplayName, Item: Info.ReprItem); }
    }
    public readonly string DisplayText = I18n.Ui_Fulfillment_Dipslay(OwnedCount, NeededCount);
}

public sealed record BuildingsConstructedDisplay(
    string Id,
    BuildingData Data,
    int Built,
    List<BuildingMaterialDisplay> Materials
) : IPageDisplayEntry
{
    public override int GetHashCode() => Id.GetHashCode();

    public readonly string DisplayName = TokenParser.ParseText(Data.Name) ?? Id;
    public readonly SDUISprite Sprite = GetBuildingSprite(Data);

    public readonly Color DisplayTint = Built == 0 ? HandbookContext.InactiveColor * 0.5f : HandbookContext.ActiveColor;
    public bool Needed { get; } = Built == 0;
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

    internal static BuildingsConstructedDisplay Make(string id, BuildingData data, int built, PlayerOwned playerOwned)
    {
        List<BuildingMaterialDisplay> materials = [];
        if (data.BuildCost > 0)
        {
            if (ItemInfoCache.Cache.TryGetValue("(O)GoldCoin", out ItemInfo? info))
            {
                materials.Add(new(info, data.BuildCost, Game1.player.Money));
            }
        }
        if (data.BuildMaterials != null)
        {
            foreach (BuildingMaterial material in data.BuildMaterials)
            {
                if (
                    ItemRegistry.QualifyItemId(material.ItemId) is not string qId
                    || !ItemInfoCache.Cache.TryGetValue(material.ItemId, out ItemInfo? info)
                )
                {
                    continue;
                }
                int ownedCount = 0;
                if (playerOwned.OwnedGroups.TryGetValue(qId, out OwnedItemGroup? group))
                    ownedCount = group.CountRepr.ReprStack;
                materials.Add(new(info, material.Amount, ownedCount));
            }
        }
        return new(id, data, built, materials);
    }
}

public sealed class GoalBuildingsConstructedContext(IGoalContext goalCtx)
    : AbstractPageListContext<BuildingsConstructedDisplay>(
        goalCtx,
        canToggleNeeded: true,
        canToggleCountMode: false,
        canPaginate: false
    )
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
            builtDisplay.Add(BuildingsConstructedDisplay.Make(buildingId, buildingData, builtCount, GoalCtx.OwnedInfo));
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
