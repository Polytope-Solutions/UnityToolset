using System.Runtime.InteropServices;

namespace PolytopeSolutions.Toolset.Plugins.Web {
	public static class WebHelper {
		[DllImport("__Internal")]
		public static extern void SaveFile(byte[] array, int byteLength, string fileName);
	}
}