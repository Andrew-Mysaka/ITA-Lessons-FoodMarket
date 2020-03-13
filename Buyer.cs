using System.Collections.Generic;

namespace FoodMarket
{
    public class Buyer
    {
        object locked = new object();
        public List<string> Visitedstends
        {
            get { lock (locked) { return this.visitedstends; } }
            set { lock (locked) { this.visitedstends = value; } }
        }
        public int ID { private set; get; }

        public Buyer(int id)
        {
            this.ID = id;
            this.Visitedstends = new List<string>();
        }

        List<string> visitedstends;
    }
}
