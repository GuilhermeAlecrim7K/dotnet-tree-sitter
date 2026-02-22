using System.Runtime.InteropServices;
using System.Text;

namespace TreeSitter;

public abstract class Language : IDisposable
{
    private IntPtr _pointer;
    private bool _disposed = false;

    internal IntPtr Pointer
    {
        get
        {
            ThrowIfDisposed();
            return _pointer;
        }
    }

    protected Language(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
            throw new ArgumentNullException(nameof(pointer));
        _pointer = pointer;
        // TODO: Must find a way to validate the pointer.
    }

    ~Language()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources if any.
        }

        // Dispose unmanaged resources.
        Binding.ts_language_delete(_pointer);
        _pointer = IntPtr.Zero;
        _disposed = true;
    }

    public Language Copy()
    {
        ThrowIfDisposed();
        var newPointer = Binding.ts_language_copy(_pointer);
        if (newPointer == IntPtr.Zero)
            throw new InvalidOperationException("Failed to copy the language instance. The native function returned a null pointer.");

        var copy = this.MemberwiseClone() as Language;
        copy!._pointer = newPointer;
        return copy;
    }

    public bool Equals(Language? other)
    {
        ThrowIfDisposed();
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return _pointer == other._pointer;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public string? Name()
    {
        ThrowIfDisposed();
        return Marshal.PtrToStringUTF8(Binding.ts_language_name(_pointer));
    }

    public uint AbiVersion()
    {
        ThrowIfDisposed();
        return Binding.ts_language_abi_version(_pointer);
    }

    public LanguageMetadata Metadata()
    {
        ThrowIfDisposed();
        return Binding.ts_language_metadata(_pointer);
    }

    public uint SymbolCount()
    {
        ThrowIfDisposed();
        return Binding.ts_language_symbol_count(_pointer);
    }

    public ushort Symbol(string name, bool isNamed)
    {
        ThrowIfDisposed();
        return Binding.ts_language_symbol_for_name(_pointer, name, (uint)Encoding.UTF8.GetByteCount(name), isNamed);
    }

    public string? SymbolName(ushort symbol)
    {
        ThrowIfDisposed();
        return Marshal.PtrToStringUTF8(Binding.ts_language_symbol_name(_pointer, symbol));
    }

    public SymbolType SymbolType(ushort symbol)
    {
        ThrowIfDisposed();
        return Binding.ts_language_symbol_type(_pointer, symbol);
    }

    public uint FieldCount()
    {
        ThrowIfDisposed();
        return Binding.ts_language_field_count(_pointer);
    }

    public string? FieldName(ushort field)
    {
        ThrowIfDisposed();
        return Marshal.PtrToStringUTF8(Binding.ts_language_field_name_for_id(_pointer, field));
    }

    public ushort FieldId(string name)
    {
        ThrowIfDisposed();
        return Binding.ts_language_field_id_for_name(_pointer, name, (uint)Encoding.UTF8.GetByteCount(name));
    }

    public uint StateCount()
    {
        ThrowIfDisposed();
        return Binding.ts_language_state_count(_pointer);
    }

    public Parser CreateParser()
    {
        ThrowIfDisposed();
        return new Parser(this);
    }

    public Query CreateQuery(string source)
    {
        ThrowIfDisposed();
        return new Query(_pointer, source);
    }

}