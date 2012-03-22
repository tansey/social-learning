using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using social_learning;

namespace TestWorld
{
    public class ActionListAgent : Agent
    {
        List<float[]> _actions;

        public List<float[]> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }
        int _nextAction;
        public ActionListAgent(int id, List<float[]> actions)
            : base(id)
        {
            _actions = actions;
        }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            return _actions[_nextAction++];
        }

        public override void Reset()
        {
        }
        protected override void ProcessReward(double r)
        {
        }
    }
}
