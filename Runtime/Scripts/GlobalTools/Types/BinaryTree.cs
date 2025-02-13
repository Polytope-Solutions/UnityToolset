using System;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public class BinaryTree<T> where T : IComparable<T> {
        public BinaryTreeNode<T> Root { get; private set; } = null;

        public void Add(T value) {
            if (this.Root == null) 
                this.Root = new BinaryTreeNode<T>(value);
            else 
                this.Root.Add(value);
        }
        public void Remove(T value) {
            if (this.Root.Remove(value))
                this.Root = null;
        }
    }

    public class BinaryTreeNode<T> where T : IComparable<T> {
        public T Value { get; private set; }
        public BinaryTreeNode<T> Left { get; private set; } = null;
        public BinaryTreeNode<T> Right { get; private set; } = null;

        public BinaryTreeNode(T value) => this.Value = value;

        public void Add(T newValue) {
            if (newValue.CompareTo(this.Value) < 0) {
                if (this.Left == null) 
                    this.Left = new BinaryTreeNode<T>(newValue);
                else 
                    this.Left.Add(newValue);
            }
            else if (newValue.CompareTo(this.Value) > 0) {
                if (this.Right == null) 
                    this.Right = new BinaryTreeNode<T>(newValue);
                else 
                    this.Right.Add(newValue);
            }
            // Ignore if equals to have only unique items.
        }
        public void Add(BinaryTreeNode<T> newNode) {
            Add(newNode.Value);
            if (newNode.Left == null && newNode.Right != null)
                // Has right child only
                Add(newNode.Right);
            if (newNode.Right == null && newNode.Left != null)
                Add(newNode.Left);
        }

        public bool Remove(T value) {
            if (value.CompareTo(this.Value) == 0) {
                // Current Node value is the value to remove
                if (this.Left == null && this.Right == null) {
                    // Has no children
                    return true;
                }
                else if (this.Left == null) {
                    // Has right child only
                    this.Value = this.Right.Value;
                    this.Left = this.Right.Left;
                    this.Right = this.Right.Right;
                }
                else if (this.Right == null) {
                    // Has left child only
                    this.Value = this.Left.Value;
                    this.Left = this.Left.Left;
                    this.Right = this.Left.Right;
                }
                else {
                    // Has both children
                    // Take the left tree and move the right to the right of the left
                    this.Value = this.Left.Value;
                    this.Left = this.Left.Left;
                    BinaryTreeNode<T> temp = this.Right;
                    this.Right = this.Left.Right;
                    this.Right.Add(temp);
                }
            }
            else if (this.Left != null && value.CompareTo(this.Value) < 0) {
                // Left might have the item
                if (this.Left.Remove(value)) 
                    this.Left = null;
            }
            else if (this.Right != null && value.CompareTo(this.Value) < 0) {
                // Right might have the item
                if (this.Right.Remove(value))
                    this.Right = null;
            }
            return false;
        }
    }
}
