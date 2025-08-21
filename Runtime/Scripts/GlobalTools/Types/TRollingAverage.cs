using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Types{
    public class TRollingAverage<T> {
        private T[] items;
        private int index;
        private int count;
        private bool isComplete;
        private Func<T, T, T> add;
        private Func<T, float, T> multiply;

        public TRollingAverage(int count, Func<T, T, T> add, Func<T, float, T> multiply) {
            this.count = count;
            this.items = new T[count];
            this.add = add;
            this.multiply = multiply;
            Clear();
        }
        public void Add(T item) {
            this.items[this.index] = item;
            this.index = (this.index + 1) % this.count;
            if (!this.isComplete)
                this.isComplete = this.index == 0;
        }
        public void Clear() {
            this.index = 0;
            this.isComplete = false;
        }
        public T Average {
            get {
                T total = default;
                int currentCount = (this.isComplete ? this.count : this.index);
                for (int i = 0, j = (this.index + this.count - 1) % this.count; i < currentCount; i++, j = (j + 1) % this.count) {
                    total = this.add(total, this.items[i]);
                }
                return multiply(total, 1.0f / currentCount);
            }
        }
    }
}