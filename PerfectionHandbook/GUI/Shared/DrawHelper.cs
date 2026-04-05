using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public static class DrawHelper
{
    private static readonly Dictionary<long, RenderTarget2D> cachedIcons = [];

    public static void DisposeCache()
    {
        foreach (RenderTarget2D renderTarget in cachedIcons.Values)
        {
            renderTarget.Dispose();
        }
        cachedIcons.Clear();
    }

    public static void PreloadCache()
    {
        foreach (Farmer who in Game1.getAllFarmers())
        {
            GetFarmerMiniIcon(who);
        }
    }

    public static RenderTarget2D? GetFarmerMiniIcon(Farmer? who)
    {
        if (who == null)
            return null;
        if (
            cachedIcons.TryGetValue(who.UniqueMultiplayerID, out RenderTarget2D? renderTarget)
            && !renderTarget.IsDisposed
        )
            return renderTarget;

        ModEntry.Log($"Render farmer mini-icon for {who.displayName}({who.UniqueMultiplayerID})");

        RenderTarget2D? wasRenderTarget;
        {
            RenderTargetBinding[] wasRenderTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
            wasRenderTarget = wasRenderTargets.Length > 0 ? wasRenderTargets[0].RenderTarget as RenderTarget2D : null;
        }

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
        Game1.SetRenderTarget(renderTarget);

        SpriteBatch? renderBatch = null;
        try
        {
            renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
            renderBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            who.FarmerRenderer.drawMiniPortrat(renderBatch, Vector2.Zero, 1f, 3f, who.facingDirection.Value, who);
            renderBatch.End();
        }
        finally
        {
            Game1.SetRenderTarget(wasRenderTarget);
            renderBatch?.Dispose();
        }

        cachedIcons[who.UniqueMultiplayerID] = renderTarget;
        return renderTarget;
    }
}
