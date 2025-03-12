// based on https://github.com/adammyhre/Unity-Serialized-Interface

using System;
using UnityEngine;

using UnityEditor;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.Editor.GlobalTools.InterfaceReferenceGUIUtility;

namespace PolytopeSolutions.Toolset.GlobalTools {
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute), true)]
    public class RequireInterfaceAttributeDrawer : PropertyDrawer {
        private RequireInterfaceAttribute internalAttribute => (RequireInterfaceAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Type interfaceType = internalAttribute.interfaceType;
            EditorGUI.BeginProperty(position, label, property);
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic) 
                DrawInterfaceCollectionObjectField(position, property, label, interfaceType);
            else
                DrawInterfaceSingleObjectField(position, property, label, interfaceType);
            EditorGUI.EndProperty();
            InterfaceArgs args = new InterfaceArgs(GetTypeOrElementType(fieldInfo.FieldType), interfaceType);
            DrawInterfaceLabel(position, property, args);
        }
        private void DrawInterfaceSingleObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType) {
            UnityEngine.Object oldReference = property.objectReferenceValue;
            UnityEngine.Object newReference = EditorGUI.ObjectField(position, label, oldReference, typeof(UnityEngine.Object), true);

            if (newReference != null && newReference != oldReference)
                ValidateAndAsssign(property, newReference, interfaceType);
            else if (newReference == null)
                property.objectReferenceValue = null;
        }
        private void DrawInterfaceCollectionObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType) {
            float yOffset = EditorGUIUtility.singleLineHeight;
            property.arraySize = EditorGUI.IntField(new Rect(position.x, position.y, position.width, yOffset), $"{label.text} Size", property.arraySize);

            SerializedProperty element;
            Rect elementRect;
            for (int i = 0; i < property.arraySize; i++, yOffset += EditorGUIUtility.singleLineHeight) {
                element = property.GetArrayElementAtIndex(i);
                elementRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                DrawInterfaceSingleObjectField(elementRect, element, new GUIContent($"Element {i}"), interfaceType);
            }
        }

        private void ValidateAndAsssign(SerializedProperty property, UnityEngine.Object newReference, Type interfaceType) {
            if (newReference is GameObject goItem) {
                var component = goItem.GetComponent(interfaceType);
                if (component != null) {
                    property.objectReferenceValue = component;
                    return;
                }
            } else if (interfaceType.IsAssignableFrom(newReference.GetType())) {
                property.objectReferenceValue = newReference;
                return;
            }
            Debug.LogWarning($"The assigned obejct does not implement {interfaceType.Name}");
            property.objectReferenceValue = null;
        }

        private Type GetTypeOrElementType(Type type) { 
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }
    }
}
