using UnityEngine;

namespace JamUnity.Commons
{
    /// <summary>
    /// Used
    /// </summary>
    public interface IDiffRule
    {
        /// <summary>
        /// Used to modify the game based on current diff of game
        /// </summary>
        void HandleDiff();
    }
}
