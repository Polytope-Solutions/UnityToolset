#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using PolytopeSolutions.Toolset.Solvers;
using System;

namespace PolytopeSolutions.Toolset.Grid {
    public abstract class GridElement : Element {
        protected int[] gridPosition;

        public GridElement() : base() {}
        public GridElement(int[] _gridPosition) : this() { 
            this.gridPosition = _gridPosition;
        }
        public virtual void Update(int[] _gridPosition) { 
            this.gridPosition = _gridPosition;
        }
    }
    public abstract class Grid<T> : ElementList<T> where T : GridElement, new() {
        protected int[] _dimensions;
        public int[] dimensions => this._dimensions;

        public Grid() : base() { }
        public Grid(int[] dimensions) : this() {
            this._dimensions = dimensions;

            int count = dimensions[0];
            for (int d = 1; d < dimensions.Length; d++)
                count *= dimensions[d];
            for (int i = 0; i < count; i++)
                this._elements.Add(null);
        }

        ///////////////////////////////////////////////////////////////////////////////////
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
        public override int Count {
            get {
                if (this._dimensions == null || this._dimensions.Length == 0)
                    return 0;
                return Index(this.LastElementIndex)+1;
            }
        }
        public override void Clear() {
            this._dimensions = new int[0];
            base.Clear();
        }
        ///////////////////////////////////////////////////////////////////////////////////

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
            bool isValid = true;
            for (int d = 0; d < this._dimensions.Length; d++)
                isValid &= ((axisIndices[d] < this._dimensions[d]) && (axisIndices[d] >= 0));
            return isValid;
        }

        //////////////////////////////////////////////////////////////////////////////////
        public virtual void Insert(int[] axisIndices, T element) {
            Insert(Index(axisIndices), element);
        }
        public virtual void RemoveAt(int[] axisIndices) {
            this._elements.RemoveAt(Index(axisIndices));
        }
    }
}
