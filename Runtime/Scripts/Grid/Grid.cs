#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System;

namespace PolytopeSolutions.Toolset.Grid {
    public abstract class GridElement {
        protected bool unitChanged = false;
        protected bool unitUpdated = false;
        protected bool unitResolved = false;
        public bool UnitChanged => this.unitChanged;
        public bool UnitUpdated => this.unitUpdated;
        public bool UnitResolved => this.unitResolved;
        protected int[] gridPosition;

        public GridElement() {
            this.unitChanged = true;
            this.unitUpdated = true;
            this.unitResolved = false;
        }
        public GridElement(int[] _gridPosition) : this() { 
            this.gridPosition = _gridPosition;
        }
        public virtual void Update(int[] _gridPosition) { 
            this.gridPosition = _gridPosition;
        }

        public void MarkSetup() {
            this.unitChanged = false;
            this.unitUpdated = false;
        }
    }
    public abstract class Grid<T> where T : GridElement, new() {
        protected List<T> _elements;
        protected int[] _dimensions;

        public T this[int i] {
            get {
                return this._elements[i];
            }
            set {
                this._elements[i] = value;
            }
        }
        public T this[int[] axisIndices] {
            get {
                return this[Index(axisIndices)];
            }
            set {
                this[Index(axisIndices)] = value;
            }
        }
        public int[] LastElementIndex {
            get {
                int[] lastElementIndex = new int[this._dimensions.Length];
                for (int d = 0; d< this._dimensions.Length; d++) {
                    lastElementIndex[d] = this._dimensions[d] - 1;
                }
                return lastElementIndex;
            }
        }
        public int Count {
            get {
                if (this._dimensions == null || this._dimensions.Length == 0)
                    return 0;
                return Index(this.LastElementIndex)+1;
            }
        }
        public int[] dimensions => this._dimensions;
        public List<T> elements => this._elements;

        public Grid() {
            this._elements = new List<T>();
        }
        public Grid(int[] dimensions) : this() {
            this._dimensions = dimensions;

            int count = dimensions[0];
            for (int d = 1; d < dimensions.Length; d++)
                count *= dimensions[d];
            for (int i = 0; i < count; i++)
                this._elements.Add(null);
        }
        public virtual void Update(int[] dimensions) {
            bool changed = (dimensions.Length != this._dimensions?.Length);
            if (!changed) { 
                for (int d = 0; d < dimensions.Length; d++) { 
                    changed |= this._dimensions[d] != dimensions[d];
                }
            }
            if (changed) {
                int oldCount = this.Count;
                this._dimensions = dimensions;
                int newCount = this.Count;
                if (oldCount < newCount) { 
                    Insert(LastElementIndex, new T());
                }
                else if (oldCount > newCount) {
                    for (int i = newCount+1; i < oldCount; i++) { 
                        RemoveAt(i); 
                    }
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////
        public virtual int Index(int[] axisIndices) {
            int index = 0, count = 0;

            for (int d0 = 1; d0 < this._dimensions.Length; d0++) {
                count = this._dimensions[d0];
                for (int d1 = d0+1; d1 < this._dimensions.Length; d1++){
                    count *= this._dimensions[d1];
                }

                index += axisIndices[d0 - 1] * count;
            }
            index += axisIndices[axisIndices.Length - 1];
            return index;
        }
        public virtual int[] AxisInidices(int index) { 
            int[] axisIndices = new int[this._dimensions.Length];
            int count = 0;

            for (int d0 = 1; d0 < this._dimensions.Length; d0++) {
                count = this._dimensions[d0];
                for (int d1 = d0 + 1; d1 < this._dimensions.Length; d1++) {
                    count *= this._dimensions[d1];
                }

                axisIndices[d0 - 1] = index/count;
                index -= axisIndices[d0 - 1] * count;
            }
            axisIndices[this._dimensions.Length - 1] = index;

            return axisIndices;
        }
        public virtual bool ValidIndex(int[] axisIndices) { 
            if (this._dimensions == null || this._dimensions.Length == 0 || this._dimensions.Length != axisIndices.Length) return false;
            bool isValid = false;
            for (int d = 0; d < this._dimensions.Length; d++)
                isValid |= ((this._dimensions[d] > axisIndices[d]) && (axisIndices[d] > 0));
            return isValid;
        }

        //////////////////////////////////////////////////////////////////////////////////
        public virtual void Insert(int i, T element) {
            while (this._elements.Count <= i)
                this._elements.Add(new T());
            this._elements[i] = element;
        }
        public virtual void Insert(int[] axisIndices, T element) {
            Insert(Index(axisIndices), element);
        }
        public virtual void RemoveAt(int i) {
            if (i < this.Count)
                this._elements.RemoveAt(i);
        }
        public virtual void RemoveAt(int[] axisIndices) {
            this._elements.RemoveAt(Index(axisIndices));
        }
    }
}
