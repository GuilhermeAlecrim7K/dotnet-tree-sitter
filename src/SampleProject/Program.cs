using TreeSitter;

namespace SampleProject;

public class Program
{
    public static void Main()
    {
        GettingStarted();
        WorkingWithQueries();
    }

    public static void GettingStarted()
    {
        using var jsonLang = Json.CreateLanguage();
        using var parser = jsonLang.CreateParser();

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

        Console.WriteLine($"S-Expression: '{rootNode.ToString()}'.");
    }


    public static void WorkingWithQueries()
    {
        using var lang = Json.CreateLanguage();
        var source = """
        {
          "a": 1,
          "b": "2",
          "c": 3
        }
        """;
        using var parser = lang.CreateParser();
        using var tree = parser.ParseString(source);
        var rootNode = tree.RootNode();
        Console.WriteLine($"S-Expression: '{rootNode.ToString()}'.");

        using var query = new Query(lang, "(pair key: (string) @key value: (number) @value)");

        var captures = query.Captures(rootNode).ToList();
        for (int i = 0; i < captures.Count; i++)
        {
            Console.WriteLine($"Capture {query.CaptureNameForId(captures[i].Index)} at {i}: {captures[i].Node.Text(source)}, '{captures[i].Node.Type()}'");
        }
    }
}