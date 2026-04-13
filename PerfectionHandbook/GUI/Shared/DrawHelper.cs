using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.Integration;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public record SeasonSprite(string Name, SDUISprite Sprite);

public static class DrawHelper
{
    private static readonly Dictionary<long, RenderTarget2D> cachedMiniIconRT = [];
    private static Dictionary<Season, SeasonSprite>? seasonSprites = null;

    public static void DisposeCache()
    {
        foreach (RenderTarget2D renderTarget in cachedMiniIconRT.Values)
        {
            renderTarget.Dispose();
        }
        cachedMiniIconRT.Clear();
        seasonSprites = null;
    }

    public static RenderTarget2D? GetFarmerMiniIcon(Farmer? who)
    {
        if (MenuHandler.IsPreloading || who == null)
            return null;
        if (
            !cachedMiniIconRT.TryGetValue(who.UniqueMultiplayerID, out RenderTarget2D? renderTarget)
            || renderTarget.IsDisposed
        )
        {
            renderTarget = new(
                Game1.graphics.GraphicsDevice,
                48,
                48,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents
            );
            cachedMiniIconRT[who.UniqueMultiplayerID] = renderTarget;
        }

        RenderToTarget(
            renderTarget,
            (renderBatch) =>
                who.FarmerRenderer.drawMiniPortrat(renderBatch, Vector2.Zero, 1f, 3f, who.facingDirection.Value, who)
        );

        cachedMiniIconRT[who.UniqueMultiplayerID] = renderTarget;
        return renderTarget;
    }

    private static RenderTarget2D RenderToTarget(RenderTarget2D renderTarget, Action<SpriteBatch> drawCallback)
    {
        RenderTarget2D? wasRenderTarget;
        {
            RenderTargetBinding[] wasRenderTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
            wasRenderTarget = wasRenderTargets.Length > 0 ? wasRenderTargets[0].RenderTarget as RenderTarget2D : null;
        }

        Game1.SetRenderTarget(renderTarget);

        SpriteBatch? renderBatch = null;
        try
        {
            renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
            renderBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            drawCallback(renderBatch);
            renderBatch.End();
        }
        finally
        {
            Game1.SetRenderTarget(wasRenderTarget);
            renderBatch?.Dispose();
        }

        return renderTarget;
    }

    public static Texture2D SafeLoad(string? assetName, Texture2D? fallbackTx)
    {
        if (string.IsNullOrEmpty(assetName))
            return fallbackTx ?? Game1.mouseCursors;
        if (!Game1.content.DoesAssetExist<Texture2D>(assetName))
            return fallbackTx ?? Game1.mouseCursors;
        return Game1.content.Load<Texture2D>(assetName);
    }

    public static SeasonSprite GetSeasonSprite(Season season)
    {
        if ((seasonSprites ??= GetAllSeasonSprites()).TryGetValue(season, out SeasonSprite? sprite))
            return sprite;
        ModEntry.Log($"Unrecognized season: {season}");
        return seasonSprites[Season.Spring];
    }

    private static Dictionary<Season, SeasonSprite> GetAllSeasonSprites()
    {
        Dictionary<Season, SeasonSprite> sprites = [];
        sprites[Season.Spring] = new(
            Game1.content.LoadString("Strings/StringsFromCSFiles:spring"),
            new(Game1.mouseCursors, new(406, 441, 12, 8))
        );
        sprites[Season.Summer] = new(
            Game1.content.LoadString("Strings/StringsFromCSFiles:summer"),
            new(Game1.mouseCursors, new(406, 449, 12, 8))
        );
        sprites[Season.Fall] = new(
            Game1.content.LoadString("Strings/StringsFromCSFiles:fall"),
            new(Game1.mouseCursors, new(406, 457, 12, 8))
        );
        sprites[Season.Winter] = new(
            Game1.content.LoadString("Strings/StringsFromCSFiles:winter"),
            new(Game1.mouseCursors, new(406, 465, 12, 8))
        );
        return sprites;
    }
}
