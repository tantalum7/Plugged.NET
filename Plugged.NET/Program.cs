﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugged.NET
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Plugged.NET");
            

            Plugged.Discover();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

        }
    }
}
