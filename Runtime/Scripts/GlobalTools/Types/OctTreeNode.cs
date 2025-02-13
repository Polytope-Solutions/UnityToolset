using System;
using System.Collections.Generic;
using System.Text;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public partial class OctTree {
        private class OctTreeNode {
            //private int parent;
            private int depth;
            private List<int> children;
            private SpatialRange range;
            private HashSet<object> elements;

            private int hashCode;
            public override int GetHashCode() => this.hashCode;
            public bool IsLeaf => this.children.Count == 0;
            public bool IsEmpty => IsLeaf && this.elements.Count == 0;
            public int Depth => this.depth;
            public int ElementCount => this.elements.Count;

            public OctTreeNode() {
                //this.parent = -1;
                this.depth = 0;
                this.children = new();
                this.elements = new();
                this.range = new SpatialRange(0, 1, 0, 1, 0, 1);
                this.hashCode = this.range.GetHashCode();
            }
            public OctTreeNode(OctTreeNode parent, bool isLeft, bool isBack, bool isBottom) {
                //this.parent = parent.GetHashCode();
                this.depth = parent.depth + 1;
                this.children = new();
                this.elements = new();
                this.range = new SpatialRange(
                    (isLeft)    ? parent.range.MinX     : parent.range.HalfX,
                    (isLeft)    ? parent.range.HalfX    : parent.range.MaxX,
                    (isBottom)  ? parent.range.MinY     : parent.range.HalfY,
                    (isBottom)  ? parent.range.HalfY    : parent.range.MaxY,
                    (isBack)    ? parent.range.MinZ     : parent.range.HalfZ,
                    (isBack)    ? parent.range.HalfZ    : parent.range.MaxZ
                );
                this.hashCode = this.range.GetHashCode();
            }

            internal void AddElement(OctTree tree, object element, SpatialRange elementRelativeRange) {
                // Skip if not relevant
                if (!this.range.Intesects(elementRelativeRange)) return;
                // If has children - pass it along to the corresponding ones.
                if (!this.IsLeaf) {
                    OctTreeNode child;
                    for (int i = 0; i < 8; i++) {
                        child = tree[this.children[i]];
                        if (child.range.Intesects(elementRelativeRange))
                            child.AddElement(tree, element, elementRelativeRange);
                    } 
                    return;
                }
                // Otherwise try to add
                this.elements.Add(element);
                // if not at max depth and exceeded amount of elements - split and spread them.
                if (this.elements.Count > tree.maxElementsPerLeaf && this.depth < tree.maxDepth)
                    Split(tree);
            }
            private void Split(OctTree tree) {
                // Init Children
                bool isLeft, isBack, isBottom;
                OctTreeNode child;
                for (int i = 0; i < 8; i++) {
                    isLeft = i % 2 == 0;
                    isBack = i % 4 < 2;
                    isBottom = i < 4;
                    child = new OctTreeNode(this, isLeft, isBack, isBottom);
                    tree.Add(child);
                    this.children.Add(child.GetHashCode());
                }
                // Evaluate Elements and distribute to corresponding children
                SpatialRange elementRelativeRange;
                foreach(object element in this.elements) {
                    elementRelativeRange = tree.evaluator(element);

                    for (int i = 0; i < 8; i++) {
                        child = tree[this.children[i]];
                        child.AddElement(tree, element, elementRelativeRange);
                    }
                }
                this.elements.Clear();
            }

            internal void RemoveElement(OctTree tree, object element, SpatialRange elementRelativeRange) {
                // Skip if not relevant
                if (!this.range.Intesects(elementRelativeRange)) return;
                // If has children - pass it along to the corresponding one.
                if (!this.IsLeaf) {
                    OctTreeNode child;
                    for (int i = 0; i < 8; i++) {
                        child = tree[this.children[i]];
                        child.RemoveElement(tree, element, elementRelativeRange);
                    }
                    return;
                }
                // Otherwise try to remove
                this.elements.Remove(element);
                // TODO: Check if this is needed
                //// if doesn't contain elements - maybe try to merge 
                //if (this.elements.Count == 0)
                //    tree[this.parent].TryMerge(tree);
            }
            //private void TryMerge(OctTree tree) {
            //}

            internal void EvaluateRange(OctTree tree, SpatialRange range, ref HashSet<object> objects) {
                // Skip if not relevant
                if (!this.range.Intesects(range)) return;
                // In range
                // - If has children - pass to them
                if (!this.IsLeaf) {
                    foreach(int childHash in this.children)
                        tree[childHash].EvaluateRange(tree, range, ref objects);
                }
                // - Otherwise join elements
                else {
                    objects.UnionWith(this.elements);
                }
            }

            internal void Log(OctTree tree, ref StringBuilder log) {
                log.AppendLine($"{new string('-', this.depth)}|{this.range.ToString("F3")}. Children {this.children.Count}, Elements {this.elements.Count}");
                foreach (int childHash in this.children) {
                    tree[childHash].Log(tree, ref log);
                }
            }
        }
    }
}