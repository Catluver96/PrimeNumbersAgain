
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Collections;
using System.Collections.Generic;
namespace PrimeNumbersAgain;

class Program
{
    private const int MaxAllowableTime = 3_000;

    /// <summary>
    /// Prompts for a prime's position, times the search, and displays the result and time check.
    /// </summary>
    /// <param name="args">Command-line arguments; these are not used.</param>
    static void Main(string[] args)
    {
        long n, prime;
        Stopwatch timer = new Stopwatch();

        PrintBanner();
        n = GetNumber();

        timer.Start();
        prime = FindNthPrime(n);
        timer.Stop();
        
        
        Console.WriteLine($"\nToo easy.. {prime} is the nth prime when n is {n}. I found that answer in {timer.Elapsed.TotalMilliseconds:F3} milliseconds.");

        EvaluatePassingTime(timer.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Finds the prime at the specified one-based position. Students must implement this method.
    /// </summary>
    /// <param name="n">The requested position, from 1 through 1,999,999, where 1 represents the prime 2.</param>
    /// <returns>The prime at position <paramref name="n"/> once implemented; currently returns 0 as a placeholder.</returns>
    static long FindNthPrime(long n)
    {
        if (n < 1)
            throw new ArgumentOutOfRangeException(nameof(n));
        //this is suppposed to crash the thing if it asks for 0th prime for example
        if (n == 1) return 2;
        double n_d = (double)n;
        double lnN = Math.Log(n_d);
        double lnLnN = Math.Log(lnN);
        double lnNSq = lnN * lnN;
        double lnLnNSq = lnLnN * lnLnN;
        double commonTerms = lnN + lnLnN - 1 + (lnLnN - 2) / lnN;
        long U;
        if (n < 6)
        {
            U = 15;
        }
        else if (n < 3468)
        {
            U = checked((long)Math.Ceiling(n_d * (lnN +lnLnN)));
        }
        else
        {
            U = checked((long)Math.Ceiling(n_d * (commonTerms - (lnLnNSq - 6 * lnLnN) / (2 * lnNSq))));
        }
        if (U % 2 == 0) U++;
        double root = Math.Ceiling(Math.Sqrt(U));
        if (root >= int.MaxValue)
            throw new NotSupportedException("Base prime limit is too large.");
        int factor = (int)root;
        if ((long)factor * factor < U) factor++;
        List<int> basePrimes = GenerateBasePrimes(factor);
        const int segment = 60000;
        long count = 1;
        for (long low = 3; low <= U;)
        {
            long high = low + Math.Min(U - low, 2L * (segment - 1));
            //if you put L after a int it turns it into a long
            BitArray bits= SieveS(low,high,basePrimes);
            for (int i = 0; i < bits.Length; i++)
            {
                if (!bits[i])
                {
                    count++;
                    if (count == n)
                        return low + 2L * i;
                }
            }

            if (high == U) break;
            low = high + 2;
        }
        throw new InvalidOperationException("Upper bound was too small.");
    }

    public static List<int> GenerateBasePrimes(int upperLimit)
    {
        List<int> basePrimes = new List<int>();
        BitArray isComposite = new BitArray((int)(((long)upperLimit+1)/2));
        isComposite[0] = true;
        for (int p = 3; (long)p*p <= upperLimit; p+=2)
        {
            if (!isComposite[(int)(p/2)])
            {
                for (long i = (long)p * p; i <= upperLimit; i += 2L*p)
                {
                    isComposite[(int)(i/2)] = true;
                }
            }
        }

        basePrimes.Add(2);
        for (long p = 3; p <= upperLimit; p+=2)
        {
            if (!isComposite[(int)(p/2)])
            {
                basePrimes.Add((int)p);
            }
        }

        return basePrimes;
    }

    public static BitArray SieveS(long low, long high, List<int> basePrimes)
    {
        int size = (int)((high - low) / 2 + 1);
        BitArray isComposite = new BitArray(size);
        foreach (int p in basePrimes)
        {
            if (p == 2) continue;
            if ((long)p * p > high) break;
            long remainder = low % p;
            long offset = remainder == 0 ? 0 : p - remainder;
            if (offset > high - low) continue;
            long start = Math.Max(low + offset,(long)p*p);
            if(start%2==0)
            {
                if (high - start < p) continue;
                start +=p;
            }
            for(long j =start;j<=high;)
            {
                isComposite[(int)((j-low)/2)] = true;
                if (high - j < 2L*p) break;
                j+=2L*p;
            }
        }
        return isComposite;
    }
    /// <summary>
    /// Prompts until the user enters a valid 32-bit integer.
    /// </summary>
    /// <returns>The entered integer; its range is not validated against the assignment requirements.</returns>
    static long GetNumber()
    {
        long n = 0;
        while (true)
        {
            Console.Write("Which nth prime should I find?: ");
            
            string num = Console.ReadLine();
            if (Int64.TryParse(num, out n))
            {
                return n;
            }

            Console.WriteLine($"{num} is not a valid number.  Please try again.\n");
        }
    }

    /// <summary>
    /// Displays the application banner and configured time limit.
    /// </summary>
    static void PrintBanner()
    {
        Console.WriteLine(".................................................");
        Console.WriteLine(".#####...#####...######..##...##..######...####..");
        Console.WriteLine(".##..##..##..##....##....###.###..##......##.....");
        Console.WriteLine(".#####...#####.....##....##.#.##..####.....####..");
        Console.WriteLine(".##......##..##....##...##...##..##..........##.");
        Console.WriteLine(".##......##..##..######..##...##..######...####..");
        Console.WriteLine(".................................................\n\n");
        Console.WriteLine($"Nth Prime Solver O-Matic Online..\nGuaranteed to find primes up to 2 million in under {MaxAllowableTime} milliseconds!\n\n");
        
    }

    /// <summary>
    /// Displays a colored pass or fail result by comparing the supplied time with the configured limit.
    /// </summary>
    /// <param name="time">The elapsed time in milliseconds to compare with <see cref="MaxAllowableTime"/>.</param>
    static void EvaluatePassingTime(double time)
    {
        Console.WriteLine("\n");
        Console.Write("Time Check: ");

        if (time <= MaxAllowableTime)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Pass");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Fail");
        }

        Console.ForegroundColor = ConsoleColor.Gray;
        
    }
}
