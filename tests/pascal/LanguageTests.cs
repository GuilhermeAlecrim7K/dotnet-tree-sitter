namespace TreeSitter.Pascal.Tests;

public class LanguageTests
{
    [Test]
    public void Constructor_WithPascalLanguage_ShouldReturnValidInstance()
    {
        using var sut = new PascalLanguage();
        Assert.That(sut, Is.Not.Null);
    }

    [Test]
    public void Dispose_WithPascalLanguage_ShouldNotThrowException()
    {
        using var sut = new PascalLanguage();
        Assert.That(() => sut.Dispose(), Throws.Nothing);

        Assert.That(() => sut.Name(), Throws.TypeOf<ObjectDisposedException>(), "Calling methods after disposal should throw ObjectDisposedException.");

        Assert.That(() => sut.Dispose(), Throws.Nothing);
    }

    [Test]
    public void Copy_WithPascalLanguage_ShouldReturnNewInstanceOfTheSameType()
    {
        using var sut = new PascalLanguage();
        using var copy = sut.Copy();
        Assert.That(copy, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(copy, Is.Not.SameAs(sut));
            Assert.That(copy, Is.InstanceOf<PascalLanguage>());
            Assert.That(copy.Equals(sut), Is.True, "Copied instance should be equal to the original.");
        });
    }

    [Test]
    public void Name_WithPascalLanguage_ShouldReturnPascal()
    {
        using var sut = new PascalLanguage();
        Assert.That(sut.Name(), Is.EqualTo("pascal"));
    }

    [Test]
    public void AbiVersion_WithPascalLanguage_ShouldReturnExpectedValue()
    {
        using var sut = new PascalLanguage();
        Assert.That(sut.AbiVersion(), Is.EqualTo(15));
    }

    [Test]
    public void Symbol_WithInvalidName_ShouldReturnInvalidId()
    {
        using var sut = new PascalLanguage();
        Assert.That(sut.Symbol("foobar", true), Is.EqualTo(ushort.MinValue));
        Assert.That(sut.Symbol("foobar", false), Is.EqualTo(ushort.MinValue));
    }

    [Test]
    [Ignore("I do not understand how symbols are supposed to work yet. Maybe when I dive deeper into the other classes it will become more clear.")]
    public void WhenMatchingSymbolsNamesAndIds_ShouldReturnExpectedResults()
    {
        using var sut = new PascalLanguage();

        // NOTE: Apparently reserved symbols
        Assert.That(sut.SymbolName(0), Is.EqualTo("end"), "Symbol id 0 should be 'end' for some reason.");
        Assert.That(sut.SymbolName(ushort.MaxValue), Is.EqualTo("ERROR"), "Symbol id ushort.MaxValue should 'ERROR'.");
        Assert.That(sut.SymbolName(ushort.MaxValue - 1), Is.EqualTo("_ERROR"), "Symbol id ushort.MaxValue -1 should not have a name.");

        var symbolCount = sut.SymbolCount();
        Assert.That(symbolCount, Is.LessThan(ushort.MaxValue - 1), "Symbol count should be less than ushort.MaxValue to avoid overflow.");

        Assert.Multiple(() =>
        {
            var symbols = new List<(string name, ushort id)>();
            for (ushort i = 1; i < symbolCount; i++)
            {
                var name = sut.SymbolName(i);
                Assert.That(name, Is.Not.Null, $"Symbol id {i} should have a name.");
                if (name is not null)
                    symbols.Add((name, i));
            }
            Assert.That(symbolCount, Is.EqualTo((ushort)symbols.Count), "Symbol count should match the number of valid symbol names found.");

            var getMessage = (ushort namedId, ushort unnamedId) =>
            {
                if (unnamedId != ushort.MinValue && namedId != ushort.MinValue)
                    return $"Named({namedId}): {sut.SymbolName(namedId)}. Unnamed({unnamedId}): {sut.SymbolName(unnamedId)}";
                else if (unnamedId != ushort.MinValue)
                    return $"Unnamed({unnamedId}): {sut.SymbolName(unnamedId)}";
                else
                    return $"Named({namedId}): {sut.SymbolName(namedId)}";
            };
            foreach (var item in symbols)
            {
                var unnamedId = sut.Symbol(item.name, false);
                var namedId = sut.Symbol(item.name, true);
                if (unnamedId == ushort.MinValue && namedId == ushort.MinValue)
                {
                    Assert.Fail($"Symbol '{item.name}' of type {sut.SymbolType(item.id)} should have a valid id. Owner: {sut.SymbolName(item.id)}");
                    continue;
                }

                if (item.id != unnamedId && item.id != namedId)
                    Assert.Fail($"Symbol '{item.name}' id {item.id} mismatch. {getMessage(namedId, unnamedId)}");
            }

            for (ushort i = (ushort)symbolCount; i < ushort.MaxValue - 1; i++)
                Assert.That(sut.SymbolName(i), Is.Null, $"Symbol id {i} is higher than {nameof(symbolCount)} and should not have a name.");
        });
    }

    [Test]
    public void WhenMatchingFieldNamesAndIds_ShouldReturnExpectedResults()
    {
        using var sut = new PascalLanguage();

        Assert.That(sut.FieldName(0), Is.Null, "Field id 0 should be null.");
        Assert.That(sut.FieldName(ushort.MaxValue), Is.Null, "Field id ushort.MaxValue should be null.");

        var fieldCount = sut.FieldCount();
        Assert.That(fieldCount, Is.EqualTo(36), "Field count should be 36 for Pascal language.");

        Assert.Multiple(() =>
        {
            var fields = new List<(string name, ushort id)>();
            for (ushort i = 1; i <= fieldCount; i++)
            {
                var name = sut.FieldName(i);
                Assert.That(name, Is.Not.Null, $"Field id {i} should have a name.");
                if (name is not null)
                    fields.Add((name, i));
            }
            Assert.That(fieldCount, Is.EqualTo((ushort)fields.Count), "Field count should match the number of valid field names found.");

            foreach (var item in fields)
            {
                var id = sut.FieldId(item.name);
                if (id == ushort.MinValue)
                {
                    Assert.Fail($"Field '{item.name}' should have a valid id. Owner: {sut.FieldName(item.id)}");
                    continue;
                }

                if (item.id != id)
                    Assert.Fail($"Field '{item.name}' id {item.id} mismatch. Id got: {id}");
            }

            for (ushort i = (ushort)(fieldCount + 1); i < ushort.MaxValue; i++)
                Assert.That(sut.FieldName(i), Is.Null, $"Field id {i} is higher than {nameof(fieldCount)} and should not have a name.");
        });
    }

    [Test]
    public void StateCount_WithPascalLanguage_ShouldReturnExpectedValue()
    {
        using var sut = new PascalLanguage();
        Assert.That(sut.StateCount(), Is.EqualTo(2105), "State count should be 2105 for Pascal language.");
    }

    [Test]
    public void CreateParser_WithPascalLanguage_ShouldReturnValidParser()
    {
        using var sut = new PascalLanguage();
        using var parser = sut.CreateParser();
        Assert.That(parser, Is.Not.Null);
        Assert.That(parser.Language, Is.SameAs(sut), "Parser's language should be the same instance as the one used to create it.");
    }

    [Test]
    public void CreateQuery_WithPascalLanguage_ShouldReturnValidQuery()
    {
        using var sut = new PascalLanguage();
        using var query = sut.CreateQuery("(identifier) @id");
        Assert.That(query, Is.Not.Null);
    }
}