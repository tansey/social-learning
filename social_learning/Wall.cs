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
        public float Y1{ get; set; }
	public float X2 { get; set; }
	public float Y2 { get; set; }
	public float slope {get;}
	public float b {get; }
        public int Id { get { return _id; } }

        public Wall(int id)
        {
            _id = id;
        }

	public void getFormula(){

		this.slope = (this.Y2-this.Y1)/(this.X2-this.X1);
		this.b = this.Y2-slope*this.X2;

	}

	public bool checkCollision(float X, float Y){
		getFormula();
		if((X>X1 && X>X2) || (X<X1 && X<X2) || (Y>Y1 && Y>Y2) || (Y<Y1 && Y<Y2)) return false;
		return (Y - (this.slope*X + this.b)) == 0;

	}

        public void Reset(){
			this.X1 = 0;
			this.Y1 = 0;
			this.X2 = 0;
			this.Y2 = 0;
		}
    }
}
