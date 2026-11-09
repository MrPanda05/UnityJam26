using UnityEngine;

namespace JamUnity.Enemies.Brains
{
    public interface IBrain
    {
        //The actual logic of the brain lives here, can be called anywhere
        void Think();
        //Enables the thinking
        void StartThinking();
        //Disables thinking
        void StopThinking();
        //Is the brain activily thinking
        bool IsThinking { get; }
        //The max frame that should pass before the brain thinks again or use for things that has a lot of processing power.
        int MaxThinkingTime { get; }

    }
}
