using System.Runtime.InteropServices;

namespace TreeSitter;

public sealed class QueryException(uint errorOffset, QueryError error) : Exception
{
    public uint ErrorOffset { get; } = errorOffset;
    public QueryError Error { get; } = error;
}

public sealed class QueryCapture(uint index, Node node)
{
    public readonly uint Index = index;
    public readonly Node Node = node;
}

public sealed class QueryMatch(ushort index, QueryCapture[] captures)
{
    public readonly ushort Index = index;
    public readonly QueryCapture[] Captures = captures;
}

public sealed class Query : IDisposable
{
    private IntPtr _pointer;

    internal Query(IntPtr languagePointer, string source)
    {
        _pointer = Binding.ts_query_new(languagePointer, source, (uint)source.Length, out var errorOffset, out var errorType);

        if (_pointer == IntPtr.Zero)
        {
            throw new QueryException(errorOffset / sizeof(ushort), errorType);
        }
    }

    public void Dispose()
    {
        if (_pointer != IntPtr.Zero)
        {
            Binding.ts_query_delete(_pointer);
            _pointer = IntPtr.Zero;
        }
    }

    public QueryCursor Exec(Node node)
    {
        var cursor = new QueryCursor(this, node.Tree);
        Binding.ts_query_cursor_exec(cursor.Ptr, _pointer, node.NativeNode);
        return cursor;
    }

    public uint PatternCount() => Binding.ts_query_pattern_count(_pointer);

    public uint CaptureCount() => Binding.ts_query_capture_count(_pointer);

    public uint StringCount() => Binding.ts_query_string_count(_pointer);

    public uint StartOffsetForPattern(uint patternIndex) => Binding.ts_query_start_byte_for_pattern(_pointer, patternIndex) / sizeof(ushort);

    public QueryPredicateStep[] PredicatesForPattern(uint patternIndex) => Binding.ts_query_predicates_for_pattern(_pointer, patternIndex, out _);

    public bool IsPatternRooted(uint patternIndex) => Binding.ts_query_is_pattern_rooted(_pointer, patternIndex);

    public bool IsPatternNonLocal(uint patternIndex) => Binding.ts_query_is_pattern_non_local(_pointer, patternIndex);

    public bool IsPatternGuaranteedAtOffset(uint offset) => Binding.ts_query_is_pattern_guaranteed_at_step(_pointer, offset / sizeof(ushort));

    public string? CaptureNameForId(uint id) => Marshal.PtrToStringAnsi(Binding.ts_query_capture_name_for_id(_pointer, id, out _));

    public Quantifier CaptureQuantifierForId(uint patternId, uint captureId) => Binding.ts_query_capture_quantifier_for_id(_pointer, patternId, captureId);

    public string? StringValueForId(uint id) => Marshal.PtrToStringAnsi(Binding.ts_query_string_value_for_id(_pointer, id, out _));

    public void DisableCapture(string captureName) => Binding.ts_query_disable_capture(_pointer, captureName, (uint)captureName.Length);

    public void DisablePattern(uint patternIndex) => Binding.ts_query_disable_pattern(_pointer, patternIndex);
}

public static class QueryUtils
{
    public static IEnumerable<QueryMatch> Matches(this Query query, Node node)
    {
        // TODO: Memory leak?
        var cursor = query.Exec(node);
        while (cursor.NextMatch() is { } match)
            yield return match;
    }

    public static IEnumerable<QueryCapture> Captures(this Query query, Node node)
    {
        // TODO: Memory leak?
        var cursor = query.Exec(node);
        while (cursor.NextCapture() is { } capture)
            yield return capture;
    }

    public static QueryCapture? ByIndex(this IEnumerable<QueryCapture> captures, uint index) =>
        captures.FirstOrDefault(x => x.Index == index);

    public static QueryMatch? ByIndex(this IEnumerable<QueryMatch> matches, uint index) =>
        matches.FirstOrDefault(x => x.Index == index);

    public static IEnumerable<Node> CapturedNodes(this Query query, Node node) =>
        query.Captures(node).Select(x => x.Node);
}