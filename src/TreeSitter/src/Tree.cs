namespace TreeSitter;

public sealed class Tree : IDisposable
{
    internal IntPtr Ptr;
    public Language Language { get; }

    internal Tree(IntPtr ptr, Language language)
    {
        Ptr = ptr;
        Language = language;
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            Binding.ts_tree_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public Tree Copy()
    {
        var ptr = Binding.ts_tree_copy(Ptr);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to copy tree.");
        return new Tree(ptr, Language);
    }

    public Node? RootNode()
    {
        return Node.FromNative(Binding.ts_tree_root_node(Ptr), this);
    }

    public Node? RootNodeWithOffset(uint offsetBytes, Point offsetPoint) => Node.FromNative(Binding.ts_tree_root_node_with_offset(Ptr, offsetBytes, offsetPoint), this);

    public void Edit(InputEdit edit) => Binding.ts_tree_edit(Ptr, ref edit);
}