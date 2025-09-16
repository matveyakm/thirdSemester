using System;
using System.Threading;

class DiningPhilosophers
{
    private const int NumPhilosophers = 5;
    private const int Iterations = 5;
    private static readonly object[] Forks = new object[NumPhilosophers];
    private static readonly Random Random = new Random();

    static void Main()
    {
        for (int i = 0; i < NumPhilosophers; i++)
        {
            Forks[i] = new object();
        }

        Thread[] philosophers = new Thread[NumPhilosophers];
        for (int i = 0; i < NumPhilosophers; i++)
        {
            int id = i;
            philosophers[i] = new Thread(() => PhilosopherLife(id));
            philosophers[i].Start();
        }

        foreach (var philosopher in philosophers)
        {
            philosopher.Join();
        }

        Console.WriteLine("\nОбед окончен.");
    }

    private static void PhilosopherLife(int id)
    {
        for (int i = 0; i < Iterations; i++)
        {
            Console.WriteLine($"Философ {id} думает.");
            Thread.Sleep(Random.Next(1000, 3000));

            Console.WriteLine($"Философ {id} голоден и пытается взять вилки.");
            Eat(id);
        }
    }

    private static void Eat(int id)
    {
        int leftFork = id;
        int rightFork = (id + 1) % NumPhilosophers;

        int firstFork = Math.Min(leftFork, rightFork);
        int secondFork = Math.Max(leftFork, rightFork);

        lock (Forks[firstFork])
        {
            Console.WriteLine($"Философ {id} взял вилку {firstFork} (первую)");

            lock (Forks[secondFork])
            {
                Console.WriteLine($"Философ {id} взял вилку {secondFork} (вторую)");

                Console.WriteLine($"Философ {id} ест.");
                Thread.Sleep(Random.Next(1000, 3000));
                Console.WriteLine($"Философ {id} закончил есть.");
            }
        }
    }
}