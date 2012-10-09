using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public abstract class WorldThingy : IWorldThingy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public abstract void Reset();
    }
}
