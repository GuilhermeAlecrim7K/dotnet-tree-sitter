namespace TreeSitter.Pascal.Tests;

public class NodeTests : PascalTestFixture
{
    [Test]
    public void ToString_WithValidNode_ShouldReturnSExpression()
    {
        using var tree = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);
        var rootNode = tree.RootNode();
        Assert.That(rootNode.ToString(), Is.EqualTo(PascalGrammar.SAMPLE_PROGRAM_S_EXPRESSION), "The S-Expression of the root node should match the expected structure for the given source code.");
    }

    [TestCaseSource(nameof(NodeShapes))]
    public void TypeAndCount_ForNodeAtPath_ShouldReturnExpectedValues(string childPath, string expectedType, uint expectedChildCount, uint expectedNamedChildCount)
    {
        using var tree = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);
        var node = NodeAt(tree.RootNode(), childPath);
        Assert.That(node, Is.Not.Null, $"Node at path '{childPath}' should not be null.");

        Assert.Multiple(() =>
        {
            Assert.That(node!.Type(), Is.EqualTo(expectedType), $"Type mismatch at path '{childPath}'.");
            Assert.That(node!.ChildCount(), Is.EqualTo(expectedChildCount), $"ChildCount mismatch for '{expectedType}'.");
            Assert.That(node!.NamedChildCount(), Is.EqualTo(expectedNamedChildCount), $"NamedChildCount mismatch for '{expectedType}'.");
        });
    }

    [Test]
    public void GrammarSymbol_OnNamedNode_ReturnsNonZeroId()
    {
        using var tree = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);
        var programNode = tree.RootNode().Child(0);
        Assert.That(programNode, Is.Not.Null, "The 'program' node should not be null.");

        Assert.Multiple(() =>
        {
            Assert.That(programNode!.GrammarSymbol(), Is.Not.Zero, "GrammarSymbol should return a valid (non-zero) symbol id.");
            // For a non-aliased/non-supertype node the grammar symbol coincides with the public symbol.
            Assert.That(programNode!.GrammarSymbol(), Is.EqualTo(programNode!.Symbol()), "GrammarSymbol should match Symbol for a non-aliased node.");
        });
    }

    [Test]
    public void GrammarType_OnNamedNode_ReturnsExpectedString()
    {
        using var tree = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);
        var programNode = tree.RootNode().Child(0);
        Assert.That(programNode, Is.Not.Null, "The 'program' node should not be null.");

        Assert.Multiple(() =>
        {
            Assert.That(programNode!.GrammarType(), Is.Not.Empty, "GrammarType should return a non-empty string.");
            Assert.That(programNode!.GrammarType(), Is.EqualTo("program"), "GrammarType should report the grammar type name for the node.");
        });
    }

    [Test]
    public void Text_WithNonAsciiSource_ShouldPreserveCharacters()
    {
        using var tree = Parser.ParseString(PascalGrammar.NON_ASCII_PROGRAM);

        // The root spans the whole document from offset 0, so Text returns the source
        // (up to the last token) regardless of how the grammar lexes "Café". This guards
        // the byte / sizeof(ushort) char-offset math for BMP non-ASCII: each such char is
        // one UTF-16 code unit (2 bytes), so char indices stay aligned.
        Assert.That(tree.RootNode().Text(PascalGrammar.NON_ASCII_PROGRAM), Does.Contain("Café"), "Char-offset slicing over a UTF-16 document must preserve BMP non-ASCII characters.");
    }

    /// <summary>
    /// One case per node of <see cref="PascalGrammar.SAMPLE_PROGRAM"/>'s parse tree:
    /// child path, expected type, expected child count, expected named-child count.
    /// The path is a '.'-separated list of child indices from the root; "" is the root itself.
    /// Mirrors <see cref="PascalGrammar.SAMPLE_PROGRAM_S_EXPRESSION"/>.
    /// </summary>
    private static IEnumerable<TestCaseData> NodeShapes()
    {
        yield return Shape("", "root", 1, 1);
        yield return Shape("0", "program", 5, 4);
        yield return Shape("0.0", "kProgram", 0, 0);
        yield return Shape("0.1", "moduleName", 1, 1);
        yield return Shape("0.1.0", "identifier", 0, 0);
        yield return Shape("0.2", ";", 0, 0);
        yield return Shape("0.3", "block", 2, 2);
        yield return Shape("0.3.0", "kBegin", 0, 0);
        yield return Shape("0.3.1", "kEnd", 0, 0);
        yield return Shape("0.4", "kEndDot", 1, 0);
        yield return Shape("0.4.0", ".", 0, 0);
    }

    // SetArgDisplayNames keeps the case names readable: NUnit would otherwise render 0u as
    // "uint.MinValue" and the path of the root node as an empty string.
    private static TestCaseData Shape(string childPath, string expectedType, uint childCount, uint namedChildCount) =>
        new TestCaseData(childPath, expectedType, childCount, namedChildCount)
            .SetArgDisplayNames(childPath.Length == 0 ? "root" : childPath, expectedType, childCount.ToString(), namedChildCount.ToString());

    private static Node? NodeAt(Node root, string childPath)
    {
        if (childPath.Length == 0)
            return root;

        Node? node = root;
        foreach (var index in childPath.Split('.'))
            node = node?.Child(uint.Parse(index));

        return node;
    }
}
