namespace PerfectionHandbook.Integration;

public interface IVanillaPlusProfessions
{
    /// <summary>
    /// Exposes XP limits for VPP's new levels. Index 0 is total experience required for level 11.
    /// </summary>
    public int[] LevelExperiences { get; }
}
