namespace TreeSitter.Pascal.Tests;

/// <summary>
/// Sample sources and the facts the suite asserts about the pinned tree-sitter-pascal grammar.
/// </summary>
internal static class PascalGrammar
{
    public const string SAMPLE_PROGRAM = "program HelloWorld; begin end.";
    public const string SAMPLE_PROGRAM_S_EXPRESSION = "(root (program (kProgram) (moduleName (identifier)) (block (kBegin) (kEnd)) (kEndDot)))";

    public const string SAMPLE_UNIT = "unit Foo; interface end.";
    public const string SAMPLE_UNIT_S_EXPRESSION = "(root (unit (kUnit) (moduleName (identifier)) (interface (kInterface)) (kEnd) (kEndDot)))";

    // Carries a BMP non-ASCII character (é) so string round-trips exercise UTF-8 decoding
    // and UTF-16 char-offset math. Used by the marshaling tests.
    public const string NON_ASCII_PROGRAM = "program Café; begin end.";

    // The values below are properties of the grammar artifact currently pinned by the
    // tests/pascal/tree-sitter-pascal submodule, which is generated at ABI 14
    // (src/parser.c: #define LANGUAGE_VERSION 14). They must all be revisited together
    // if the grammar is ever regenerated at ABI 15.
    public const uint ABI_VERSION = 14;

    // ts_language_name returns NULL below ABI 15 — see lib/src/language.c, which gates the
    // name on abi_version >= LANGUAGE_VERSION_WITH_RESERVED_WORDS (= 15).
    public const string? NAME = null;

    public const uint STATE_COUNT = 2715;
    public const uint FIELD_COUNT = 38;
}
