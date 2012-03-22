using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning.Acceptability
{
    public class DynamicAcceptability : IAcceptabilityFunction
    {
        Predicate<LinkedList<StateActionReward>> _fn;
        public DynamicAcceptability(Predicate<LinkedList<StateActionReward>> fn)
        {
            _fn = fn;
        }

        public bool Accept(LinkedList<StateActionReward> memory)
        {
            return _fn.Invoke(memory);
        }

        public void Reset()
        {
        }
    }
}
