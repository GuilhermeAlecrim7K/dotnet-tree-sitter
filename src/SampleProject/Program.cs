namespace SampleProject;

public class Program
{
	public static void Main()
	{
		var lang = Pascal.CreateLanguage();
		Console.WriteLine($"Pascal language has {lang.Symbols.Length} symbols.");
	}
}
