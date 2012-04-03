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
        //public float slope { get; set; }
        //public float b { get; set; }
        public int Id { get { return _id; } }
        public float collisionNum { get; set; }
        public float prevCollisionNum { get; set; }
        //public bool inRegion { get; set; }
        public float intersect { get; set; } 

        public Wall(int id)
        {
            _id = id;
        }

	public float[] getFormula(float X1, float Y1, float X2, float Y2){
        float slope = 0f;
        float b = 0f;
        
        /*if (X1 == X2)
            slope = float.MaxValue;
        else
		*/    slope = (Y2-Y1)/(X2-X1);
		
        b = Y2-slope*X2;

        return new float[] { slope, b };
	}

    /**
        * Check whether an agent collided with a wall.
        **/
	public bool checkCollision(IAgent agent){
        // X = (b2 - b) / (m - m2)
        float Xintersect = getXYinteresect(agent)[0];
        if (Xintersect == -1)
            return false;
        float tempWallX1 = Math.Min(this.X1, this.X2);
        float tempWallX2 = Math.Max(this.X1, this.X2);

        float tempAgentPrevX = Math.Min(agent.prevX, agent.X);
        float tempAgentX = Math.Max(agent.prevX, agent.X);
        //debug
        intersect = Xintersect;
        if ((Xintersect >= tempWallX1 && Xintersect <= tempWallX2) 
            && (Xintersect >= tempAgentPrevX && Xintersect <= tempAgentX)) 
        {
            return true;
        }

        return false;
        
        /*
        float V = Math.Min(agent.MaxVelocity, Math.Max(0, agent.Velocity));
	    float Orient = agent.Orientation;
        Orient += 360;
        Orient %= 360;
        
        //velocities of x and y
        float vX = V * (float)(Math.Cos(Orient * (Math.PI / 180.0)));
        float vY = V * (float)(Math.Sin(Orient * (Math.PI / 180.0)));
 
        this.prevCollisionNum = collisionNum;
        this.collisionNum = ((Y) - (this.slope * (X) + this.b));
        
        //region check
        inRegion = checkRegion(X,Y,vX,vY);
        if (inRegion)
        {
            if ((this.collisionNum <= 0 && this.prevCollisionNum >= 0) || (this.collisionNum >= 0 && this.prevCollisionNum <= 0))
                {
                    return true;
            }
        }

        return false;
        //return (Y - (this.slope*X + this.b)) == 0;
        */
	}
    public float[] getXYinteresect(IAgent agent)
    {   
        //slope and b for a wall
        float[] wallLine = new float[2];
        wallLine = getFormula(this.X1, this.Y1, this.X2, this.Y2);

        //slope and b for a agent
        float[] agentLine = new float[2];
        agentLine = getFormula(agent.X, agent.Y, agent.prevX, agent.prevY);

        if(wallLine[0] == agentLine[0] || Math.Abs(wallLine[0] - agentLine[0])< 1){
            return new float[] {-1,-1};
        }
        
        //x = (b2 - b) / (m - m2)
        float Xintersect = (agentLine[1] - wallLine[1]) / (wallLine[0] - agentLine[0]);
        //m * Xintersect + b
        float Yintersect = wallLine[0] * Xintersect + wallLine[1];

        return new float[] {Xintersect, Yintersect};
    }

        public void Reset(){
	 		this.X1 = 0;
			this.Y1 = 0;
			this.X2 = 0;
			this.Y2 = 0;
			//this.slope = 0;
			//this.b = 0;
            this.collisionNum = 0;
            this.prevCollisionNum = 0;
            //this.inRegion = false;
		}

        /**
            * Check whether given X and Y is in the region of a wall
            * Parameter: X, Y, velocities of an agent
            **/
        public bool checkRegion(float X, float Y, float vX, float vY)
        {
            if (((X <= this.X1 + Math.Abs(vX) && X >= this.X2 - Math.Abs(vX)) || (X >= this.X1 - Math.Abs(vX) && X <= this.X2 + Math.Abs(vX)))
                && ((Y <= this.Y1 + Math.Abs(vY) && Y >= this.Y2 - Math.Abs(vY)) || (Y >= this.Y1 - Math.Abs(vY) && Y <= this.Y2 + Math.Abs(vY))))
            {
                return true;

            }
            return false;
        }
    }
}
