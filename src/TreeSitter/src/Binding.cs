using System.Runtime.InteropServices;

namespace TreeSitter;

internal static partial class Binding
{
    [StructLayout(LayoutKind.Sequential)]
    public record struct LoggerData
    {
        public IntPtr Payload;
        public IntPtr Log;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback(IntPtr payload, LogType logType,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [StructLayout(LayoutKind.Sequential)]
    public record struct TreeCursor
    {
        public IntPtr Tree;
        public IntPtr Id;
        public uint Context0;
        public uint Context1;
        public uint Context2;
    }


    [StructLayout(LayoutKind.Sequential)]
    public record struct Node
    {
        public uint Context0;
        public uint Context1;
        public uint Context2;
        public uint Context3;
        public IntPtr Id;
        public IntPtr Tree;
    }
    [StructLayout(LayoutKind.Sequential)]
    public record struct QueryCapture
    {
        public Node Node;
        public uint Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public record struct QueryMatch
    {
        public uint Id;
        public ushort PatternIndex;
        public ushort CaptureCount;
        public IntPtr Captures;
    }

    // tree-sitter's default allocator is the CRT malloc/free (lib/src/alloc.c), and
    // libtree-sitter does not re-export `free` itself, so it cannot be P/Invoked from
    // the native library. NativeMemory.Free is the portable equivalent of CRT free.
    public static unsafe void free(IntPtr str) => NativeMemory.Free((void*)str);

    /********************/
    /* Section - Parser */
    /********************/

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_parser_new();

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_parser_delete(IntPtr parser);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_parser_language(IntPtr parser);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_parser_set_language(IntPtr parser, IntPtr language);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_parser_set_included_ranges(IntPtr parser,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Range[] ranges, uint length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
    public static partial Range[] ts_parser_included_ranges(IntPtr parser, out uint length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_parser_parse_string(IntPtr parser, IntPtr oldTree,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string input, uint length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    // TODO: If this is encoding based, shouldn't the string be passed as a pointer?
    public static partial IntPtr ts_parser_parse_string_encoding(IntPtr parser, IntPtr oldTree,
        [MarshalAs(UnmanagedType.LPWStr)] string input, uint length, InputEncoding encoding);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_parser_reset(IntPtr parser);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_parser_set_logger(IntPtr parser, LoggerData logger);

    /******************/
    /* Section - Tree */
    /******************/

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_tree_copy(IntPtr tree);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_tree_delete(IntPtr tree);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_tree_root_node(IntPtr tree);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_tree_root_node_with_offset(IntPtr tree, uint offsetBytes, Point offsetPoint);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_tree_language(IntPtr tree);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_tree_included_ranges(IntPtr tree, out uint length);

    public static void ts_tree_included_ranges_free(IntPtr ranges) => free(ranges);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_tree_edit(IntPtr tree, ref InputEdit edit);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_tree_get_changed_ranges(IntPtr old_tree, IntPtr new_tree, out uint length);

    /******************/
    /* Section - Node */
    /******************/

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_node_type(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_node_symbol(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_node_language(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_node_grammar_type(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_node_grammar_symbol(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_node_start_byte(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Point ts_node_start_point(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_node_end_byte(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Point ts_node_end_point(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_node_string(Node node);

    public static void ts_node_string_free(IntPtr str) => free(str);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_is_null(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_is_named(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_is_missing(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_is_extra(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_has_changes(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_has_error(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_is_error(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_node_parse_state(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_node_next_parse_state(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_parent(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_child_with_descendant(Node node, Node descendant);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_child(Node node, uint index);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_node_field_name_for_child(Node node, uint index);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_node_field_name_for_named_child(Node node, uint index);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_node_child_count(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_named_child(Node node, uint index);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_node_named_child_count(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_child_by_field_name(Node self,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string field_name, uint field_name_length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_child_by_field_id(Node self, ushort fieldId);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_next_sibling(Node self);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_prev_sibling(Node self);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_next_named_sibling(Node self);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_prev_named_sibling(Node self);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_first_child_for_byte(Node self, uint byteOffset);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_first_named_child_for_byte(Node self, uint byteOffset);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_node_descendant_count(Node self);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_descendant_for_byte_range(Node self, uint startByte, uint endByte);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_descendant_for_point_range(Node self, Point startPoint, Point endPoint);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_named_descendant_for_byte_range(Node self, uint startByte, uint endByte);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_node_named_descendant_for_point_range(Node self, Point startPoint, Point endPoint);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_node_edit(Node node, ref InputEdit edit);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_node_eq(Node node1, Node node2);

    /************************/
    /* Section - TreeCursor */
    /************************/

    // TODO: These ones are being passed as ref. Is this correct?

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial TreeCursor ts_tree_cursor_new(Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_tree_cursor_delete(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_tree_cursor_reset(ref TreeCursor cursor, Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Node ts_tree_cursor_current_node(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_tree_cursor_current_field_name(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_tree_cursor_current_field_id(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_tree_cursor_goto_parent(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_tree_cursor_goto_next_sibling(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_tree_cursor_goto_previous_sibling(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_tree_cursor_goto_first_child(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_tree_cursor_goto_last_child(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_tree_cursor_goto_descendant(ref TreeCursor cursor, uint goalDescendantIndex);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_tree_cursor_current_descendant_index(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_tree_cursor_current_depth(ref TreeCursor cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long ts_tree_cursor_goto_first_child_for_byte(ref TreeCursor cursor, uint byteOffset);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long ts_tree_cursor_goto_first_child_for_point(ref TreeCursor cursor, Point point);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial TreeCursor ts_tree_cursor_copy(ref TreeCursor cursor);

    /*******************/
    /* Section - Query */
    /*******************/

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_query_new(IntPtr language, [MarshalAs(UnmanagedType.LPUTF8Str)] string source,
        uint source_len, out uint error_offset, out QueryError error_type);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_delete(IntPtr query);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_query_pattern_count(IntPtr query);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_query_capture_count(IntPtr query);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_query_string_count(IntPtr query);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_query_start_byte_for_pattern(IntPtr query, uint patternIndex);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_query_end_byte_for_pattern(IntPtr query, uint patternIndex);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
    public static partial QueryPredicateStep[] ts_query_predicates_for_pattern(IntPtr query, uint patternIndex, out uint length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_query_is_pattern_rooted(IntPtr query, uint patternIndex);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_query_is_pattern_non_local(IntPtr query, uint patternIndex);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_query_is_pattern_guaranteed_at_step(IntPtr query, uint byteOffset);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_query_capture_name_for_id(IntPtr query, uint id, out uint length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Quantifier ts_query_capture_quantifier_for_id(IntPtr query, uint patternId, uint captureId);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_query_string_value_for_id(IntPtr query, uint id, out uint length);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_disable_capture(IntPtr query,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string captureName, uint captureNameLength);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_disable_pattern(IntPtr query, uint patternIndex);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_query_cursor_new();

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_cursor_delete(IntPtr cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_cursor_exec(IntPtr cursor, IntPtr query, Node node);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_query_cursor_did_exceed_match_limit(IntPtr cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_query_cursor_match_limit(IntPtr cursor);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_cursor_set_match_limit(IntPtr cursor, uint limit);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_cursor_set_byte_range(IntPtr cursor, uint start_byte, uint end_byte);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_cursor_set_point_range(IntPtr cursor, Point start_point, Point end_point);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_query_cursor_next_match(IntPtr cursor, out QueryMatch match);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_query_cursor_remove_match(IntPtr cursor, uint id);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ts_query_cursor_next_capture(IntPtr cursor, out QueryMatch match, out uint capture_index);

    /**********************/
    /* Section - Language */
    /**********************/

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_language_copy(IntPtr ptr);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void ts_language_delete(IntPtr ptr);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_language_symbol_count(IntPtr language);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_language_state_count(IntPtr language);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_language_symbol_for_name(IntPtr language,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length, [MarshalAs(UnmanagedType.I1)] bool is_named);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_language_field_count(IntPtr language);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_language_field_name_for_id(IntPtr language, ushort fieldId);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort ts_language_field_id_for_name(IntPtr language,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length);

    /* NOTE: Not useful for now
        [LibraryImport("tree-sitter")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial ushort ts_language_supertypes(IntPtr language, out uint length);

        [LibraryImport("tree-sitter")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial ushort ts_language_subtypes(IntPtr language, ushort supertype, out uint length);
    */

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr ts_language_symbol_name(IntPtr language, ushort symbol);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial SymbolType ts_language_symbol_type(IntPtr language, ushort symbol);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint ts_language_abi_version(IntPtr language);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial LanguageMetadata ts_language_metadata(IntPtr language);

    [LibraryImport("tree-sitter")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl)})]
    public static partial IntPtr ts_language_name(IntPtr language);

}