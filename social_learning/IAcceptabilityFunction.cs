using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public interface IAcceptabilityFunction
    {
        bool Accept(LinkedList<StateActionReward> memory);
        void Reset();
    }
}
