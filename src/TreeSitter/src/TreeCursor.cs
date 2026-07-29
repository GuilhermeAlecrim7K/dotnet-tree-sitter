namespace TreeSitter;

public sealed class TreeCursor : IDisposable
{
    private bool _disposed = false;
    private Binding.TreeCursor NativeCursor;

    internal TreeCursor(Binding.TreeCursor cursor)
    {
        NativeCursor = cursor;
    }

    public TreeCursor(Node node)
    {
        NativeCursor = Binding.ts_tree_cursor_new(node.NativeNode);
    }

    ~TreeCursor()
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

        Binding.ts_tree_cursor_delete(ref NativeCursor);
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TreeCursor));
    }

    public void Reset(Node node)
    {
        ThrowIfDisposed();
        Binding.ts_tree_cursor_reset(ref NativeCursor, node.NativeNode);
    }

    public Node CurrentNode()
    {
        ThrowIfDisposed();
        return new(Binding.ts_tree_cursor_current_node(ref NativeCursor));
    }

    public ushort CurrentFieldId()
    {
        ThrowIfDisposed();
        return Binding.ts_tree_cursor_current_field_id(ref NativeCursor);
    }

    public ushort CurrentSymbol()
    {
        ThrowIfDisposed();
        return Binding.ts_node_symbol(Binding.ts_tree_cursor_current_node(ref NativeCursor));
    }

    public bool GotoParent()
    {
        ThrowIfDisposed();
        return Binding.ts_tree_cursor_goto_parent(ref NativeCursor);
    }

    public bool GotoNextSibling()
    {
        ThrowIfDisposed();
        return Binding.ts_tree_cursor_goto_next_sibling(ref NativeCursor);
    }

    public bool GotoFirstChild()
    {
        ThrowIfDisposed();
        return Binding.ts_tree_cursor_goto_first_child(ref NativeCursor);
    }

    public long GotoFirstChildForOffset(uint offset)
    {
        ThrowIfDisposed();
        return Binding.ts_tree_cursor_goto_first_child_for_byte(ref NativeCursor, offset * sizeof(ushort));
    }

    public long GotoFirstChildForPoint(Point point)
    {
        ThrowIfDisposed();
        return Binding.ts_tree_cursor_goto_first_child_for_point(ref NativeCursor, point);
    }

    public TreeCursor Copy()
    {
        ThrowIfDisposed();
        return new(Binding.ts_tree_cursor_copy(ref NativeCursor));
    }
}