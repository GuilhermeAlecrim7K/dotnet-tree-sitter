using TreeSitter;
using System.Runtime.InteropServices;

namespace SampleProject;

public static partial class Pascal
{
	public static Language CreateLanguage() => new PascalLanguage();

	private class PascalLanguage : Language
	{
		public PascalLanguage() : base(tree_sitter_pascal())
		{
		}
	}

	[LibraryImport("tree-sitter-pascal")]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl)})]
	private static partial IntPtr tree_sitter_pascal();
}
