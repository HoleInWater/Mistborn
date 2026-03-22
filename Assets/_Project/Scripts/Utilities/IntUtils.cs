using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for integer operations
    /// </summary>
    public static class IntUtils
    {
        /// <summary>
        /// Clamps an integer between min and max
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Returns the minimum of two integers
        /// </summary>
        public static int Min(int a, int b)
        {
            return Mathf.Min(a, b);
        }

        /// <summary>
        /// Returns the maximum of two integers
        /// </summary>
        public static int Max(int a, int b)
        {
            return Mathf.Max(a, b);
        }

        /// <summary>
        /// Returns the absolute value of an integer
        /// </summary>
        public static int Abs(int value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// Returns the sign of an integer (-1, 0, or 1)
        /// </summary>
        public static int Sign(int value)
        {
            return Mathf.Sign(value);
        }

        /// <summary>
        /// Returns the power of an integer (returns float due to potential large values)
        /// </summary>
        public static float Pow(int value, int power)
        {
            return Mathf.Pow(value, power);
        }

        /// <summary>
        /// Returns the square root of an integer (returns float)
        /// </summary>
        public static float Sqrt(int value)
        {
            return Mathf.Sqrt(value);
        }

        /// <summary>
        /// Returns true if an integer is even
        /// </summary>
        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// Returns true if an integer is odd
        /// </summary>
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// Returns true if an integer is prime
        /// </summary>
        public static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            
            var boundary = (int)Mathf.Floor(Mathf.Sqrt(number));
            
            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }
            
            return true;
        }

        /// <summary>
        /// Returns the greatest common divisor of two integers
        /// </summary>
        public static int GCD(int a, int b)
        {
            a = Abs(a);
            b = Abs(b);
            
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Returns the least common multiple of two integers
        /// </summary>
        public static int LCM(int a, int b)
        {
            if (a == 0 || b == 0) return 0;
            return Abs(a * b) / GCD(a, b);
        }

        /// <summary>
        /// Returns the factorial of an integer
        /// </summary>
        public static long Factorial(int n)
        {
            if (n < 0) return -1;
            if (n == 0) return 1;
            
            long result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        /// <summary>
        /// Returns the sum of integers from 1 to n
        /// </summary>
        public static int SumTo(int n)
        {
            return n * (n + 1) / 2;
        }

        /// <summary>
        /// Returns the nth Fibonacci number
        /// </summary>
        public static long Fibonacci(int n)
        {
            if (n <= 0) return 0;
            if (n == 1) return 1;
            
            long a = 0;
            long b = 1;
            
            for (int i = 2; i <= n; i++)
            {
                long temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (exclusive)
        /// </summary>
        public static int RandomRange(int min, int max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Returns a random integer between 0 and max (exclusive)
        /// </summary>
        public static int Random(int max)
        {
            return Random.Range(0, max);
        }
    }
}
