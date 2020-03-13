using System;
using System.Threading;

namespace FoodMarket
{
    public enum ShopState
    {
        Open,
        Closing,
        Closed
    }

    public delegate void NewVisitor(object sender, Buyer buyer);

    class Shop
    {
        int newBuyerId;
        Manager manager;
        public event NewVisitor RizeNewVisitor;
        Thread managerThread;
        ShopState shopState;

        public ShopState ShopState
        {
            get { return this.shopState; }
            private set { this.shopState = value; }
        }

        public Shop()
        {
            this.newBuyerId = 0;
            this.ShopState = ShopState.Closed;
            this.manager = new Manager(this);
            this.manager.FinishedWork += new FinishedWorkManager(manager_FinishedWork);
            this.ShopState = ShopState.Closed;
            this.managerThread = new Thread(this.manager.StartToWork);
            managerThread.Name = "managerThread";
        }

        void manager_FinishedWork(object sender, ManagerState managerState)
        {
            if (managerState == ManagerState.NotWork)
                this.ShopState = ShopState.Closed;
        }

        public void OpenShop()
        {

            Console.WriteLine("Магазин открылся");
            this.ShopState = ShopState.Open;

            managerThread.Start();

            while (this.ShopState == ShopState.Open)
            {
                Thread.Sleep(new Random().Next(500, 800));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Появился новый  покупатель");
                Console.ForegroundColor = ConsoleColor.White;

                if (this.RizeNewVisitor != null)
                {
                    while (this.manager.CurrentBuyer != null)
                    {
                        Thread.Sleep(75);
                    }
                    
                    this.RizeNewVisitor(this, new Buyer(++newBuyerId));
                }
            }
        }
        public void CloseShop()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Магазин закрыл двери на вход");
            Console.ForegroundColor = ConsoleColor.White;
            this.ShopState = ShopState.Closing;
            this.manager.EndToWork();
        }
    }
}
