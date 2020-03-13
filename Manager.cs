using System;
using System.Collections.Generic;
using System.Threading;

namespace FoodMarket
{
    public delegate void FinishedWorkManager(object sender, ManagerState managerState);

    public enum ManagerState
    {
        Work,
        NotWork,
        Finishing
    }
    class Manager
    {
        Shop shop;
        ManagerState managerState;
        List<Stand> stends;
        List<Thread> stendsThread;
        Buyer currentBuyer;
        object locked = new object();
        object colorLocked = new object();
        public Buyer CurrentBuyer
        {
            get { lock (locked) { return currentBuyer; } }
            set { lock (locked) { currentBuyer = value; } }
        }
        public event FinishedWorkManager FinishedWork;
        decimal profit;
        public static int CountOfVisitedBuyers { get; private set; }
        List<string> productName;
        List<Buyer> countOfBuyersFromAllStends;


        public Manager(Shop shop)
        {
            this.countOfBuyersFromAllStends = new List<Buyer>();
            this.profit = 0;
            this.productName = new List<string>();
            productName.Add("Шоколад");
            productName.Add("Мороженное");
            productName.Add("Напитки");


            CountOfVisitedBuyers = 0;

            this.managerState = ManagerState.NotWork;
            this.stends = new List<Stand>(3);
            for (int i = 0; i < stends.Capacity; i++)
                this.stends.Add(new Stand(this.productName[i], (i+1) * 1.0M));

            foreach (Stand stend in this.stends)
                stend.ReturnBuyerToManager += new ReturnBuyer(RizeVisitor);

            this.stendsThread = new List<Thread>(this.stends.Count);
            for (int i = 0; i < this.stends.Count; i++)
            {
                this.stendsThread.Add(new Thread(this.stends[i].StartToWork));
                this.stendsThread[i].Name = this.stends[i].ProductName + "Thread";
            }

            this.managerState = ManagerState.NotWork;
            this.shop = shop;
            this.shop.RizeNewVisitor += new NewVisitor(RizeVisitor);

            foreach (Stand stend in this.stends)
                stend.FinishedWork += new FinishedWorkStend(Stend_FinishedWork);
        }

        public void StartToWork()
        {
            lock (colorLocked)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Менеджер приступил к работе");
                Console.ForegroundColor = ConsoleColor.White;
            }

            foreach (Thread thread in this.stendsThread)
                thread.Start();

            this.managerState = ManagerState.Work;

            while (this.managerState != ManagerState.NotWork)
            {
                Thread.Sleep(50);

                if (this.managerState == ManagerState.Finishing)
                {
                    lock (colorLocked)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Менеджер удалил текущего покупателя поскольку магазин закрывается");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    this.currentBuyer = null;
                    continue;
                }

                if (this.currentBuyer != null)
                {
                    this.stends.Sort();
                    this.stends.Reverse();

                    if (this.currentBuyer.Visitedstends.Count == 0)
                    {
                        this.stends[0].Buyers.Enqueue(this.currentBuyer);
                        
                        lock (colorLocked)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Менеджер отправил в очередь стенда {0} нового покупателя с id={1}",
                                this.stends[0].ProductName, this.CurrentBuyer.ID);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        this.CurrentBuyer = null;
                        continue;
                    }

                    if (this.currentBuyer.Visitedstends.Count == this.stends.Count)
                    {
                        lock (colorLocked)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Менеджер избавился от покупателя с id={0} поскольку тот прошел все стенды",
                                this.CurrentBuyer.ID);
                            this.countOfBuyersFromAllStends.Add(this.CurrentBuyer);
                            Console.ForegroundColor = ConsoleColor.White;

                            this.currentBuyer = null;
                        }
                        continue;
                    }

                    foreach (Stand stend in this.stends)
                    {
                        if (this.currentBuyer.Visitedstends.Contains(stend.ProductName))
                            continue;

                        stend.Buyers.Enqueue(this.currentBuyer);

                        string visitedStends = "[";
                        foreach (string stendName in this.CurrentBuyer.Visitedstends)
                            visitedStends += " " + stendName;
                        visitedStends += "]";

                        lock (colorLocked)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Менеджер направил бывалого покупателя с id={0} (пройдены стенды: {2}) на стенд {1}",
                                this.CurrentBuyer.ID, stend.ProductName, visitedStends);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        this.currentBuyer = null;
                        break;
                    }
                }
            }
        }

        public void EndToWork()
        {
            lock (colorLocked)
            {
                this.managerState = ManagerState.Finishing;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Менеджер дал стендам команду завершать работу");
                Console.ForegroundColor = ConsoleColor.White;
            }

            foreach (Stand stend in this.stends)
                stend.EndToWork();
        }

        void RizeVisitor(object sender, Buyer buyer)
        {
            while (this.currentBuyer != null)
            {
                Thread.Sleep(50);
            }
                

            this.currentBuyer = buyer;

            if (sender is Stand)
            {
                lock (colorLocked)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Менеджер принял покупателя с id={0} со стенда {1}",
                        buyer.ID, ((Stand)sender).ProductName);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            if (sender is Shop)
            {
                lock (colorLocked)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Менеджер принял покупателя с id={0} с входя магазина", buyer.ID);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        void Stend_FinishedWork(object sender, StendState stendState)
        {
            foreach (Stand stend in this.stends)
                if (stend.StendState != StendState.NotWork)
                    return;

            this.managerState = ManagerState.NotWork;

            foreach (Stand stend in this.stends)
            {
                profit += stend.ProductPrice * stend.CountOfSelledProduct;
                Console.WriteLine("Стенд {0} продал {1} товаров по цене {2} и прибыль со стенда составляет {3}",
                    stend.ProductName, stend.CountOfSelledProduct, stend.ProductPrice, stend.ProductPrice * stend.CountOfSelledProduct);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Все стенды прошло {0} человек", this.countOfBuyersFromAllStends.Count);
            foreach (Buyer b in this.countOfBuyersFromAllStends)
                Console.WriteLine("Покупатель с id={0}", b.ID);

            Console.WriteLine("Общая прибыль составляет ${0}", this.profit);
            Console.ForegroundColor = ConsoleColor.White;


            if (this.FinishedWork != null)
                this.FinishedWork(this, ManagerState.NotWork);
        }
    }
}
