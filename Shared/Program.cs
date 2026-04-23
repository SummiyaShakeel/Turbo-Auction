using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Project_cars
{
    class Program
    {
        static void Main(string[] args)
        {
            string json = File.ReadAllText("cars.json");

            List<Car> carList = JsonConvert.DeserializeObject<List<Car>>(json);

            foreach (Car car in carList)
            {
                Console.WriteLine("Brand: " + car.Brand);
                Console.WriteLine("Model: " + car.Model);
                Console.WriteLine("Year: " + car.Year);
                Console.WriteLine("Price: $" + car.StartingPrice);
                Console.WriteLine("Image: " + car.ImagePath);
                Console.WriteLine("-----------------------------------");
            }

            Console.ReadKey();
        }
    }
}
