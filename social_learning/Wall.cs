using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class Wall
    {
        private readonly int _id;
        public float X1 { get; set; }
        public float Y1 { get; set; }
	    public float X2 { get; set; }
	    public float Y2 { get; set; }
        public int Id { get { return _id; } }
        public float collisionNum { get; set; }
        public float prevCollisionNum { get; set; }
        public float intersect { get; set; } 

        public Wall(int id)
        {
            _id = id;
        }

	public float[] getFormula(float X1, float Y1, float X2, float Y2){
        float slope = 0f;
        float b = 0f;
        
        slope = (Y2-Y1)/(X2-X1);
		b = Y2-slope*X2;

        return new float[] { slope, b };
	}

    /**
        * Check whether an agent collided with a wall.
        **/
	public bool checkCollision(float agentX, float agentY, float agentPrevX, float agentPrevY){
        // X = (b2 - b) / (m - m2)
        float Xintersect = getXYinteresect(agentX, agentY, agentPrevX, agentPrevY)[0];
        if (Xintersect == -1)
            return true;
        float tempWallX1 = Math.Min(this.X1, this.X2);
        float tempWallX2 = Math.Max(this.X1, this.X2);

        float tempAgentPrevX = Math.Min(agentPrevX, agentX);
        float tempAgentX = Math.Max(agentPrevX, agentX);
        
        //debug
        intersect = Xintersect;
        if ((Xintersect >= tempWallX1 && Xintersect <= tempWallX2) 
            && (Xintersect >= tempAgentPrevX && Xintersect <= tempAgentX)) 
        {
            return true;
        }

        return false;
        
	}
    public float[] getXYinteresect(float _X1, float _Y1, float _X2, float _Y2)
    {   
        //slope and b for a wall
        float[] wallLine = new float[2];
        wallLine = getFormula(this.X1, this.Y1, this.X2, this.Y2);

        //slope and b for a agent
        float[] newLine = new float[2];

        float Yintersect = 0f;

        //agent moving up or down
        if (_X1 == _X2)
        {
            //Yintersect = wallLine[0] * _X1 + wallLine[1];
            //return new float[] { _X1, Yintersect };
            return new float[] { -1, -1 };
        }
        newLine = getFormula(_X1, _Y1, _X2, _Y2);

        if(wallLine[0] == newLine[0]){
            return new float[] {-1,-1};
        }
        
        //x = (b2 - b) / (m - m2)
        float Xintersect = (newLine[1] - wallLine[1]) / (wallLine[0] - newLine[0]);
        //m * Xintersect + b
        Yintersect = wallLine[0] * Xintersect + wallLine[1];

        return new float[] {Xintersect, Yintersect};
    }

        public void Reset(){
	 		this.X1 = 0;
			this.Y1 = 0;
			this.X2 = 0;
			this.Y2 = 0;
		    this.collisionNum = 0;
            this.prevCollisionNum = 0;
         }
    }
}
