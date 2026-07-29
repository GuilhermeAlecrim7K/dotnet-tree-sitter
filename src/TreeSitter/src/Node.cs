using System.Runtime.InteropServices;

namespace TreeSitter;

public sealed class Node
{
    internal readonly Binding.Node NativeNode;

    internal Node(Binding.Node nativeNode)
    {
        NativeNode = nativeNode;
    }

    public string Type() => Marshal.PtrToStringAnsi(Binding.ts_node_type(NativeNode)) ?? "";

    public ushort Symbol() => Binding.ts_node_symbol(NativeNode);

    public string GrammarSymbol() => Marshal.PtrToStringAnsi(Binding.ts_node_grammar_symbol(NativeNode)) ?? "";

    public uint StartByteOffset() => Binding.ts_node_start_byte(NativeNode) / sizeof(ushort);

    public Point StartPoint()
    {
        var pt = Binding.ts_node_start_point(NativeNode);
        return new Point(pt.Row, pt.Column / sizeof(ushort));
    }

    public uint EndByteOffset() => Binding.ts_node_end_byte(NativeNode) / sizeof(ushort);

    public Point EndPoint()
    {
        var pt = Binding.ts_node_end_point(NativeNode);
        return new Point(pt.Row, pt.Column / sizeof(ushort));
    }

    public override string ToString()
    {
        var memAllocatedString = Binding.ts_node_string(NativeNode);
        var result = Marshal.PtrToStringAnsi(memAllocatedString) ?? "";
        Binding.ts_node_string_free(memAllocatedString);
        return result;
    }

    public bool IsNamed() => Binding.ts_node_is_named(NativeNode);

    public bool IsMissing() => Binding.ts_node_is_missing(NativeNode);

    public bool IsExtra() => Binding.ts_node_is_extra(NativeNode);

    public bool HasChanges() => Binding.ts_node_has_changes(NativeNode);

    public bool HasError() => Binding.ts_node_has_error(NativeNode);

    public bool IsError() => Binding.ts_node_is_error(NativeNode);

    public Node? Parent() => FromNative(Binding.ts_node_parent(NativeNode));

    public Node? Child(uint index) => FromNative(Binding.ts_node_child(NativeNode, index));

    public string? FieldNameForChild(uint index) => Marshal.PtrToStringAnsi(Binding.ts_node_field_name_for_child(NativeNode, index));

    public string? FieldNameForNamedChild(uint index) => Marshal.PtrToStringAnsi(Binding.ts_node_field_name_for_named_child(NativeNode, index));

    public uint ChildCount() => Binding.ts_node_child_count(NativeNode);

    public Node? NamedChild(uint index) => FromNative(Binding.ts_node_named_child(NativeNode, index));

    public uint NamedChildCount() => Binding.ts_node_named_child_count(NativeNode);

    public Node? ChildByFieldName(string fieldName) => FromNative(Binding.ts_node_child_by_field_name(NativeNode, fieldName, (uint)fieldName.Length));

    public Node? ChildByFieldId(ushort fieldId) => FromNative(Binding.ts_node_child_by_field_id(NativeNode, fieldId));

    public Node? NextSibling() => FromNative(Binding.ts_node_next_sibling(NativeNode));

    public Node? PrevSibling() => FromNative(Binding.ts_node_prev_sibling(NativeNode));

    public Node? NextNamedSibling() => FromNative(Binding.ts_node_next_named_sibling(NativeNode));

    public Node? PrevNamedSibling() => FromNative(Binding.ts_node_prev_named_sibling(NativeNode));

    public Node? FirstChildForByteOffset(uint offset) => FromNative(Binding.ts_node_first_child_for_byte(NativeNode, offset * sizeof(ushort)));

    public Node? FirstNamedChildForByteOffset(uint offset) => FromNative(Binding.ts_node_first_named_child_for_byte(NativeNode, offset * sizeof(ushort)));

    public uint DescendantCount() => Binding.ts_node_descendant_count(NativeNode);

    public Node? DescendantForByteOffsetRange(uint start, uint end) => FromNative(Binding.ts_node_descendant_for_byte_range(NativeNode, start * sizeof(ushort), end * sizeof(ushort)));

    public Node? DescendantForPointRange(Point start, Point end) => FromNative(Binding.ts_node_descendant_for_point_range(NativeNode, start, end));

    public Node? NamedDescendantForByteOffsetRange(uint start, uint end) => FromNative(Binding.ts_node_named_descendant_for_byte_range(NativeNode, start * sizeof(ushort), end * sizeof(ushort)));

    public Node? NamedDescendantForPointRange(Point start, Point end) => FromNative(Binding.ts_node_named_descendant_for_point_range(NativeNode, start, end));

    public string Text(string data) => data[(int)StartByteOffset()..(int)EndByteOffset()];

    internal static Node? FromNative(Binding.Node nativeNode) => Binding.ts_node_is_null(nativeNode) ? null : new(nativeNode);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Node other && this == other;
    public override int GetHashCode() => NativeNode.GetHashCode();

    public static bool operator == (Node a, Node b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if ((object)a == null || (object)b == null)
            return false;

        return Binding.ts_node_eq(a.NativeNode, b.NativeNode);
    }

    public static bool operator !=(Node a, Node b) => !(a == b);
}