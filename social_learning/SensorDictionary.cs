using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class WorldPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int[] distanceAndPosition { get; set; }
        //get sensor index (assuming agent is at this point)

        public WorldPoint(int x, int y)
        {
            //X = x;
            //Y = y;
            int dist = (int) Math.Sqrt(x * x + y * y);
            int pos = (int)( Math.Atan((float)y /x) * 180.0 / Math.PI + 360);
            if (x < 0)
                pos += 180;
            pos %= 360;
            distanceAndPosition = new int[] { dist, pos };
        }


    }

    public class SensorDictionary
    {

        Dictionary<int, WorldPoint> dictionary {get; set;}
        int _agentHorizon { get; set; }
        int _height { get; set; }
        int _width { get; set; }
        public SensorDictionary(int agentHorizon, int height, int width)
        {
            dictionary = new Dictionary<int, WorldPoint>();
            _agentHorizon = agentHorizon;
            _height = height;
            _width = width;
            for (int i = -1 * agentHorizon; i < agentHorizon; i++)
                for (int j = -1 * agentHorizon; j < agentHorizon; j++)
                    for (int k = -1; k < 2; k++)
                        for (int l = -1; l < 2; l++)
                        {
                            WorldPoint p = new WorldPoint(i, j);
                            dictionary[(i + k * height) % height * agentHorizon * 100 + ((j + l * width) % width)] = p;
                        }
        }

        public int[] getDistanceAndOrientation(int agentX, int agentY, int plantX, int plantY)
        {
            if (! dictionary.ContainsKey((plantX - agentX) * _agentHorizon * 100 + (plantY - agentY)))
                return new int[] {_agentHorizon + 10, -1};
            WorldPoint p = dictionary[(plantX - agentX) * _agentHorizon * 100 + (plantY - agentY)];
            //Console.WriteLine("Point ({0}, {1}), Dist {2} Pos {3}", p.X, p.Y, p.distanceAndPosition[0], p.distanceAndPosition[1]);
            return p.distanceAndPosition;
        }
    }
}
