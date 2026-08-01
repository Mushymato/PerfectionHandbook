using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public record SeasonSprite(string Name, SDUISprite Sprite);

public static class DrawHelper
{
    private static readonly List<(long, RenderTarget2D)> cachedMiniIconRT = [];
    private static IReadOnlyList<SeasonSprite>? seasonSprites = null;
    private static int[] playerPanelFrames = [0, 1, 0, 2];

    public static void DisposeCache()
    {
        foreach ((_, RenderTarget2D renderTarget) in cachedMiniIconRT)
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
            cachedMiniIconRT.FirstOrDefault(thing => thing.Item1 == who.UniqueMultiplayerID)
                is not
                (_, RenderTarget2D renderTarget)
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
            cachedMiniIconRT.Add((who.UniqueMultiplayerID, renderTarget));
        }

        RenderToTarget(
            renderTarget,
            (renderBatch) =>
                who.FarmerRenderer.drawMiniPortrat(renderBatch, Vector2.Zero, 1f, 3f, who.facingDirection.Value, who)
        );

        cachedMiniIconRT.Add((who.UniqueMultiplayerID, renderTarget));
        return renderTarget;
    }

    public static RenderTarget2D GetEntireFarmer(Farmer who)
    {
        RenderTarget2D? farmerRT = new(
            Game1.graphics.GraphicsDevice,
            Game1.daybg.Width,
            Game1.daybg.Height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents
        );
        RenderToTarget(
            farmerRT,
            (renderBatch) =>
            {
                renderBatch.Draw(Game1.daybg, Vector2.Zero, Color.White);
                FarmerRenderer.isDrawingForUI = true;
                who.FarmerRenderer.draw(
                    renderBatch,
                    new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false),
                    0,
                    new Rectangle(0, 0, 16, 32),
                    new Vector2(32, 32),
                    Vector2.Zero,
                    0.8f,
                    2,
                    Color.White,
                    0f,
                    1f,
                    who
                );
                FarmerRenderer.isDrawingForUI = false;
            }
        );
        return farmerRT;
    }

    public static RenderTarget2D RenderDrawableToTarget(IViewDrawable drawable)
    {
        drawable.DoUpdate(Game1.currentGameTime.ElapsedGameTime);
        int actualWidth = (int)drawable.ActualSize.X;
        int actualHeight = (int)drawable.ActualSize.Y;
        RenderTarget2D? exportRT = new(
            Game1.graphics.GraphicsDevice,
            actualWidth,
            actualHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents
        );
        RenderToTarget(exportRT, (renderBatch) => drawable.Draw(renderBatch, Vector2.Zero));
        return exportRT;
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

    public static Texture2D SafeLoad(string? assetName, Texture2D? fallbackTx = null)
    {
        if (string.IsNullOrEmpty(assetName))
            return fallbackTx ?? Game1.mouseCursors;
        if (!Game1.content.DoesAssetExist<Texture2D>(assetName))
            return fallbackTx ?? Game1.mouseCursors;
        return Game1.content.Load<Texture2D>(assetName);
    }

    public static SeasonSprite GetSeasonSprite(Season season)
    {
        seasonSprites ??= GetAllSeasonSprites();
        if (seasonSprites.Count > (int)season)
            return seasonSprites[(int)season];
        ModEntry.Log($"Unrecognized season: {season}", LogLevel.Error);
        return seasonSprites[0];
    }

    private static IReadOnlyList<SeasonSprite> GetAllSeasonSprites()
    {
        return
        [
            new(
                Game1.content.LoadString("Strings/StringsFromCSFiles:spring"),
                new(Game1.mouseCursors, new(406, 441, 12, 8))
            ),
            new(
                Game1.content.LoadString("Strings/StringsFromCSFiles:summer"),
                new(Game1.mouseCursors, new(406, 441, 12, 8))
            ),
            new(
                Game1.content.LoadString("Strings/StringsFromCSFiles:spring"),
                new(Game1.mouseCursors, new(406, 441, 12, 8))
            ),
            new(
                Game1.content.LoadString("Strings/StringsFromCSFiles:winter"),
                new(Game1.mouseCursors, new(406, 441, 12, 8))
            ),
        ];
    }

    public static SDUISprite? GetQualityStar(int quality)
    {
        return quality switch
        {
            1 => new(Game1.mouseCursors, new Rectangle(338, 400, 8, 8)),
            2 => new(Game1.mouseCursors, new Rectangle(346, 400, 8, 8)),
            4 => new(Game1.mouseCursors, new Rectangle(346, 392, 8, 8)),
            _ => null,
        };
    }
}
