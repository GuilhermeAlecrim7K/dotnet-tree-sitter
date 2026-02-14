using System.Runtime.InteropServices;

namespace TreeSitter;

public class Language : IDisposable
{
    internal readonly string[] Symbols;
    internal readonly string[] Fields;
    internal readonly Dictionary<string, ushort> FieldIds;

    internal IntPtr Ptr;

    protected Language(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            throw new ArgumentNullException(nameof(ptr));
        Ptr = ptr;

        // NOTE: Why +1? Copilot suggested: Because the count is zero-based, but we want to include the last one? Wasn't able to verify this yet.
        var symbolCount = Binding.ts_language_symbol_count(Ptr) + 1;
        Symbols = new string[symbolCount];

        // HACK: On json, 25 was null
        for (ushort i = 0; i < Symbols.Length -1; i++)
            Symbols[i] = Marshal.PtrToStringAnsi(Binding.ts_language_symbol_name(Ptr, i)) ?? throw new InvalidOperationException($"Wasn't expecting null symbol name for id {i}");

        var fieldCount = (int)Binding.ts_language_field_count(Ptr) + 1;
        Fields = new string[fieldCount + 1];
        FieldIds = new Dictionary<string, ushort>();

        // HACK: On json, 0 was null and 3 was null
        for (ushort i = 1; i < Fields.Length -1; i++)
        {
            Fields[i] = Marshal.PtrToStringAnsi(Binding.ts_language_field_name_for_id(Ptr, i)) ?? throw new InvalidOperationException($"Wasn't expecting null field name for id {i}");
            if (Fields[i] != null)
                if (!FieldIds.TryAdd(Fields[i], i))
                    throw new InvalidOperationException($"Wasn't expecting duplicate field name {Fields[i]} for id {i}");
        }
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            Binding.ts_language_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public string SymbolName(ushort symbol) => symbol != ushort.MaxValue ? Symbols[symbol] : "ERROR";
    public ushort SymbolForName(string str, bool isNamed) => Binding.ts_language_symbol_for_name(Ptr, str, (uint)str.Length, isNamed);
    public ushort FieldIdForName(string str) => FieldIds.GetValueOrDefault(str, (ushort)0);
    public SymbolType SymbolType(ushort symbol) => Binding.ts_language_symbol_type(Ptr, symbol);

}