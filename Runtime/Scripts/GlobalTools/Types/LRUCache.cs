using System.Collections.Generic;
using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public class LRUCache<TKey, TNode> where TNode : class {
        private readonly int capacity;
        private readonly LinkedList<TNode> cache;  // front = newest
        private readonly Dictionary<TKey, LinkedListNode<TNode>> map;
        private readonly Action<TKey, TNode> evictionCallback;

        public LRUCache(int capacity, Action<TKey, TNode> evictionCallback = null) {
            this.capacity = capacity;
            this.cache = new LinkedList<TNode>();
            this.map  = new Dictionary<TKey, LinkedListNode<TNode>>(capacity);
            this.evictionCallback = evictionCallback;
        }

        #region PUBLIC_FUNCTIONALITY
        public int Count
            => this.cache.Count;
        public int Capacity 
            => this.capacity;
        public bool Contains(TKey hash) 
            => this.map.ContainsKey(hash);

        public IEnumerable<TNode> Entries => this.cache;
        public TNode Touch(TKey hash, TNode newData) {
            if (this.map.TryGetValue(hash, out LinkedListNode<TNode> node)) {
                if (this.cache.First != node) {
                    this.cache.Remove(node);
                    this.cache.AddFirst(node);
                }
                return node.Value; // return already-cached Node
            }

            LinkedListNode<TNode> newNode = this.cache.AddFirst(newData);
            this.map[hash] = newNode;

            if (this.cache.Count > this.capacity)
                this.EvictTail(hash);

            return newData;
        }

        public TNode TryGet(TKey hash) {
            return this.map.TryGetValue(hash, out LinkedListNode<TNode> node) 
                ? node.Value : null;
        }


        public bool Remove(TKey hash) {
            if (!this.map.TryGetValue(hash, out LinkedListNode<TNode> node)) 
                return false;
            this.cache.Remove(node);
            this.map.Remove(hash);
            this.evictionCallback?.Invoke(hash, node.Value);
            return true;
        }
        public void Clear() {
            foreach (var kvp in this.map) {
                this.evictionCallback?.Invoke(kvp.Key, kvp.Value.Value);
            }
            this.cache.Clear();
            this.map.Clear();
        }
        #endregion
        #region INTERNALS
        private void EvictTail(TKey hash) {
            LinkedListNode<TNode> tail = this.cache.Last!;
            this.map.Remove(hash);
            this.cache.RemoveLast();
            this.evictionCallback?.Invoke(hash, tail.Value);
        }
        #endregion
    }
}