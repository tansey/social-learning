using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class SensorDictionary
    {
        Dictionary<int, int[]> _dictionary;
        int _agentHorizon;
        int _height;
        int _width;
        int[] _defaultDistanceAndOrientation;

        public SensorDictionary(int agentHorizon, int height, int width)
        {
            _dictionary = new Dictionary<int, int[]>();
            _agentHorizon = agentHorizon;
            _height = height;
            _width = width;
            _defaultDistanceAndOrientation = new int[] { _agentHorizon + 10, -1 };
            for (int i = -1 * agentHorizon; i < agentHorizon; i++)
                for (int j = -1 * agentHorizon; j < agentHorizon; j++)
                    for (int k = -1; k < 2; k++)
                        for (int l = -1; l < 2; l++)
                        {
                            int[] p = calculateDistanceAndOrientation(i, j);
                            _dictionary[(i + k * height) % height * agentHorizon * 100 + ((j + l * width) % width)] = p;
                        }
        }

        public int[] getDistanceAndOrientation(int agentX, int agentY, int objectX, int objectY)
        {
            int key = (objectX - agentX) * _agentHorizon * 100 + (objectY - agentY);
            int[] p;
            if (!_dictionary.TryGetValue(key, out p))
                return _defaultDistanceAndOrientation;
            return p;
        }

        private int[] calculateDistanceAndOrientation(int x, int y)
        {
            int dist = (int)Math.Sqrt(x * x + y * y);
            int pos = (int)(Math.Atan(y / (float)x) * 180.0 / Math.PI + 360);
            if (x < 0)
                pos += 180;
            pos %= 360;
            return new int[] { dist, pos };
        }
    }
}
