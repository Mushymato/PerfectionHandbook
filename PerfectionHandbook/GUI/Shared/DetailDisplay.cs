using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.GUI.Shared;

public sealed record CropDetailDisplay(ItemInfo Info)
{
    public sealed record CropDay(SDUISprite Sprite, bool IsHarvest);

    public IReadOnlyList<CropDay> HarvestCells
    {
        get
        {
            CropData crop = Info.FromCrop[0];
            Texture2D cropTx = DrawHelper.SafeLoad(crop.Texture, Game1.cropSpriteSheet);
            List<CropDay> harvestCells = [];
            int growDays = crop.DaysInPhase.Sum();
            int regrowDays = crop.RegrowDays;
            int phase = -1;
            UpdatePhase(crop, 0, ref phase, out int nextPhaseDay, out int phaseIndex);
            if (regrowDays < 1)
            {
                // non-regrowing
                harvestCells.Add(GetPhaseSprite(cropTx, crop.SpriteIndex, phaseIndex));
                for (int day = 1; day < WorldDate.DaysPerMonth; day++)
                {
                    if (day >= nextPhaseDay)
                        UpdatePhase(crop, day, ref phase, out nextPhaseDay, out phaseIndex);
                    harvestCells.Add(
                        day % growDays == 0
                            ? GetHarvestSprite(Info.Datum)
                            : GetPhaseSprite(cropTx, crop.SpriteIndex, phaseIndex)
                    );
                }
            }
            else
            {
                // regrowing
                harvestCells.Add(GetPhaseSprite(cropTx, crop.SpriteIndex, phaseIndex));
                for (int day = 1; day < growDays; day++)
                {
                    if (day >= nextPhaseDay)
                        UpdatePhase(crop, day, ref phase, out nextPhaseDay, out phaseIndex);
                    harvestCells.Add(GetPhaseSprite(cropTx, crop.SpriteIndex, phaseIndex));
                }
                harvestCells.Add(GetHarvestSprite(Info.Datum));
                for (int day = 1; day < WorldDate.DaysPerMonth - growDays; day++)
                {
                    harvestCells.Add(
                        day % regrowDays == 0
                            ? GetHarvestSprite(Info.Datum)
                            : GetPhaseSprite(cropTx, crop.SpriteIndex, phaseIndex)
                    );
                }
            }

            return harvestCells;
        }
    }

    private static void UpdatePhase(CropData crop, int day, ref int phase, out int nextPhaseDay, out int phaseIndex)
    {
        phase++;
        if (phase >= crop.DaysInPhase.Count)
            phase = 0;
        phaseIndex = phase == 0 ? Random.Shared.Next(0, 2) : 1 + phase;
        nextPhaseDay = day + crop.DaysInPhase[phase];
    }

    private static CropDay GetPhaseSprite(Texture2D cropTx, int spriteIndex, int phaseIndex)
    {
        return new(
            new(
                cropTx,
                new(spriteIndex % 2 * 128 + phaseIndex * 16, spriteIndex / 2 * 32, 16, 32),
                FixedEdges: SDUIEdges.NONE,
                SliceSettings: new(Scale: 3)
            ),
            false
        );
    }

    private static CropDay GetHarvestSprite(ParsedItemData datum)
    {
        return new(
            new(datum.GetTexture(), datum.GetSourceRect(), FixedEdges: SDUIEdges.NONE, SliceSettings: new(Scale: 3)),
            true
        );
    }
}
