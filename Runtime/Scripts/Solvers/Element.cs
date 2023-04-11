#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System;

namespace PolytopeSolutions.Toolset.Solvers {
    public abstract class Element {
        protected bool unitChanged = false;
        protected bool unitUpdated = false;
        protected bool unitResolved = false;
        public bool UnitChanged => this.unitChanged;
        public bool UnitUpdated => this.unitUpdated;
        public bool UnitResolved => this.unitResolved;

        public Element() {
            this.unitChanged = true;
            this.unitUpdated = true;
            this.unitResolved = false;
        }
        
        public void MarkSetup() {
            this.unitChanged = false;
            this.unitUpdated = false;
        }
        public void MarkResolved() {
            this.unitResolved = true;
        }
    }
    
    public abstract class ElementList<T> where T : Element, new() {
        protected List<T> _elements;
        public List<T> elements => this._elements;

        public T this[int i] {
            get {
                return this._elements[i];
            }
            set {
                this._elements[i] = value;
            }
        }
        public virtual int Count {
            get {
                return this._elements.Count;
            }
        }

        public ElementList() {
            this._elements = new List<T>();
        }

        //////////////////////////////////////////////////////////////////////////////////
        public virtual void Insert(int i, T element) {
            while (this._elements.Count <= i)
                this._elements.Add(new T());
            this._elements[i] = element;
        }
        public virtual void RemoveAt(int i) {
            if (i < this.Count)
                this._elements.RemoveAt(i);
        }
    }
}