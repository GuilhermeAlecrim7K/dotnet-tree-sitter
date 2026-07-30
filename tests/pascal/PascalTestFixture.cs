namespace TreeSitter.Pascal.Tests;

/// <summary>
/// Shared per-test fixture: a fresh Pascal <see cref="TreeSitter.Language"/> for every test,
/// plus a lazily created <see cref="TreeSitter.Parser"/> for the tests that need one.
/// </summary>
/// <remarks>
/// Tests that are themselves about construction may create their own instance instead;
/// everything else should use <see cref="Language"/> so the suite has a single setup strategy.
/// </remarks>
public abstract class PascalTestFixture
{
    private Parser? _parser;

    protected Language Language { get; private set; } = null!;

    protected Parser Parser => _parser ??= Language.CreateParser();

    [SetUp]
    public void SetUpFixture()
    {
        _parser = null;
        Language = new PascalLanguage();
    }

    [TearDown]
    public void TearDownFixture()
    {
        // Dispose in reverse creation order. Both implementations are idempotent, so a test
        // that already disposed either one is safe here.
        _parser?.Dispose();
        Language.Dispose();
    }
}
