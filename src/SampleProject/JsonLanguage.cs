using TreeSitter;
using System.Runtime.InteropServices;

namespace SampleProject;

public static partial class Json
{
	public static Language CreateLanguage() => new JsonLanguage();

	private class JsonLanguage : Language
	{
		public JsonLanguage() : base(tree_sitter_json())
		{
		}
	}

	[LibraryImport("tree-sitter-json")]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl)})]
	private static partial IntPtr tree_sitter_json();
}