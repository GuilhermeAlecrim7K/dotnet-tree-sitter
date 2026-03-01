namespace TreeSitter.Pascal.Tests;

public class NodeTests
{
    private PascalLanguage _language = null!;
    private Parser _parser = null!;

    private const string SAMPLE_SOURCE = "program HelloWorld; begin end.";
    private const string SAMPLE_SOURCE_S_EXPRESSION = "(root (program (kProgram) (moduleName (identifier)) (block (kBegin) (kEnd)) (kEndDot)))";

    [SetUp]
    public void SetUp()
    {
        _language = new PascalLanguage();
        _parser = _language.CreateParser();
    }

    [TearDown]
    public void TearDown()
    {
        _parser.Dispose();
        _language.Dispose();
    }

    [Test]
    public void ToString_WithValidNode_ShouldReturnSExpression()
    {
        using var tree = _parser.ParseString(SAMPLE_SOURCE);
        var rootNode = tree.RootNode();
        var sExpression = rootNode.ToString();
        Assert.That(sExpression, Is.EqualTo(SAMPLE_SOURCE_S_EXPRESSION), "The S-Expression of the root node should match the expected structure for the given source code.");
    }

    [Test]
    public void TypeAndCount_WithSampleSource_ShouldReturnExpectedValues()
    {
        using var tree = _parser.ParseString(SAMPLE_SOURCE);
        var rootNode = tree.RootNode();
        // NOTE: Refer to the S-Expression in the SAMPLE_SOURCE_S_EXPRESSION constant for the expected structure of the parse tree. The assertions below are based on that structure.
        var runNodeAssertions = (Node? node, string expectedType, uint expectedChildCount, uint expectedNamedChildCount) =>
        {
            if (node is null)
            {
                Assert.Fail($"Node '{expectedType}' should not be null.");
                return;
            }
            Assert.Multiple(() =>
            {
                Assert.That(node.Type(), Is.EqualTo(expectedType), $"Type mismatch for.");
                Assert.That(node.ChildCount(), Is.EqualTo(expectedChildCount), $"ChildCount mismatch for '{expectedType}'.");
                Assert.That(node.NamedChildCount(), Is.EqualTo(expectedNamedChildCount), $"NamedChildCount mismatch for '{expectedType}'.");
            });
        };

        runNodeAssertions(rootNode, "root", 1, 1);
        var node = rootNode.Child(0);
        runNodeAssertions(node, "program", 5, 4);
        runNodeAssertions(node?.Child(0), "kProgram", 0, 0);
        runNodeAssertions(node?.Child(1), "moduleName", 1, 1);
        runNodeAssertions(node?.Child(1)?.Child(0), "identifier", 0, 0);
        runNodeAssertions(node?.Child(2), ";", 0, 0);
        runNodeAssertions(node?.Child(3), "block", 2, 2);
        runNodeAssertions(node?.Child(3)?.Child(0), "kBegin", 0, 0);
        runNodeAssertions(node?.Child(3)?.Child(1), "kEnd", 0, 0);
        runNodeAssertions(node?.Child(4), "kEndDot", 1, 0);
        runNodeAssertions(node?.Child(4)?.Child(0), ".", 0, 0);
    }
}