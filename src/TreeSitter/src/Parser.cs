namespace TreeSitter;

public sealed class Parser : IDisposable
{
    private IntPtr _pointer;
    private readonly Language _language;
    private bool _disposed = false;
    private Binding.LogCallback? _logCallbackKeepAliveRef;

    public Language Language
    {
        get
        {
            ThrowIfDisposed();
            return _language;
        }
    }

    internal Parser(Language language)
    {
        _pointer = Binding.ts_parser_new();
        if (_pointer == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create a new parser instance.");
        if (!Binding.ts_parser_set_language(_pointer, language.Pointer))
            throw new InvalidOperationException("Failed to set the language for the parser.");
        _language = language;
    }

    ~Parser()
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
            SetLogger(null);
        }

        Binding.ts_parser_set_logger(_pointer, new Binding.LoggerData { Log = IntPtr.Zero});
        Binding.ts_parser_delete(_pointer);
        _pointer = IntPtr.Zero;
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Parser));
    }

    public Tree ParseString(string source, Tree? oldTree = null)
    {
        ThrowIfDisposed();
        var ptr = Binding.ts_parser_parse_string_encoding(_pointer, oldTree?.Ptr ?? IntPtr.Zero,
            source, (uint)source.Length * 2, InputEncoding.InputEncodingUTF16LE);
        return ptr != IntPtr.Zero ? new Tree(ptr, _language) : throw new InvalidOperationException("Failed to parse the source into a tree.");
    }

    public void Reset()
    {
        ThrowIfDisposed();
        Binding.ts_parser_reset(_pointer);
    }

    public void SetLogger(Logger? logger)
    {
        ThrowIfDisposed();
        if (logger is null)
        {
            Binding.ts_parser_set_logger(_pointer, new Binding.LoggerData { Log = IntPtr.Zero});
            _logCallbackKeepAliveRef = null;
            return;
        }

        // TODO: What is payload and what should it be used for?
        _logCallbackKeepAliveRef = new Binding.LogCallback((_, type, message) => logger(type, message));
        var data = new Binding.LoggerData
        {
            Log = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(_logCallbackKeepAliveRef),
        };
        Binding.ts_parser_set_logger(_pointer, data);
    }
}