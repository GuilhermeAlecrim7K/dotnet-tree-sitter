namespace TreeSitter;

public sealed class Parser : IDisposable
{
    private IntPtr Ptr { get; set; }
    private readonly Language _language;

    public Language Language => _language;

    internal Parser(Language language, IntPtr languagePointer)
    {
        Ptr = Binding.ts_parser_new();
        if (Ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create a new parser instance.");
        if (!Binding.ts_parser_set_language(Ptr, languagePointer))
            throw new InvalidOperationException("Failed to set the language for the parser.");
        _language = language;
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            Binding.ts_parser_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public bool SetIncludedRanges(Range[] ranges) => Binding.ts_parser_set_included_ranges(Ptr, ranges, (uint)ranges.Length);

    public Range[] IncludedRanges() => Binding.ts_parser_included_ranges(Ptr, out _);

    public Tree ParseString(string source, Tree? oldTree = null)
    {
        var ptr = Binding.ts_parser_parse_string_encoding(Ptr, oldTree?.Ptr ?? IntPtr.Zero,
            source, (uint)source.Length * 2, InputEncoding.InputEncodingUTF16LE);
        return ptr != IntPtr.Zero ? new Tree(ptr, _language) : throw new InvalidOperationException("Failed to parse the source into a tree.");
    }

    public void Reset() => Binding.ts_parser_reset(Ptr);

    private Binding.LogCallback? _logCallbackKeepAliveRef;

    public void SetLogger(Logger logger)
    {
        _logCallbackKeepAliveRef = null;
        if (logger is null)
            return;

        _logCallbackKeepAliveRef = new Binding.LogCallback((_, type, message) => logger(type, message));
        var data = new Binding.LoggerData
        {
            Log = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(_logCallbackKeepAliveRef),
        };
        Binding.ts_parser_set_logger(Ptr, data);
    }
}