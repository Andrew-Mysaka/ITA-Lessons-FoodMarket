using System;
using System.Threading;

namespace FoodMarket
{
    class Program
    {
        static void Main(string[] args)
        {
            Shop shop = new Shop();
            Thread shopThread = new Thread(shop.OpenShop);
            
            Console.WriteLine("Откритие магазина");
            Console.ReadKey();

            shopThread.Start();

            Console.WriteLine("Закрытие магазина");
            Console.ReadKey();
            shop.CloseShop();

            Console.ReadKey();
        }
    }
}
