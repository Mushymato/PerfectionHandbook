using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public static class DrawHelper
{
    private static readonly Dictionary<long, RenderTarget2D> cachedRT = [];

    public static void DisposeCache()
    {
        foreach (RenderTarget2D renderTarget in cachedRT.Values)
        {
            renderTarget.Dispose();
        }
        cachedRT.Clear();
    }

    public static RenderTarget2D? GetFarmerMiniIcon(Farmer? who)
    {
        if (who == null)
            return null;
        if (!cachedRT.TryGetValue(who.UniqueMultiplayerID, out RenderTarget2D? renderTarget) || renderTarget.IsDisposed)
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
            cachedRT[who.UniqueMultiplayerID] = renderTarget;
        }

        RenderToTarget(
            renderTarget,
            (renderBatch) =>
                who.FarmerRenderer.drawMiniPortrat(renderBatch, Vector2.Zero, 1f, 3f, who.facingDirection.Value, who)
        );

        cachedRT[who.UniqueMultiplayerID] = renderTarget;
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
}
