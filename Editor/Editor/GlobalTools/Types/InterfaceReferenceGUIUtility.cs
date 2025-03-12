// based on https://github.com/adammyhre/Unity-Serialized-Interface

using System;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Reflection;

using UnityEditor;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Editor.GlobalTools {
    public static class InterfaceReferenceGUIUtility {
        private static GUIStyle labelStyle;
        public struct InterfaceArgs {
            public readonly Type interfaceType;
            public readonly Type objectType;

            public InterfaceArgs(Type interfaceType, Type objectType) {
                Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} is not an interface");
                Debug.Assert(typeof(UnityEngine.Object).IsAssignableFrom(objectType), $"{nameof(objectType)} needs to be of type {typeof(UnityEngine.Object)}");
                this.objectType = objectType;
                this.interfaceType = interfaceType;
            }
            public static InterfaceArgs GetArguments(FieldInfo fieldInfo) {
                Type interfaceType = null, objectType = null, fieldType = fieldInfo.FieldType;

                if (!TryGetTypesFromInterfaceReference(fieldType, out interfaceType, out objectType))
                    GetTypesFromCollection(fieldType, out interfaceType, out objectType);

                return new InterfaceArgs(interfaceType, objectType);
            }
            private static bool TryGetTypesFromInterfaceReference(Type type, out Type interfaceType, out Type objectType) {
                interfaceType = objectType = null;

                if (type?.IsGenericType != true) return false;
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) {
                    type = type.BaseType;
                    genericType = type.GetGenericTypeDefinition();
                }
                if (genericType == typeof(InterfaceReference<,>)) {
                    Type[] types = type.GetGenericArguments();
                    interfaceType = types[0];
                    objectType = types[1];
                    return true;
                }
                return false;
            }
            private static void GetTypesFromCollection(Type type, out Type interfaceType, out Type objectType) {
                interfaceType = objectType = null;

                Type listInterface = type.GetInterfaces().FirstOrDefault(item
                    => item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IList<>)
                );
                if (listInterface != null) {
                    Type elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out interfaceType, out objectType);
                }
            }
        }

        public static void DrawInterfaceLabel(Rect position, SerializedProperty property, InterfaceArgs args) {
            InitializeStyleIfNeeded();
            int controlID = GUIUtility.GetControlID(FocusType.Passive) - 1;
            bool isHovering = position.Contains(Event.current.mousePosition);
            string displayString = (property.objectReferenceValue == null || isHovering) ? $"{args.interfaceType.Name}" : "*";
            DrawInterfaceNameLabel(position, displayString, controlID);
        }
        private static void DrawInterfaceNameLabel(Rect position, string displayString, int controlID) {
            if (Event.current.type != EventType.Repaint) return;

            GUIContent content = EditorGUIUtility.TrTextContent(displayString);
            Vector2 size = labelStyle.CalcSize(content);
            Rect labelPosition = position;

            labelPosition.x += position.width - size.x - 18;
            labelPosition.y += 1;
            labelPosition.width = size.x + 3;
            labelPosition.height -= 2;
            labelStyle.Draw(labelPosition, content, controlID, DragAndDrop.activeControlID == controlID, false);
        }
        private static void InitializeStyleIfNeeded() {
            if (labelStyle != null) return;
            labelStyle = new GUIStyle(EditorStyles.label) {
                font = EditorStyles.objectField.font,
                fontSize = EditorStyles.objectField.fontSize,
                fontStyle = EditorStyles.objectField.fontStyle,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 2, 0, 0)
            };
        }
    }
}
