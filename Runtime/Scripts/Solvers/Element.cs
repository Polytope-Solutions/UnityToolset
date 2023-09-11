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
        protected int state;

        public Element() {
            this.state = 0;
        }

        public abstract void UpdateState(int newState);
    }
    
    public abstract class ElementList<T> where T : Element, new() {
        protected List<T> _elements;
        public List<T> elements => this._elements;
        

        public ElementList() {
            this._elements = new List<T>();
        }
        //////////////////////////////////////////////////////////////////////////////////
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
        public virtual void Clear() {
            if (this._elements != null)
                this._elements.Clear();
        }

        //////////////////////////////////////////////////////////////////////////////////
        public virtual void Add(T element) {
            this._elements.Add(element);
        }
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