using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public interface IWorldThingy
    {
        float X { get; set; }
        float Y { get; set; }
        void Reset();
    }
}
