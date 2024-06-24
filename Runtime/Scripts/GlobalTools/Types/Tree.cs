#define DEBUG
//#undef DEBUG
//#define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public class Tree<T> where T : ITreeNode, new() {
        private Hashtable nodes;
        private HashSet<T> roots;

        public Tree() {
            this.nodes = new Hashtable();
            this.roots = new HashSet<T>();
        }

        public void AddNode(T _itemNode) {
            // Add the node..
            this.nodes.Add(_itemNode.GetHashCode(), _itemNode);
            // If poarent present set the node as child otherwise add to roots
            bool parentPresent = false;
            if (_itemNode.ParentHash != null && HasNode(_itemNode.ParentHash.Value)) { 
                T _parentNode = GetNode(_itemNode.ParentHash.Value);
                _itemNode.SetParent(_parentNode);
                _parentNode.AddChild(_itemNode);
                parentPresent = true;
            } else 
                this.roots.Add(_itemNode);
            // Check if children are present and set their parent
            bool childrenPresent = false;
            List<int> possibleChildrenHashes = new List<int>(_itemNode.ChildrenHash);
            foreach (int childHash in possibleChildrenHashes) {
                if (!HasNode(childHash)) {
                    _itemNode.RemoveChild(childHash);
                    continue;
                }
                T childNode = GetNode(childHash);
                SetNodeParent(childNode, _itemNode);
                childrenPresent = true;
            }
            #if DEBUG2
            this.Log($"Node: {_itemNode.ToString()} added, is root {IsRoot(_itemNode)}. Parent Present: {parentPresent}. Children present: {childrenPresent}, count: {_itemNode.ChildrenCount}.");
            #endif
        }
        private void SetNodeParent(T childNode, T parentNode) {
            if (IsRoot(childNode)) {
                this.roots.Remove(childNode);
            }
            else {
                GetNode(childNode).RemoveChild(childNode);
            }
            childNode.SetParent(parentNode);
            parentNode.AddChild(childNode);
        }

        public bool HasNode(T node) => node != null && HasNode(node.GetHashCode());
        public bool HasNode(int hashCode) => this.nodes.ContainsKey(hashCode);

        public T? GetNode(T node) => GetNode(node.GetHashCode());
        private T? GetNode(int haashCode) => (T)this.nodes[haashCode];

        public bool IsRoot(T node) => node != null && this.roots.Contains(node);

        public override string ToString() {
            string message = string.Empty;
            foreach (T itemNode in this.roots) {
                message += $"{ToString(itemNode, 0)}\n";

            }

            return message;
        }
        private string ToString(T node, int depth) {
            string message = new String('|', depth) + $"{node.ToString()}\n";
            foreach (int childHash in node.ChildrenHash) {
                if (!HasNode(childHash)) continue;
                T childNode = GetNode(childHash);
                message += ToString(childNode, depth + 1);
            }
            return message;
        }
    }
    public interface ITreeNode {
        public int? ParentHash { get; set; }
        public List<int> ChildrenHash { get; set; }

        public int GetHashCode();
        public string ToString();

        public void SetParent(ITreeNode _parentNode) => SetParent(_parentNode.GetHashCode());
        public void SetParent(int _parentHash) {
            this.ParentHash = _parentHash;
        }

        public void AddChild(ITreeNode _childNode) => AddChild(_childNode.GetHashCode());
        public void AddChild(int _childHash) {
            this.ChildrenHash.Add(_childHash);
        }

        public void RemoveChild(ITreeNode _childNode) => RemoveChild(_childNode.GetHashCode());
        public void RemoveChild(int _childHash) {
            this.ChildrenHash.Remove(_childHash);
        }

        public int ChildrenCount => this.ChildrenHash.Count;

        public bool IsMatching(ITreeNode valueNode) => this.GetHashCode() == valueNode.GetHashCode();
        public bool IsMatching(int hashCode) => this.GetHashCode() == hashCode;
        public bool HasChild(ITreeNode valueNode) => this.ChildrenHash.Exists(hash => hash == valueNode.GetHashCode());
        public bool HasChild(int hashCode) => this.ChildrenHash.Exists(hash => hash == hashCode);
    }
}
