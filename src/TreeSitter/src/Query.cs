using System.Runtime.InteropServices;

namespace TreeSitter;

public sealed class Query : IDisposable
{
    private readonly IntPtr _pointer;
    private bool _disposed = false;
    private readonly string _source;

    internal IntPtr Pointer
    {
        get
        {
            ThrowIfDisposed();
            return _pointer;
        }
    }

    internal Query(IntPtr languagePointer, string source)
    {
        _source = source;
        _pointer = Binding.ts_query_new(languagePointer, source, (uint)System.Text.Encoding.UTF8.GetByteCount(source), out var errorOffset, out var errorType);

        if (_pointer == IntPtr.Zero)
            throw new QueryException(errorOffset / sizeof(ushort), errorType);
    }

    ~Query()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources if any.
        }

        Binding.ts_query_delete(_pointer);
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Query));
    }

    public QueryCursor CreateCursor()
    {
        ThrowIfDisposed();
        return new QueryCursor(this);
    }

    public uint PatternCount()
    {
        ThrowIfDisposed();
        return Binding.ts_query_pattern_count(_pointer);
    }

    public uint CaptureCount()
    {
        ThrowIfDisposed();
        return Binding.ts_query_capture_count(_pointer);
    }

    public uint StringCount()
    {
        ThrowIfDisposed();
        return Binding.ts_query_string_count(_pointer);
    }

    public uint StartByteOffsetForPattern(uint patternIndex)
    {
        ThrowIfDisposed();
        return Binding.ts_query_start_byte_for_pattern(_pointer, patternIndex) / sizeof(ushort);
    }

    public uint EndByteOffsetForPattern(uint patternIndex)
    {
        ThrowIfDisposed();
        return Binding.ts_query_end_byte_for_pattern(_pointer, patternIndex) / sizeof(ushort);
    }

    public QueryPredicateStep[] PredicatesForPattern(uint patternIndex)
    {
        ThrowIfDisposed();
        return Binding.ts_query_predicates_for_pattern(_pointer, patternIndex, out _);
    }

    public bool IsPatternRooted(uint patternIndex)
    {
        ThrowIfDisposed();
        return Binding.ts_query_is_pattern_rooted(_pointer, patternIndex);
    }

    public bool IsPatternNonLocal(uint patternIndex)
    {
        ThrowIfDisposed();
        return Binding.ts_query_is_pattern_non_local(_pointer, patternIndex);
    }

    public bool IsPatternGuaranteedAtOffset(uint offset)
    {
        ThrowIfDisposed();
        return Binding.ts_query_is_pattern_guaranteed_at_step(_pointer, offset / sizeof(ushort));
    }

    public string? CaptureNameForId(uint id)
    {
        ThrowIfDisposed();
        return Marshal.PtrToStringAnsi(Binding.ts_query_capture_name_for_id(_pointer, id, out _));
    }

    public Quantifier CaptureQuantifierForId(uint patternId, uint captureId)
    {
        ThrowIfDisposed();
        return Binding.ts_query_capture_quantifier_for_id(_pointer, patternId, captureId);
    }

    public string? StringValueForId(uint id)
    {
        ThrowIfDisposed();
        return Marshal.PtrToStringAnsi(Binding.ts_query_string_value_for_id(_pointer, id, out _));
    }

    public void DisableCapture(string captureName)
    {
        ThrowIfDisposed();
        Binding.ts_query_disable_capture(_pointer, captureName, (uint)captureName.Length);
    }

    public void DisablePattern(uint patternIndex)
    {
        ThrowIfDisposed();
        Binding.ts_query_disable_pattern(_pointer, patternIndex);
    }
}