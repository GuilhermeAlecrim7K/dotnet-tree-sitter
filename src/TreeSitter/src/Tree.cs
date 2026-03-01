namespace TreeSitter;

public sealed class Tree : IDisposable
{
    private IntPtr _pointer;
    private bool _disposed = false;
    public Language Language { get; }

    internal IntPtr Pointer
    {
        get
        {
            ThrowIfDisposed();
            return _pointer;
        }
    }

    internal Tree(IntPtr ptr, Language language)
    {
        _pointer = ptr;
        Language = language;
    }

    ~Tree()
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

        Binding.ts_tree_delete(_pointer);
        _pointer = IntPtr.Zero;
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Tree));
    }


    public Tree Copy()
    {
        ThrowIfDisposed();
        var ptr = Binding.ts_tree_copy(_pointer);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to copy tree.");
        return new Tree(ptr, Language);
    }

    public Node RootNode()
    {
        ThrowIfDisposed();
        return Node.FromNative(Binding.ts_tree_root_node(_pointer), this) ?? throw new InvalidOperationException("Failed to get root node.");
    }

    public Node? RootNodeWithOffset(uint offsetBytes, Point offsetPoint) 
    {
        ThrowIfDisposed();
        return Node.FromNative(Binding.ts_tree_root_node_with_offset(_pointer, offsetBytes, offsetPoint), this);
    }

    public void Edit(InputEdit edit) 
    {
        ThrowIfDisposed();
        Binding.ts_tree_edit(_pointer, ref edit);
    }
}