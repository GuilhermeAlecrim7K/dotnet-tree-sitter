namespace TreeSitter.Pascal.Tests;

public class QueryTests : PascalTestFixture
{
    // Query source is UTF-8: a quoted string literal carrying non-ASCII bytes must
    // round-trip through StringValueForId intact. Under PtrToStringAnsi the bytes
    // C3 A9 ("é") decode via the OS ANSI code page and the value is corrupted.
    [Test]
    public void StringValueForId_WithNonAsciiLiteral_ShouldRoundTripAsUtf8()
    {
        using var query = Language.CreateQuery("((identifier) @id (#eq? @id \"café\"))");

        // Contains, not index equality: the string table also holds the predicate
        // name ("eq?"), and the ordering of entries is not guaranteed.
        var values = new List<string?>();
        for (uint i = 0; i < query.StringCount(); i++)
            values.Add(query.StringValueForId(i));

        Assert.That(values, Does.Contain("café"), "A non-ASCII query string literal should be marshaled as UTF-8.");
    }

    [Test]
    public void CaptureNameForId_WithAsciiCapture_ShouldReturnCaptureName()
    {
        // Capture names are ASCII-only (the query lexer rejects non-ASCII), so this is
        // coverage for the CaptureNameForId marshaling path, not a non-ASCII round-trip.
        using var query = Language.CreateQuery("(identifier) @id");
        Assert.That(query.CaptureNameForId(0), Is.EqualTo("id"));
    }
}
