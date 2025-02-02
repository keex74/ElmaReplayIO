namespace ElmaReplayIO;
using System.Collections.ObjectModel;

/// <summary>
/// Defines a level polygon.
/// </summary>
/// <param name="index">The polygon index.</param>
/// <param name="points"></param>
/// <param name="isGrass"></param>
public class ElmaPolygon(int index, IList<Position<double>> points, bool isGrass)
{
    /// <summary>
    /// Gets the idnex of the polygon in the level.
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// Gets or sets the polygon's Z-order.
    /// </summary>
    public int ZOrder { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this is a grass polygon.
    /// </summary>
    public bool IsGrass { get; } = isGrass;

    /// <summary>
    /// Gets the polygon vertices.
    /// </summary>
    public ReadOnlyCollection<Position<double>> Vertices { get; } = new ReadOnlyCollection<Position<double>>(points);
}