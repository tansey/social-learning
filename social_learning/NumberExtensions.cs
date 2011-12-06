using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public static class NumberExtensions
    {
        public static double ToRadians(this double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double ToDegrees(this double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
