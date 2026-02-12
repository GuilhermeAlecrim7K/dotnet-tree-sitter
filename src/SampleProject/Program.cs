using TreeSitter;

namespace SampleProject;

public class Program
{
	public static void Main()
	{
        using var parser = new Parser();
        using var jsonLang = Json.CreateLanguage();
        parser.SetLanguage(jsonLang);

        var source = @"[1, null]";

        using var tree = parser.ParseString(source);
        var rootNode = tree.RootNode();
        var arrayNode = rootNode.NamedChild(0)!;
        var numberNode = arrayNode.NamedChild(0)!;

        Console.WriteLine($"Root node type: {rootNode.Type()}. Expected: 'document'.");
        Console.WriteLine($"Root node named child 0 type: {arrayNode.Type()}. Expected: 'array'.");
        Console.WriteLine($"Array node named child 0 type: {numberNode.Type()}. Expected: 'number'.");

        Console.WriteLine($"Root node children count: {rootNode.ChildCount()}. Expected: 1.");
        Console.WriteLine($"Array node children count: {arrayNode.ChildCount()}. Expected: 5.");
        Console.WriteLine($"Array node named children count: {arrayNode.NamedChildCount()}. Expected: 2.");
        Console.WriteLine($"Number node children count: {numberNode.ChildCount()}. Expected: 0.");

        Console.WriteLine($"Root node text: '{rootNode.ToString()}'.");
	}
}