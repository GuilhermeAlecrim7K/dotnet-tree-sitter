A fork of a refactored .NET binding for https://github.com/tree-sitter/tree-sitter library.

Sample code:

```cs
using TreeSitter;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        var lang = new PythonLanguage();
        var parser = new Parser(lang);
        var source = File.ReadAllText("source.py", Encoding.UTF8);

        using var tree = parser.ParseString(source);

        using (var query = new Query(lang, "(import_statement) @import"))
        {
            foreach (var node in query.Captures(tree.RootNode()).Select(x => x.Node))
            {
                var moduleName = node.ChildByFieldName("name").Text(source);
                var match = new
                {
                    type = "import",
                    module = moduleName,
                    start = new { row = node.StartPoint().Row, col = node.StartPoint().Column },
                    end = new { row = node.EndPoint().Row, col = node.EndPoint().Column },
                    text = node.Text(source)
                };

                Console.WriteLine(JsonConvert.SerializeObject(match));
            }
        }
    }
}
```

Binding for the grammar can be created using native grammar library and code like this:

```cs
using System;
using System.Runtime.InteropServices;
using TreeSitter;

namespace YourProject;

internal sealed partial class PythonLanguage() : Language(tree_sitter_python())
{ 
	[LibraryImport("tree-sitter-python")]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl)})]
    private static partial IntPtr tree_sitter_python();
}
```

## Status

This is currently a work in progress. There are no tests yet aside from the Sample Project. I will be getting the hang of how tree-sitter is supposed to be used and make updates as I go.

Quick tip: If there's no example of a certain class or method in the SampleProject, it hasn't been tested yet.

There are no build automations yet. This is how I've been setting up the project on an Ubuntu distro:

1. Submodules must be checked out
2. `cd` into each submodule and `make`
3. Extract `libtree-sitter.so` from tree-sitter and `libtree-sitter-json.so` from tree-sitter-json and paste into the bin folder at SampleProject.
4. dotnet build and dotnet run SampleProject.

If you decide to implement a class for another language using this repo as a starting point, you will have to follow the same steps.
