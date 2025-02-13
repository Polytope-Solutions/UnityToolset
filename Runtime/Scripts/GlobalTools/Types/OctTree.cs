using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public partial class OctTree {
        private OctTreeNode root;
        private Hashtable octTreeNodes;
        private int maxDepth;
        private int maxElementsPerLeaf;
        private Func<object, SpatialRange> evaluator;

        private OctTreeNode this[int hash] {
            get {
                return (OctTreeNode)this.octTreeNodes[hash];
            }
        }

        public OctTree(int maxDepth, int maxElementsPerLeaf, Func<object, SpatialRange> evaluator) {
            this.maxDepth = maxDepth;
            this.maxElementsPerLeaf = maxElementsPerLeaf;
            this.evaluator = evaluator;

            this.root = new ();
            this.octTreeNodes = new ();
            this.octTreeNodes.Add(this.root.GetHashCode(), this.root);
        }

        private void Add(OctTreeNode node) {
            int hash = node.GetHashCode();
            if (this.octTreeNodes.ContainsKey(hash)) {
                StringBuilder log = new StringBuilder();
                log.AppendLine("SpatialPratitioningMonitor");
                ((OctTreeNode)this.octTreeNodes[hash]).Log(this, ref log);
                Debug.LogWarning(log.ToString());
                return;
            }
            this.octTreeNodes.Add(hash, node);
        }
        public void AddElement(object element, SpatialRange? elementRelativeRange = null) {
            if (!elementRelativeRange.HasValue)
                elementRelativeRange = this.evaluator(element);
            this.root.AddElement(this, element, elementRelativeRange.Value);
        }
        public void RemoveElement(object element, SpatialRange? elementRelativeRange=null) {
            if (!elementRelativeRange.HasValue)
                elementRelativeRange = this.evaluator(element);
            this.root.RemoveElement(this, element, elementRelativeRange.Value);
        }

        public void EvaluateRange(SpatialRange range, ref HashSet<object> objects) { 
            this.root.EvaluateRange(this, range, ref objects);
        }

        public string Log() {
            StringBuilder log = new ();
            int elementCount = 0, leafNodeCount = 0, totalNodeCount = this.octTreeNodes.Count, emptyNodeCount = 0, maxDepth = 0, maxElementCount = 0;
            OctTreeNode node;
            foreach (DictionaryEntry nodeHolder in this.octTreeNodes) {
                node = ((OctTreeNode)nodeHolder.Value);
                if (node.IsLeaf)
                    leafNodeCount++;
                if (node.IsEmpty)
                    emptyNodeCount++;
                elementCount += node.ElementCount;
                maxDepth = Mathf.Max(maxDepth, node.Depth);
                maxElementCount = Mathf.Max(maxElementCount, node.ElementCount);
            }
            log.AppendLine($"OctTree: Total Nodes: {totalNodeCount}. Leaf node count {leafNodeCount}. Empty node count {emptyNodeCount}. Total non-exclusive elements {elementCount}. Max depth {maxDepth}. Max element count per node {maxElementCount}.");
            this.root.Log(this, ref log);
            return log.ToString();
        }
    }
}
