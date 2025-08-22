using BankApplication.Models;
using System.Text;
#pragma warning disable CA1416

namespace BankApplication.Helpers
{
    internal static class MenuSystem
    {
        public static int MenuInput(string[]? headers, string[] options, ConsoleColor? color)
        {
            if (options == null || options.Length == 0) throw new ArgumentException("Options måste innehålla minst ett element.");

            int selectedIndex = 0;
            ConsoleKey key = ConsoleKey.NoName;

            do
            {
                Console.Clear();

                int totalLines = options.Length + (headers?.Length ?? 0);
                CenterY(totalLines + 6 + 1);
                Header();

                if (headers != null && headers.Length > 0)
                {
                    Console.ForegroundColor = color ?? ConsoleColor.Green;
                    WriteAllCenteredX(headers);
                    Console.WriteLine();
                    Console.ResetColor();
                }

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                        WriteCenteredXBackground($" ➔ {options[i]}   ");
                    else
                        WriteCenteredX(options[i]);
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                        Console.Beep(900, 80);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % options.Length;
                        Console.Beep(700, 80);
                        break;
                    case ConsoleKey.Enter:
                        Console.Beep(500, 200);
                        break;
                }
            } while (key != ConsoleKey.Enter);

            Console.Clear();
            return selectedIndex;
        }


        public static void WriteAllCenteredX(string[] messages)
        {
            int consoleWidth = Console.WindowWidth;

            foreach (var message in messages)
            {
                int paddingX = (consoleWidth - message.Length) / 2;
                string paddingSpaces = new string(' ', paddingX);
                Console.WriteLine(paddingSpaces + message);
            }
        }

        public static void WriteAllCenteredXForeground(string[] messages, ConsoleColor color)
        {
            int consoleWidth = Console.WindowWidth;
            Console.ForegroundColor = color;

            foreach (var message in messages)
            {
                int paddingX = (consoleWidth - message.Length) / 2;
                string paddingSpaces = new string(' ', paddingX);
                Console.WriteLine(paddingSpaces + message);
            }

            Console.ResetColor();
        }

        public static void WriteCenteredX(string message)
        {
            int consoleWidth = Console.WindowWidth;
            int paddingX = (consoleWidth - message.Length) / 2;
            string paddingSpaces = new string(' ', paddingX);
            Console.WriteLine(paddingSpaces + message);
        }

        public static void WriteCenteredXForeground(string message, ConsoleColor color, bool input)
        {
            int consoleWidth = Console.WindowWidth;
            int paddingX = (consoleWidth - message.Length) / 2;
            string paddingSpaces = new string(' ', paddingX);
            Console.ForegroundColor = color;

            if (input)
            {
                Console.Write(paddingSpaces);
                Console.Write(message);
            }
            else
            {
                Console.WriteLine(paddingSpaces + message);
            }

            Console.ResetColor();
        }

        public static void WriteCenteredXBackground(string message)
        {
            int consoleWidth = Console.WindowWidth;
            int paddingX = (consoleWidth - message.Length) / 2;

            string paddingSpaces = new string(' ', paddingX);
            Console.Write(paddingSpaces);

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Green;

            Console.Write(message);
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void CenterY(int headers)
        {
            int consoleHeight = Console.WindowHeight;
            int paddingY = (consoleHeight - headers) / 2;

            for (int i = 0; i < paddingY; i++)
            {
                Console.WriteLine();
            }
        }

        public static string? ReadNumberWithEscape()
        {
            var buffer = new StringBuilder();
            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.Beep(500, 150); 
                    Console.WriteLine();
                    return buffer.ToString();
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Console.Beep(300, 150); 
                    Console.WriteLine();
                    return null;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                    {
                        buffer.Remove(buffer.Length - 1, 1);
                        Console.SetCursorPosition(startLeft + buffer.Length, startTop);
                        Console.Write(' ');
                        Console.SetCursorPosition(startLeft + buffer.Length, startTop);
                        Console.Beep(400, 80); 
                    }
                }
                else if (char.IsDigit(key.KeyChar))
                {
                    buffer.Append(key.KeyChar);
                    Console.SetCursorPosition(startLeft + buffer.Length - 1, startTop);
                    Console.Write(key.KeyChar);
                    Console.Beep(700, 80); 
                }
            }
        }

        public static string? ReadLineWithEscape()
        {
            var buffer = new StringBuilder();
            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.Beep(500, 150); 
                    Console.WriteLine();
                    return buffer.ToString();
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Console.Beep(300, 150); 
                    Console.WriteLine();
                    return null;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                    {
                        buffer.Remove(buffer.Length - 1, 1);
                        Console.SetCursorPosition(startLeft + buffer.Length, startTop);
                        Console.Write(' ');
                        Console.SetCursorPosition(startLeft + buffer.Length, startTop);
                        Console.Beep(400, 80);
                    }
                }
                else
                {
                    buffer.Append(key.KeyChar);
                    Console.SetCursorPosition(startLeft + buffer.Length - 1, startTop);
                    Console.Write(key.KeyChar);
                    Console.Beep(700, 80); 
                }
            }
        }

        public static void ExitApplication()
        {
            int choice = MenuSystem.MenuInput(
                    new[] { "Avsluta", "Är du säker på att du vill stänga av Retro Bank 3000?" },
                    new[] { "Ja", "Nej" },
                    null
                );

            if (choice == 0)
            {
                MenuSystem.MenuInput(
                    new[] { "Tack för att du använde RetroBank 3000!", "Vi hoppas du haft en trevlig upplevelse!" },
                    new[] { "Avsluta" },
                    null
                );

                Environment.Exit(0);
            }

            return;
        }

        public static User? LogOut(User user)
        {
            int choice = MenuSystem.MenuInput(
                new[] { "LOGGA UT", "Är du säker på att du vill logga ut?" },
                new[] { "Ja", "Nej" },
                null
            );

            if (choice == 0)
            {
                MenuSystem.MenuInput(
                    new[] { "Du har nu loggats ut." },
                    new[] { "Fortsätt" },
                    null
                );
                return null;
            }

            return user;
        }

        public static void Header()
        {
            string[] banner = new[]
            {
                "╔══════════════════════════════════════╗",
                "║                                      ║",
                "║           RETROBANK 3000             ║",
                "║                                      ║",
                "╚══════════════════════════════════════╝",
                ""
            };

            MenuSystem.WriteAllCenteredXForeground(banner, ConsoleColor.Green);
        }

        public static void PlayMenuIntro()
        {
            if (!OperatingSystem.IsWindows()) return;

            var melody = new (int freq, int duration)[]
            {
                (659, 100), (659, 100), (0, 50), (659, 100), (0, 50),
                (523, 100), (659, 100), (0, 100), (784, 100), (0, 150),

                (392, 100), (523, 100), (392, 100), (330, 100), (440, 100),
                (494, 100), (466, 100), (440, 100), (392, 100),

                (1047, 80), (0, 50), (1047, 80), (0, 50), (1047, 80), (0, 50),

                (784, 120), (659, 120), (523, 120),

                (523, 80), (659, 80), (784, 80), (880, 80),
                (1047, 100), (880, 100), (784, 100), (659, 100),
                (523, 100)
            };

            foreach (var note in melody)
            {
                if (note.freq == 0)
                    System.Threading.Thread.Sleep(note.duration);
                else
                    Console.Beep(note.freq, note.duration);
            }
        }

        public static void PlayMenuIntroAsync()
        {
            Task.Run(() => PlayMenuIntro());
        }
    }
}
