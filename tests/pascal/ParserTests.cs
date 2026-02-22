namespace TreeSitter.Pascal.Tests;

public class ParserTests
{
    private PascalLanguage _language = null!;
    private const string SAMPLE_SOURCE = "program HelloWorld; begin end.";
    private const string SAMPLE_SOURCE_S_EXPRESSION = "(root (program (kProgram) (moduleName (identifier)) (block (kBegin) (kEnd)) (kEndDot)))";

    [SetUp]
    public void Setup()
    {
        _language = new PascalLanguage();
    }

    [TearDown]
    public void TearDown()
    {
        _language.Dispose();
    }

    [Test]
    public void Dispose_WithParser_ShouldFreeResourcesWithoutThrowingException()
    {
        using var sut = _language.CreateParser();
        Assert.That(() => sut.Dispose(), Throws.Nothing);
        Assert.That(() => sut.Dispose(), Throws.Nothing, "Calling Dispose multiple times should not throw exceptions.");
        Assert.That(() => sut.Language, Throws.TypeOf<ObjectDisposedException>(), " Accessing properties after disposal should throw ObjectDisposedException.");
        Assert.That(() => sut.ParseString(""), Throws.TypeOf<ObjectDisposedException>(), " Accessing method 'ParseString' after disposal should throw ObjectDisposedException.");
        Assert.That(() => sut.Reset(), Throws.TypeOf<ObjectDisposedException>(), " Accessing method 'Reset' after disposal should throw ObjectDisposedException.");
        Assert.That(() => sut.SetLogger(null), Throws.TypeOf<ObjectDisposedException>(), " Accessing method 'Reset' after disposal should throw ObjectDisposedException.");
    }

    [Test]
    public void Language_WithParser_ShouldReturnAssociatedLanguage()
    {
        using var sut = _language.CreateParser();
        Assert.That(sut.Language, Is.SameAs(_language), "The Language property should return the same instance of the language used to create the parser.");
    }

    [Test]
    public void ParseString_WithValidSource_ShouldReturnTree()
    {
        using var sut = _language.CreateParser();
        var source = "program HelloWorld; begin end.";
        using var tree = sut.ParseString(source);
        Assert.That(tree, Is.Not.Null, "ParseString should return a valid Tree instance");
    }

    [Test]
    public void Parse_WhenParsingASecondSourceWithoutReset_ShouldNotThrow()
    {
        using var sut = _language.CreateParser();
        var source2 = "unit Foo; interface end.";
        using var tree1 = sut.ParseString(SAMPLE_SOURCE);
        Assert.That(
            tree1.RootNode().ToString(),
            Is.EqualTo(SAMPLE_SOURCE_S_EXPRESSION),
            "The S-Expression of the parsed tree should match the expected structure for the given source code."
        );
        using var tree2 = sut.ParseString(source2);
        Assert.That(
            tree2.RootNode().ToString(),
            Is.EqualTo("(root (unit (kUnit) (moduleName (identifier)) (interface (kInterface)) (kEnd) (kEndDot)))"),
            "The S-Expression of the parsed tree should match the expected structure for the given source code.");
    }

    [Test]
    public void SetLogger_WithValidCallback_ShouldNotThrowException()
    {
        using var sut = _language.CreateParser();
        // TODO: Replace the Console.WriteLine with a more robust logging mechanism that can be verified in tests.
        Assert.That(() => sut.SetLogger((level, message) => _ = $"Log (Level {level}): {message}"), Throws.Nothing);
        Assert.That(() => sut.ParseString(SAMPLE_SOURCE).Dispose(), Throws.Nothing);
    }

    public void SetLogger_WithNullCallback_ShouldNotThrowException()
    {
        using var sut = _language.CreateParser();
        Assert.That(() => sut.SetLogger(null), Throws.Nothing);
        Assert.That(() => sut.ParseString(SAMPLE_SOURCE).Dispose(), Throws.Nothing);
    }
}