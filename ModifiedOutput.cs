namespace HashAxe.ModifiedOutput{
    public class LineOutput
    {
        public static void WriteLineColor(string content, ConsoleColor color, object? arg0 = null, object? arg1 = null)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content, arg0, arg1);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogSuccess(string content, object? arg0 = null, object? arg1 = null)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("SUCCESS");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.Write(content, arg0, arg1);
            Console.WriteLine();
        }

        public static void LogWarning(string content, object? arg0 = null, object? arg1 = null)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARNING");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.Write(content, arg0, arg1);
            Console.WriteLine();
        }
        
        public static void LogFailure(string content, object? arg0 = null, object? arg1 = null) {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("FAILURE");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.Write(content, arg0, arg1);
            Console.WriteLine();
        }
    }
}