using System;
using System.Threading;

namespace FoodMarket
{
    public enum SellerState
    {
        Work,
        NotWork
    }
    class Seller
    {
        public event ReturnBuyer ReturnCurrentBuyer;
        SellerState sellerState;
        public bool IsFree;
        object locked = new object();
        object colorLocked = new object();
        public SellerState SellerState
        {
            get { lock (locked) { return sellerState; } }
            private set { lock (locked) { sellerState = value; } }
        }

        Buyer currentBuyer;

        public Seller()
        {
            this.currentBuyer = null;
            this.SellerState = SellerState.NotWork;
            this.IsFree = true;
        }

        public void PassBuyer(Buyer buyer)
        {
            lock (colorLocked)
            {
                this.IsFree = false;
                this.currentBuyer = buyer;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Продавец принял покупателя");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public void StartToWork()
        {
            this.SellerState = SellerState.Work;
            this.IsFree = true;

            while (this.SellerState != SellerState.NotWork)
            {
                if (this.currentBuyer != null)
                {
                    Thread.Sleep(new Random().Next(300, 500));

                    lock (colorLocked)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Продавец возвратил стенду покупателя с id={0}", this.currentBuyer.ID);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    if (this.ReturnCurrentBuyer != null)
                    {
                        this.ReturnCurrentBuyer(this, this.currentBuyer);
                        this.currentBuyer = null;
                    }
                    Thread.Sleep(200);
                }

                this.IsFree = true;
            }
        }

        public void NoMoreBuyers()
        {
            this.SellerState = SellerState.NotWork;
        }
    }
}
