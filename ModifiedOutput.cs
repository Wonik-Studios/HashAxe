namespace HashAxe.ModifiedOutput{
    public class LineOutput
    {
        public static void WriteLineColor(string content, ConsoleColor color, object? arg0 = null, object? arg1 = null)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content, arg0, arg1);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}