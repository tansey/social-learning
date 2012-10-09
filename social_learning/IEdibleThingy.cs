using System;


namespace social_learning
{
    public interface IEdibleThingy<PredatorType> : IWorldThingy
    {
        int Reward { get; set; }
        int Radius { get; set; }
        
        bool AvailableForEating(PredatorType pred);
        void EatenBy(PredatorType pred, int step);
        bool EatenByRecently(int curStep, int window, PredatorType pred);
        bool EatenRecently(int curStep, int window);
        int EaterCount { get; }
    }
}
