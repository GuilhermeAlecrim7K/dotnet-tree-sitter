using System.Runtime.InteropServices;

namespace TreeSitter.Pascal.Tests;

internal sealed partial class PascalLanguage() : Language(tree_sitter_pascal())
{
    [LibraryImport("tree-sitter-pascal")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial IntPtr tree_sitter_pascal();
}