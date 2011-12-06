using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatchExperiment
{
    public static class ListExtensions
    {
        /// <summary>
        /// Calculates the sample standard deviation.
        /// </summary>
        public static double Stdev(this IEnumerable<double> data)
        {
            if (data.Count() < 2)
                return 0;

            double avg = data.Average();
            double numerator = 0;
            foreach (double d in data)
                numerator += (d - avg) * (d - avg);

            double variance = numerator / (double)(data.Count() - 1);

            return Math.Sqrt((double)variance);
        }
    }
}
