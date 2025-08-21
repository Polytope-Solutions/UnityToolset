using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    // Mini tool to show cursor in editor
    public class CursorRecordingVisual : MonoBehaviour {
        [SerializeField] private Texture2D cursor;
        void Start() {
            #if UNITY_EDITOR
            Cursor.SetCursor(this.cursor, Vector2.zero, CursorMode.ForceSoftware);
            #endif
        }
    }
}
