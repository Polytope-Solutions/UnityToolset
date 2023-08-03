using System.Runtime.InteropServices;

namespace PolytopeSolutions.Toolset.Plugins.WebGL {
	public static class WebGLHelper {
		[DllImport("__Internal")]
		public static extern void SaveFile(byte[] array, int byteLength, string fileName);
	}
}