﻿using System;
using System.Reactive.Linq;

namespace Astute
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(l => l < 1 ? "JOIN#" : "SHOOT#")
                .Subscribe(Output.TcpOutput);
            Input.TcpInput.Retry().Subscribe(Console.WriteLine);
            Console.ReadKey();
        }
    }
}