namespace TreeSitter.Pascal.Tests;

public class LanguageTests : PascalTestFixture
{
    [Test]
    public void Constructor_WithPascalLanguage_ShouldReturnValidInstance()
    {
        // Owns its instance: this test is about construction itself, not about using a language.
        using var sut = new PascalLanguage();
        Assert.That(sut, Is.Not.Null);
    }

    [Test]
    public void Dispose_WithPascalLanguage_ShouldBeIdempotentAndInvalidateMembers()
    {
        // NOTE: ts_language_delete is a no-op for non-wasm grammars (lib/src/language.c), so this
        // asserts the managed disposal contract only — native release is not observable from here.
        Language.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(() => Language.Dispose(), Throws.Nothing, "Dispose should be idempotent.");
            Assert.That(() => Language.Name(), Throws.TypeOf<ObjectDisposedException>(), "Calling methods after disposal should throw ObjectDisposedException.");
            Assert.That(() => Language.AbiVersion(), Throws.TypeOf<ObjectDisposedException>(), "Calling methods after disposal should throw ObjectDisposedException.");
        });
    }

    [Test]
    public void Copy_WithPascalLanguage_ShouldReturnDistinctWrapperOverSameNativeLanguage()
    {
        using var copy = Language.Copy();
        Assert.That(copy, Is.Not.Null);

        // ts_language_copy is the identity function for non-wasm grammars (lib/src/language.c:6-11):
        // it returns the very same TSLanguage pointer. So Copy() yields a distinct *managed* wrapper
        // over the *same* native language, and Language.Equals — which compares pointers — is true.
        // NOTE: Language.Equals(Language?) does not override object.Equals, so Is.EqualTo would
        // disagree with .Equals here. Asserted via .Equals deliberately.
        Assert.Multiple(() =>
        {
            Assert.That(copy, Is.Not.SameAs(Language), "Copy should return a distinct managed wrapper.");
            Assert.That(copy, Is.InstanceOf<PascalLanguage>(), "Copy should preserve the concrete wrapper type.");
            Assert.That(copy.Equals(Language), Is.True, "The copy should point at the same native language.");
            Assert.That(Language.Equals(copy), Is.True, "Pointer equality should be symmetric.");
        });
    }

    [Test]
    public void Name_WithPascalLanguage_ShouldReturnExpectedValue()
    {
        Assert.That(Language.Name(), Is.EqualTo(PascalGrammar.NAME));
    }

    [Test]
    public void AbiVersion_WithPascalLanguage_ShouldReturnExpectedValue()
    {
        Assert.That(Language.AbiVersion(), Is.EqualTo(PascalGrammar.ABI_VERSION));
    }

    [Test]
    public void Symbol_WithUnknownName_ShouldReturnZeroSentinel()
    {
        // ts_language_symbol_for_name returns 0 when the name is not found (lib/src/language.c).
        // Beware: 0 is *also* a valid symbol id — ts_builtin_sym_end (lib/src/parser.h:13), whose
        // name is "end". So 0 means "not found" on the way in, but "end" on the way out; the two
        // are indistinguishable through this API.
        Assert.Multiple(() =>
        {
            Assert.That(Language.Symbol("foobar", true), Is.EqualTo(ushort.MinValue));
            Assert.That(Language.Symbol("foobar", false), Is.EqualTo(ushort.MinValue));
        });
    }

    [Test]
    [Ignore("I do not understand how symbols are supposed to work yet. Maybe when I dive deeper into the other classes it will become more clear.")]
    public void SymbolName_ForEverySymbolId_ShouldRoundTripThroughSymbol()
    {
        var symbolCount = Language.SymbolCount();
        Assert.That(symbolCount, Is.LessThan(ushort.MaxValue - 1), "Symbol count should be less than ushort.MaxValue to avoid overflow.");

        // ts_language_symbol_name special-cases these two above the grammar's own range.
        Assert.Multiple(() =>
        {
            Assert.That(Language.SymbolName(ushort.MaxValue), Is.EqualTo("ERROR"), "ts_builtin_sym_error should be named 'ERROR'.");
            Assert.That(Language.SymbolName((ushort)(ushort.MaxValue - 1)), Is.EqualTo("_ERROR"), "ts_builtin_sym_error_repeat should be named '_ERROR'.");
        });

        Assert.Multiple(() =>
        {
            // Symbol ids are 0-based: ts_language_symbol_name accepts any id < symbolCount.
            var symbols = new List<(string name, ushort id)>();
            for (ushort i = 0; i < symbolCount; i++)
            {
                var name = Language.SymbolName(i);
                Assert.That(name, Is.Not.Null, $"Symbol id {i} should have a name.");
                if (name is not null)
                    symbols.Add((name, i));
            }
            Assert.That(symbols, Has.Count.EqualTo((int)symbolCount), "Every symbol id below the count should have a name.");

            var getMessage = (ushort namedId, ushort unnamedId) =>
            {
                if (unnamedId != ushort.MinValue && namedId != ushort.MinValue)
                    return $"Named({namedId}): {Language.SymbolName(namedId)}. Unnamed({unnamedId}): {Language.SymbolName(unnamedId)}";
                else if (unnamedId != ushort.MinValue)
                    return $"Unnamed({unnamedId}): {Language.SymbolName(unnamedId)}";
                else
                    return $"Named({namedId}): {Language.SymbolName(namedId)}";
            };
            foreach (var item in symbols)
            {
                // Symbol id 0 cannot be round-tripped: a lookup that resolves to it is
                // indistinguishable from a failed lookup. See the note on the sentinel above.
                if (item.id == ushort.MinValue)
                    continue;

                var unnamedId = Language.Symbol(item.name, false);
                var namedId = Language.Symbol(item.name, true);
                if (unnamedId == ushort.MinValue && namedId == ushort.MinValue)
                {
                    Assert.Fail($"Symbol '{item.name}' of type {Language.SymbolType(item.id)} should have a valid id. Owner: {Language.SymbolName(item.id)}");
                    continue;
                }

                if (item.id != unnamedId && item.id != namedId)
                    Assert.Fail($"Symbol '{item.name}' id {item.id} mismatch. {getMessage(namedId, unnamedId)}");
            }

            // Bounded probes above the grammar's range instead of sweeping all ~65k ids: the two
            // built-in ids asserted above are the only named ones up there.
            foreach (var id in OutOfRangeProbes(symbolCount, ushort.MaxValue - 2))
                Assert.That(Language.SymbolName(id), Is.Null, $"Symbol id {id} is not below {symbolCount} and should not have a name.");
        });
    }

    [Test]
    public void FieldName_ForEveryFieldId_ShouldRoundTripThroughFieldId()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Language.FieldName(0), Is.Null, "Field id 0 should be null.");
            Assert.That(Language.FieldName(ushort.MaxValue), Is.Null, "Field id ushort.MaxValue should be null.");
        });

        var fieldCount = Language.FieldCount();
        Assert.That(fieldCount, Is.EqualTo(PascalGrammar.FIELD_COUNT));

        Assert.Multiple(() =>
        {
            // Field ids are 1-based: id 0 is reserved (asserted null above).
            var fields = new List<(string name, ushort id)>();
            for (ushort i = 1; i <= fieldCount; i++)
            {
                var name = Language.FieldName(i);
                Assert.That(name, Is.Not.Null, $"Field id {i} should have a name.");
                if (name is not null)
                    fields.Add((name, i));
            }
            Assert.That(fields, Has.Count.EqualTo((int)fieldCount), "Every field id up to the count should have a name.");

            foreach (var item in fields)
            {
                var id = Language.FieldId(item.name);
                if (id == ushort.MinValue)
                {
                    Assert.Fail($"Field '{item.name}' should have a valid id. Owner: {Language.FieldName(item.id)}");
                    continue;
                }

                if (item.id != id)
                    Assert.Fail($"Field '{item.name}' id {item.id} mismatch. Id got: {id}");
            }

            // Bounded probes above the grammar's range instead of sweeping all ~65k ids.
            foreach (var id in OutOfRangeProbes(fieldCount + 1, ushort.MaxValue))
                Assert.That(Language.FieldName(id), Is.Null, $"Field id {id} is higher than {fieldCount} and should not have a name.");
        });
    }

    [Test]
    public void StateCount_WithPascalLanguage_ShouldReturnExpectedValue()
    {
        Assert.That(Language.StateCount(), Is.EqualTo(PascalGrammar.STATE_COUNT));
    }

    [Test]
    public void CreateParser_WithPascalLanguage_ShouldReturnValidParser()
    {
        using var parser = Language.CreateParser();
        Assert.That(parser, Is.Not.Null);
        Assert.That(parser.Language, Is.SameAs(Language), "Parser's language should be the same instance as the one used to create it.");
    }

    [Test]
    public void CreateQuery_WithPascalLanguage_ShouldReturnValidQuery()
    {
        using var query = Language.CreateQuery("(identifier) @id");
        Assert.That(query, Is.Not.Null);
    }

    /// <summary>
    /// A handful of ids at or above <paramref name="firstInvalid"/> and no higher than
    /// <paramref name="lastProbe"/>, used in place of an exhaustive sweep over the whole
    /// <see cref="ushort"/> range.
    /// </summary>
    private static IEnumerable<ushort> OutOfRangeProbes(uint firstInvalid, uint lastProbe)
    {
        var candidates = new[] { firstInvalid, firstInvalid + 1, firstInvalid + 1000, lastProbe / 2, lastProbe };
        return candidates.Where(id => id >= firstInvalid && id <= lastProbe).Select(id => (ushort)id).Distinct();
    }
}
