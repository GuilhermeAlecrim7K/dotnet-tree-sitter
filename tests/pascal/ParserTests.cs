namespace TreeSitter.Pascal.Tests;

public class ParserTests : PascalTestFixture
{
    [Test]
    public void Dispose_WithParser_ShouldBeIdempotentAndInvalidateMembers()
    {
        // NOTE: asserts the managed disposal contract only — native release is not observable
        // from managed code, so this test cannot claim to verify that resources were freed.
        Parser.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(() => Parser.Dispose(), Throws.Nothing, "Calling Dispose multiple times should not throw exceptions.");
            Assert.That(() => Parser.Language, Throws.TypeOf<ObjectDisposedException>(), "Accessing property 'Language' after disposal should throw ObjectDisposedException.");
            Assert.That(() => Parser.ParseString(""), Throws.TypeOf<ObjectDisposedException>(), "Calling 'ParseString' after disposal should throw ObjectDisposedException.");
            Assert.That(() => Parser.Reset(), Throws.TypeOf<ObjectDisposedException>(), "Calling 'Reset' after disposal should throw ObjectDisposedException.");
            Assert.That(() => Parser.SetLogger(null), Throws.TypeOf<ObjectDisposedException>(), "Calling 'SetLogger' after disposal should throw ObjectDisposedException.");
        });
    }

    [Test]
    public void Language_WithParser_ShouldReturnAssociatedLanguage()
    {
        Assert.That(Parser.Language, Is.SameAs(Language), "The Language property should return the same instance of the language used to create the parser.");
    }

    [Test]
    public void ParseString_WithValidSource_ShouldReturnTree()
    {
        using var tree = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);
        Assert.That(tree, Is.Not.Null, "ParseString should return a valid Tree instance");
    }

    [Test]
    public void ParseString_WithSecondSourceWithoutReset_ShouldReturnCorrectTree()
    {
        using var tree1 = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);
        using var tree2 = Parser.ParseString(PascalGrammar.SAMPLE_UNIT);

        Assert.Multiple(() =>
        {
            Assert.That(
                tree1.RootNode().ToString(),
                Is.EqualTo(PascalGrammar.SAMPLE_PROGRAM_S_EXPRESSION),
                "The first tree should still match its source after a second parse.");
            Assert.That(
                tree2.RootNode().ToString(),
                Is.EqualTo(PascalGrammar.SAMPLE_UNIT_S_EXPRESSION),
                "The second parse should reflect the second source, without an intervening Reset.");
        });
    }

    [Test]
    public void SetLogger_WithValidCallback_ShouldInvokeCallbackDuringParse()
    {
        var entries = new List<(LogType Level, string Message)>();

        Assert.That(() => Parser.SetLogger((level, message) => entries.Add((level, message))), Throws.Nothing);

        using var tree = Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM);

        Assert.Multiple(() =>
        {
            Assert.That(entries, Is.Not.Empty, "The logger should be invoked while parsing.");
            Assert.That(entries.Select(e => e.Message), Has.All.Not.Null, "Every logged message should be marshaled.");
            // Distinct() because Is.SubsetOf treats repeated values as extra items.
            Assert.That(entries.Select(e => e.Level).Distinct(), Is.SubsetOf(new[] { LogType.LogTypeParse, LogType.LogTypeLex }), "Only the two documented log types should be reported.");
        });
    }

    [Test]
    public void SetLogger_WithNullCallback_ShouldNotThrowException()
    {
        Assert.That(() => Parser.SetLogger(null), Throws.Nothing);
        Assert.That(() => Parser.ParseString(PascalGrammar.SAMPLE_PROGRAM).Dispose(), Throws.Nothing);
    }
}
