using System;
using System.Collections.Generic;
using System.Threading;

namespace FoodMarket
{
    public delegate void ReturnBuyer(object sender, Buyer buyer);
    public delegate void FinishedWorkStend(object sender, StendState stendState);
    public enum StendState
    {
        Work,
        NotWork,
        Finishing
    }
    class Stand: IComparable
    {
        object locked = new object();
        object colorLocked = new object();
        public decimal Profylability
        {
            get
            {
                lock (locked)
                {
                    return profylability;
                }
            }
            set
            {
                lock (locked)
                {
                    profylability = value;
                }
            }
        }
        public int CountOfSelledProduct
        {
            get { return countOfSelledProduct; }
            set { countOfSelledProduct = value; }
        }
        public string ProductName { get; private set; }
        public decimal ProductPrice { get; private set; }
        public Queue<Buyer> Buyers
        {
            get
            {
                lock (locked)
                {
                    return buyers;
                }
            }
            set
            {
                lock (locked)
                {
                    buyers = value;
                }
            }
        }
        public event ReturnBuyer ReturnBuyerToManager; 
        public event FinishedWorkStend FinishedWork; 
        public StendState StendState { get; set; }


        public Stand(string name, decimal price)
        {
            this.CountOfSelledProduct = 0;
            this.Buyers = new Queue<Buyer>();
            this.ProductName = name;
            this.ProductPrice = price;

            this.sellers = new List<Seller>(new Random().Next(1, 6)); 
            Thread.Sleep(50);
            for (int i = 0; i < this.sellers.Capacity; i++)
                this.sellers.Add(new Seller()); 

            this.sellersTread = new List<Thread>(this.sellers.Count); 
            for (int i = 0; i < sellersTread.Capacity; i++)
            {
                this.sellersTread.Add(new Thread(this.sellers[i].StartToWork)); 
                this.sellersTread[i].Name = i + "Продавец" + this.ProductName + "Thread";
            }
            foreach (Seller seller in this.sellers)
                seller.ReturnCurrentBuyer += new ReturnBuyer(seller_ReturnCurrentBuyer); 

            this.Profylability = this.sellers.Count * this.ProductPrice / 1;

            Console.WriteLine("На стенде {0} цена товара {1}, продавцов {2}, рентабельность {3}",
                this.ProductName, this.ProductPrice, this.sellers.Count, this.Profylability);
        }

        void seller_ReturnCurrentBuyer(object sender, Buyer buyer)
        {
            this.CountOfSelledProduct++;
            buyer.Visitedstends.Add(this.ProductName);
           
            lock (colorLocked)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Стенд {0} принял от продавца покупателя с id={1}", this.ProductName, buyer.ID);
                Console.ForegroundColor = ConsoleColor.White;
            }

            lock (colorLocked)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Стенд {0} отправил покупателя с id={1} менеджеру", this.ProductName, buyer.ID);
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (this.ReturnBuyerToManager != null)
            {
                this.ReturnBuyerToManager(this, buyer);
            }
        }

        public void StartToWork()
        {
            this.StendState = StendState.Work;
            lock (colorLocked)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Стенд {0} начал работу", this.ProductName);
                Console.ForegroundColor = ConsoleColor.White;
            }
            foreach (Thread thread in this.sellersTread)
                thread.Start();

            while (true)
            {
                Thread.Sleep(1000);
                lock (colorLocked)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("На стенде {0} очередь в {1} человек. Рентабельность стенда: {2}",
                        this.ProductName, this.Buyers.Count, this.Profylability);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (this.buyers.Count != 0)
                {
                    foreach (Seller seller in this.sellers)
                    {
                        if (true == seller.IsFree)
                        {
                            if (this.buyers.Count == 0)
                                break;

                            lock (colorLocked)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Стенд {0} отдал продавцу покупателя с id={1}",
                                    this.ProductName, this.buyers.Peek().ID);
                                Console.ForegroundColor = ConsoleColor.White;

                                seller.PassBuyer(this.buyers.Dequeue());
                            }
                            if (this.buyers.Count != 0)
                                this.Profylability = this.ProductPrice * this.sellers.Count / this.Buyers.Count;
                            else
                                this.Profylability = this.ProductPrice * this.sellers.Count / 1;
                        }
                    }
                    continue;
                }

                if (this.StendState == StendState.Work)
                {
                    lock (colorLocked)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Стенд {0} пустует, но работает", this.ProductName);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    continue;
                }

                if (true == this.IsAllSellersFree())
                {
                    foreach (Seller seller in this.sellers)
                        seller.NoMoreBuyers();
                    this.StendState = FoodMarket.StendState.NotWork;

                    if (this.FinishedWork != null)
                        this.FinishedWork(this, StendState.NotWork);

                    break;
                }

            }
        }

        public void EndToWork()
        {
            this.StendState = FoodMarket.StendState.Finishing;
        }

        bool IsAllSellersFree()
        {
            foreach (Seller seller in this.sellers)
                if (true != seller.IsFree)
                    return false;

            return true;
        }

        public int CompareTo(object obj)
        {
            Stand temp = (Stand)obj;
            if (this.Profylability > temp.Profylability)
                return 1;
            if (this.Profylability < temp.Profylability)
                return -1;
            return 0;
        }

        List<Seller> sellers;
        List<Thread> sellersTread;
        Queue<Buyer> buyers;       
        decimal profylability;
        int countOfSelledProduct; 
    }
}
