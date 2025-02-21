using LoxGenerator;

namespace Lox
{
    internal class Program
    {
        private static bool _hadError = false;
        
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: dlox [script]");
                Environment.Exit(64);
            } 
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        static void RunPrompt()
        {
            
            for (;;)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }
                Run(line);
                _hadError = false;
            }
        }

        static void RunFile(string path)
        {
            var file = File.ReadAllText(path);
            Run(file);

            if (_hadError)
            {
                Environment.Exit(64);
            }
        }
        static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message) =>
            Report(line, "", message);

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error: {where}: {message}");
            _hadError = true;
        }
    }
}