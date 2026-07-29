using System.Runtime.InteropServices;

namespace TreeSitter;

public sealed class QueryCursor : IDisposable
{
    private readonly IntPtr _pointer;
    private readonly Query _query;
    private bool _disposed = false;

    internal QueryCursor(Query query)
    {
        _pointer = Binding.ts_query_cursor_new();
        _query = query;
    }

    ~QueryCursor()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources if any.
        }

        Binding.ts_query_cursor_delete(_pointer);
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(QueryCursor));
    }

    public void Exec(Node node)
    {
        ThrowIfDisposed();
        Binding.ts_query_cursor_exec(_pointer, _query.Pointer, node.NativeNode);
    }

    public bool DidExceedMatchLimit()
    {
        ThrowIfDisposed();
        return Binding.ts_query_cursor_did_exceed_match_limit(_pointer);
    }

    public uint MatchLimit()
    {
        ThrowIfDisposed();
        return Binding.ts_query_cursor_match_limit(_pointer);
    }

    public void SetMatchLimit(uint limit)
    {
        ThrowIfDisposed();
        Binding.ts_query_cursor_set_match_limit(_pointer, limit);
    }

    public void SetByteRange(uint startByte, uint endByte)
    {
        ThrowIfDisposed();
        Binding.ts_query_cursor_set_byte_range(_pointer, startByte * sizeof(ushort), endByte * sizeof(ushort));
    }

    public void SetPointRange(Point start, Point end)
    {
        ThrowIfDisposed();
        Binding.ts_query_cursor_set_point_range(_pointer, start, end);
    }

    public QueryMatch? NextMatch()
    {
        ThrowIfDisposed();
        if (!Binding.ts_query_cursor_next_match(_pointer, out var nativeMatch))
            return null;

        var match = new QueryMatch(nativeMatch.PatternIndex, new QueryCapture[nativeMatch.CaptureCount]);

        for (var n = 0; n < nativeMatch.CaptureCount; n++)
        {
            var intPtr = nativeMatch.Captures + Marshal.SizeOf(typeof(Binding.QueryCapture)) * n;
            var nativeCapture = Marshal.PtrToStructure<Binding.QueryCapture>(intPtr);

            var node = Node.FromNative(nativeCapture.Node);
            if (node is null)
                throw new InvalidOperationException("Failed to retrieve node for capture.");

            match.Captures[n] = new QueryCapture(nativeCapture.Index, node);
        }

        return match;
    }

    public void RemoveMatch(uint id)
    {
        ThrowIfDisposed();
        Binding.ts_query_cursor_remove_match(_pointer, id);
    }

    public QueryCapture? NextCapture()
    {
        ThrowIfDisposed();
        if (!Binding.ts_query_cursor_next_capture(_pointer, out var nativeMatch, out var captureIndex))
            return null;

        var intPtr = nativeMatch.Captures + Marshal.SizeOf(typeof(Binding.QueryCapture)) * (ushort)captureIndex;
        var nativeCapture = Marshal.PtrToStructure<Binding.QueryCapture>(intPtr);
        var node = Node.FromNative(nativeCapture.Node);
        if (node is null)
            throw new InvalidOperationException("Failed to retrieve node for capture.");

        return new QueryCapture(nativeCapture.Index, node);
    }
}