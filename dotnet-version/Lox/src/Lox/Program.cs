using LoxGenerator;

namespace Lox
{
    internal class Program
    {
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
        private static Interpreter _interpreter = new ();
        private Environment _environment = new Environment();
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: dlox [script]");
                System.Environment.Exit(64);
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
                System.Environment.Exit(64);
            }
        }
        static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            var expressions = parser.Parse();

            // Stop if there was a syntax error.
            if (_hadError) return;

            //Console.WriteLine(new AstPrinter().Print(expression));
            
            _interpreter.Interpret(expressions);
            
            // foreach (var token in tokens)
            // {
            //     Console.WriteLine(token);
            // }
        }
        public static void Error(int line, string message) =>
            Report(line, "", message);
        public static void RuntimeError(Interpreter.RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message} \n[line ${error.Token.Line}]");
            _hadRuntimeError = true;
        }
        public static void Error(Token token, String message) {
            if (token.Type == TokenType.Eof) {
                Report(token.Line, " at end", message);
            } else {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }
        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error: {where}: {message}");
            _hadError = true;
        }
    }
}