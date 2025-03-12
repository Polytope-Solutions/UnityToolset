using UnityEditor;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools;

namespace PolytopeSolutions.Toolset.Editor.GlobalTools {
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // One line of  oxygen free code.
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }

    }
}
