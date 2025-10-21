using System.Runtime.CompilerServices;

namespace SpellCheckingTool
{
    public unsafe class Program
    {
        static void Main(string[] args)
        {
            string s = Test();
            Console.WriteLine(s);
            Console.Read();
        }

        public static string Test()
        {
            return "Hello World!";
        }
    }
}
