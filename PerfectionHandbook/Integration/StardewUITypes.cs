/*
This file contains classes used for duck typing to/from StardewUI


MIT License

Copyright (c) 2024 focustense

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PerfectionHandbook.Integration;

[AttributeUsage(AttributeTargets.Class)]
public class DuckTypeAttribute : Attribute { }

[DuckType]
public record SDUITooltipData(
    string Text,
    string? Title = null,
    Item? Item = null,
    int? CurrencyAmount = null,
    int CurrencySymbol = 0,
    string? RequiredItemId = null,
    int RequiredItemAmount = 0,
    CraftingRecipe? CraftingRecipe = null,
    IList<Item>? AdditionalCraftingMaterials = null
);

[DuckType]
/// <summary>Duck types for StardewUI</summary>
public record SDUIEdges(int Left, int Top, int Right, int Bottom)
{
    public static readonly SDUIEdges NONE = new(0, 0, 0, 0);

    public SDUIEdges(int all)
        : this(all, all, all, all) { }

    public SDUIEdges(int horizontal, int vertical)
        : this(horizontal, vertical, horizontal, vertical) { }

    public static SDUIEdges operator *(float mult, SDUIEdges edges) =>
        new((int)(edges.Left * mult), (int)(edges.Top * mult), (int)(edges.Right * mult), (int)(edges.Bottom * mult));

    public static SDUIEdges operator *(SDUIEdges edges, float mult) => mult * edges;
}

public enum SDUISliceCenterPosition
{
    Start,
    End,
}

[DuckType]
public record SDUISliceSettings(
    int? CenterX = null,
    SDUISliceCenterPosition CenterXPosition = SDUISliceCenterPosition.Start,
    int? CenterY = null,
    SDUISliceCenterPosition CenterYPosition = SDUISliceCenterPosition.Start,
    float Scale = 4,
    bool EdgesOnly = false
);

[DuckType]
public record SDUISprite(
    Texture2D Texture,
    Rectangle? SourceRect = null,
    SDUIEdges? FixedEdges = null,
    SDUISliceSettings? SliceSettings = null
)
{
    public SDUISprite(Texture2D Texture)
        : this(Texture, Texture.Bounds, SDUIEdges.NONE, new()) { }

    public SDUISprite(Texture2D Texture, Rectangle SourceRect)
        : this(Texture, SourceRect, SDUIEdges.NONE, new()) { }
};

/// <summary>
/// Cardinal directions used in UI, matching gamepad stick/button directions for navigation.
/// </summary>
public enum SDUIDirection
{
    /// <summary>
    /// "Up" in screen space.
    /// </summary>
    North = 0,

    /// <summary>
    /// "Right" in screen space.
    /// </summary>
    East,

    /// <summary>
    /// "Down" in screen space.
    /// </summary>
    South,

    /// <summary>
    /// "Left" in screen space.
    /// </summary>
    West,
}

/// <summary>
/// Specifies an alignment (horizontal or vertical) for text or other layout.
/// </summary>
public enum SDUIAlignment
{
    /// <summary>
    /// Align to the start of the available space - horizontal left or vertical top.
    /// </summary>
    Start,

    /// <summary>
    /// Align to the middle of the available space.
    /// </summary>
    Middle,

    /// <summary>
    /// Align to the end of the available space - horizontal right or vertical bottom.
    /// </summary>
    End,
}

/// <summary>
/// Model for content placement along a nine-segment grid, i.e. all possible combinations of horizontal and vertical
/// <see cref="SDUIAlignment"/>.
/// </summary>
/// <remarks>
/// Copied from StardewUI and used to align HUD.
/// </remarks>
/// <param name="HorizontalAlignment">Content alignment along the horizontal axis.</param>
/// <param name="VerticalAlignment">Content alignment along the vertical axis.</param>
/// <param name="Offset">Absolute axis-independent pixel offset.</param>
[DuckType]
public record SDUINineGridPlacement(SDUIAlignment HorizontalAlignment, SDUIAlignment VerticalAlignment, Point Offset)
{
    private readonly Vector2 offsetVec2 = Offset.ToVector2();

    public Vector2 GetViewportPosition(Vector2 contentSize)
    {
        Viewport deviceViewport = Game1.graphics.GraphicsDevice.Viewport;
        xTile.Dimensions.Rectangle uiViewport = Game1.uiViewport;
        return GetPositionComponent(
                new(
                    Math.Min(deviceViewport.Width, uiViewport.Width),
                    Math.Min(deviceViewport.Height, uiViewport.Height)
                )
            )
            - GetPositionComponent(contentSize)
            + offsetVec2;
    }

    private Vector2 GetPositionComponent(Vector2 size)
    {
        return new(
            HorizontalAlignment switch
            {
                SDUIAlignment.Middle => size.X / 2,
                SDUIAlignment.End => size.X,
                _ => 0,
            },
            VerticalAlignment switch
            {
                SDUIAlignment.Middle => size.Y / 2,
                SDUIAlignment.End => size.Y,
                _ => 0,
            }
        );
    }
};
