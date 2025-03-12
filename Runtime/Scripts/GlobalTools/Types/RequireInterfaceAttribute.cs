// based on https://github.com/adammyhre/Unity-Serialized-Interface

using System;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute {
        public readonly Type interfaceType;

        public RequireInterfaceAttribute(Type interfaceType) {
            Debug.Assert(interfaceType.IsInterface, $"Provided type {nameof(interfaceType)} is not an interface");
            this.interfaceType = interfaceType;
        }
    }
    [Serializable]
    public class InterfaceReference<TInterface, TObject>
            where TObject : UnityEngine.Object
            where TInterface : class {
        [SerializeField, HideInInspector] private TObject value;

        public TInterface Value {
            get {
                return this.value switch {
                    TInterface interfaceValue => interfaceValue,
                    null => null,
                    _ => throw new Exception($"Value {this.value} does not implement interface {nameof(TInterface)}")
                };
            }
            set {
                this.value = value switch {
                    TObject objectValue => objectValue,
                    null => null,
                    _ => throw new Exception($"Value {value} is not of type {typeof(TObject)}")
                };
            }
        }
        public TObject DirectValue { get => this.value; set => this.value = value; }
        public InterfaceReference() { }
        public InterfaceReference(TObject value) {
            this.value = value;
        }
        public InterfaceReference(TInterface value) {
            this.value = value as TObject;
        }
    }
    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, UnityEngine.Object>
            where TInterface : class { }
}
