using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PerfectionHandbook.Integration;

public interface ISpaceCoreApi
{
    /// <summary>
    /// Returns a list of all currently loaded skill's string IDs
    /// </summary>
    /// <returns>An array of skill IDs</returns>
    string[] GetCustomSkills();

    /// <summary>
    /// Gets the Base level of the custom skill for the farmer.
    /// </summary>
    /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
    /// <param name="skill"> The string ID of the skill you want the level of.</param>
    /// <returns>Int</returns>
    int GetLevelForCustomSkill(Farmer farmer, string skill);

    /// <summary>
    /// Gets the total exp the skill has
    /// </summary>
    /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
    /// <param name="skill"> The string ID of the skill you want the level of.</param>
    /// <returns>Int</returns>
    int GetExperienceForCustomSkill(Farmer farmer, string skill);

    /// <summary>
    /// Gets the display name of the skill
    /// </summary>
    /// <param name="skill"> The ID of the skill you want to get</param>
    /// <returns>Texture2D</returns>
    string GetDisplayNameOfCustomSkill(string skill);

    /// <summary>
    /// Gets the 16x16 icon of the skill that shows up in the level up menu.
    /// </summary>
    /// <param name="skill"> The ID of the skill you want to get</param>
    /// <returns>Texture2D</returns>
    Texture2D GetSkillIconForCustomSkill(string skill);
}
