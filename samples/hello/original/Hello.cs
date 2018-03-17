using System;

namespace hello
{
    public class Hello
    {
        public static void Main()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "bye")
                {
                    break;
                }
                Console.WriteLine("Hello, {0}!", input);
            }
        }
    }
}
