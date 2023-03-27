using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using System.Linq;
using System;

namespace PolytopeSolutions.Toolset.Grid {
    public interface GridElementConstraint {
        public bool IsSatisfied(List<GridElementConstraint> otherConstraints);
    }
    [System.Serializable]
    public class GridElementNeighborConstraint : GridElementConstraint { 
        public enum ConnectorDirection { 
            Up,
            Down,
            Front,
            Back,
            Left,
            Right
        }
        public static Vector3Int NeighborOffset(GridElementNeighborConstraint constraint) {
            int x = 0, y = 0, z = 0;
            x = (constraint.direction == ConnectorDirection.Right) ? 1 : (constraint.direction == ConnectorDirection.Left) ? -1 : 0;
            y = (constraint.direction == ConnectorDirection.Up) ? 1 : (constraint.direction == ConnectorDirection.Down) ? -1 : 0;
            z = (constraint.direction == ConnectorDirection.Front) ? 1 : (constraint.direction == ConnectorDirection.Back) ? -1 : 0;
            return new Vector3Int(x, y, z);
        }
        //[Flags]
        public enum ConnectorType : UInt16 {
            None,
            Type0 = 1,
            Type1 = 2,
            Type2 = 4,
            Type3 = 8,
            Type4 = 16,
            Type5 = 32,
            Type6 = 64,
            Type7 = 128,
            Type8 = 256,
            Type9 = 512
        }
        public ConnectorDirection direction;
        public ConnectorType type;
        public bool IsSatisfied(List<GridElementConstraint> otherConstraints) {
            bool satisfied = false;
            foreach (GridElementConstraint constraint in otherConstraints) { 
                GridElementNeighborConstraint neighbor = (GridElementNeighborConstraint)constraint;
                int currentDirection = (int)this.direction;
                int neighborDirection = (int)neighbor.direction;
                satisfied |= (((currentDirection % 2 == 0) && ((int)neighbor.direction % 2 == 1) && (neighborDirection - currentDirection == 1))
                    || ((currentDirection % 2 == 1) && ((int)neighbor.direction % 2 == 0) && (neighborDirection - currentDirection == -1))
                    && (this.type == neighbor.type));
            }
            return satisfied;
        }
    }
}
