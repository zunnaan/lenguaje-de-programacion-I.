using System.Text;

public static class Identifier
{
    public static string Clean(string identifier)
    {
        var sb = new StringBuilder();
        bool upper = false;

        foreach (char c in identifier)
        {
            if (c == ' ') sb.Append('_');
            else if (char.IsControl(c)) sb.Append("CTRL");
            else if (c == '-') upper = true;
            else if (char.IsLetter(c) && (c < 'α' || c > 'ω'))
            {
                sb.Append(upper ? char.ToUpper(c) : c);
                upper = false;
            }
        }

        return sb.ToString();
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine(Identifier.Clean("my   Id"));
        Console.WriteLine(Identifier.Clean("my\0Id"));
        Console.WriteLine(Identifier.Clean("à-ḃç"));
        Console.WriteLine(Identifier.Clean("1😀2😀3😀"));
        Console.WriteLine(Identifier.Clean("MyΟβιεγτFinder"));
    }
}