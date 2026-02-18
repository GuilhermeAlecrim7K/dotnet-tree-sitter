using TreeSitter;
using System.Runtime.InteropServices;

namespace SampleProject;

internal sealed partial class JsonLanguage() : Language(tree_sitter_json())
{
	[LibraryImport("tree-sitter-json")]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl)})]
	private static partial IntPtr tree_sitter_json();
}