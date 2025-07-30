using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Helpers
{
    internal static class Menu
    {
        public static int ShowArrowMenu(string[] headers, string[] options)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;

                foreach (var header in headers)
                {
                    WriteCentered(header);
                    Console.WriteLine();
                }

                Console.ResetColor();
                Console.WriteLine();

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.Black;
                        WriteCentered($" => {options[i]}\n");
                        Console.ResetColor();
                    }
                    else
                    {
                        WriteCentered($"   {options[i]}\n");
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % options.Length;
                        break;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }

        public static void WriteCenteredWithBackground(string text, ConsoleColor fgColor, ConsoleColor bgColor)
        {
            int consoleWidth = Console.WindowWidth;
            int textLength = text.Length;
            int padding = consoleWidth - textLength / 2;
            string paddingSpaces = new string(' ', padding);

            Console.ForegroundColor = fgColor;
            Console.BackgroundColor = bgColor;

            Console.WriteLine(paddingSpaces + text + paddingSpaces);
            Console.ResetColor();
        }
    }
}
