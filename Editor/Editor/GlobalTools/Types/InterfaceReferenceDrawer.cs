// based on https://github.com/adammyhre/Unity-Serialized-Interface

using UnityEngine;

using UnityEditor;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.Editor.GlobalTools.InterfaceReferenceGUIUtility;

namespace PolytopeSolutions.Toolset.Editor.GlobalTools {
    [CustomPropertyDrawer(typeof(InterfaceReference<>), true)]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>), true)]
    public class InterfaceReferenceDrawer : PropertyDrawer {
        private const string ValueFieldName = "value";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty internalProperty = property.FindPropertyRelative(ValueFieldName);
            if (fieldInfo == null) return;
            InterfaceArgs args = InterfaceArgs.GetArguments(fieldInfo);

            EditorGUI.BeginProperty(position, label, property);
            UnityEngine.Object assignedObject = EditorGUI.ObjectField(position, label, internalProperty.objectReferenceValue, typeof(UnityEngine.Object), true);
            if (assignedObject == null) 
                internalProperty.objectReferenceValue = null;
            else {
                if (assignedObject is GameObject goItem) 
                    ValidateAndAssignObject(internalProperty, goItem.GetComponent(args.interfaceType), goItem.name, args.interfaceType.Name);
                else
                    ValidateAndAssignObject(internalProperty, assignedObject, args.interfaceType.Name);
            }
            EditorGUI.EndProperty();
            DrawInterfaceLabel(position, internalProperty, args);
        }

        private static void ValidateAndAssignObject(SerializedProperty property, UnityEngine.Object targetObject, string componentNameOrType, string interfaceName = null) {
            if (targetObject != null)
                property.objectReferenceValue = targetObject;
            else {
                Debug.LogWarning(@$"The {((string.IsNullOrEmpty(interfaceName)) ? "assigned obejct" : $"GameObject '{componentNameOrType}'")
                    } does not have a component that implements '{componentNameOrType}'"
                );
                property.objectReferenceValue = null;
            }
        }
    }
}
