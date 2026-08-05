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

    // Query-source offsets are UTF-8 byte offsets, NOT document UTF-16 offsets: the
    // source is marshaled with LPUTF8Str. Dividing by sizeof(ushort) halves them.
    [Test]
    public void StartAndEndByteOffsetForPattern_ShouldReturnUtf8ByteOffsets()
    {
        // Two patterns; ASCII so byte offset == char index. Pattern 1 starts at the
        // second "(identifier)". Under the /2 bug Start(1) would be ~8 instead of 16.
        const string source = "(identifier) @a (identifier) @b";
        using var query = Language.CreateQuery(source);

        var pattern1Start = (uint)source.IndexOf("(identifier)", 1); // 16

        Assert.Multiple(() =>
        {
            Assert.That(query.StartByteOffsetForPattern(0), Is.EqualTo(0u),
                "Pattern 0 starts at byte 0.");
            Assert.That(query.StartByteOffsetForPattern(1), Is.EqualTo(pattern1Start),
                "Pattern 1 start must be its true UTF-8 byte offset, not the halved value.");
            Assert.That(query.EndByteOffsetForPattern(1), Is.EqualTo((uint)source.Length),
                "Pattern 1 ends at the end of the source; the /2 bug would halve this.");
        });
    }

    // A malformed query must report the error at the true UTF-8 byte offset of the
    // bad node type, not half of it.
    [Test]
    public void CreateQuery_WithInvalidNodeType_ErrorOffsetPointsAtBadToken()
    {
        const string source = "(identifier) @ok (nonexistent_node) @bad";
        var badTokenOffset = (uint)source.IndexOf("nonexistent_node"); // 18

        var ex = Assert.Throws<QueryException>(() => Language.CreateQuery(source));

        // tree-sitter anchors a NodeType error at the start of the offending name.
        // The /2 bug halves the byte offset to 9, pointing into the wrong pattern.
        Assert.That(ex!.ErrorOffset, Is.EqualTo(badTokenOffset),
            "ErrorOffset must be the true UTF-8 byte offset of the error, not the halved value.");
        Assert.That(ex.Error, Is.EqualTo(QueryError.QueryErrorNodeType));
    }
}
