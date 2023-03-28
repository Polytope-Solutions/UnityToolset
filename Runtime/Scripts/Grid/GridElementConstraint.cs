using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using System.Linq;
using System;

namespace PolytopeSolutions.Toolset.Grid {
    public interface GridElementConstraint {
        public GridElementConstraint Modify(int modifierType, object modifiers);
        public bool IsSatisfied(List<GridElementConstraint> otherConstraints);
    }
    [System.Serializable]
    public class GridElementNeighborConstraint : GridElementConstraint { 
        public enum ConnectorDirection { 
            None,
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
        ///////////////////////////////////////////////////////////////////////////////////
        public GridElementConstraint Modify(int modifierType, object modifiers) {
            GridElementConstraint newConstraint = null;
            ConnectorDirection _direction = ConnectorDirection.None;
            ConnectorType _type = ConnectorType.None;
            switch (modifierType) {
                case 0: // Rotation
                    Vector3 rotation = (Vector3)modifiers; 
                    _type = this.type;
                    if (rotation.y == 90) { 
                        switch (this.direction) {
                            case ConnectorDirection.Up:
                                _direction = ConnectorDirection.Up;
                                break;
                            case ConnectorDirection.Down:
                                _direction = ConnectorDirection.Down;
                                break;
                            case ConnectorDirection.Front:
                                _direction = ConnectorDirection.Right;
                                break;
                            case ConnectorDirection.Back:
                                _direction = ConnectorDirection.Left;
                                break;
                            case ConnectorDirection.Left:
                                _direction = ConnectorDirection.Front;
                                break;
                            case ConnectorDirection.Right:
                                _direction = ConnectorDirection.Back;
                                break;
                        }
                    } else if (rotation.y == 180) { 
                        switch (this.direction) {
                            case ConnectorDirection.Up:
                                _direction = ConnectorDirection.Up;
                                break;
                            case ConnectorDirection.Down:
                                _direction = ConnectorDirection.Down;
                                break;
                            case ConnectorDirection.Front:
                                _direction = ConnectorDirection.Back;
                                break;
                            case ConnectorDirection.Back:
                                _direction = ConnectorDirection.Front;
                                break;
                            case ConnectorDirection.Left:
                                _direction = ConnectorDirection.Right;
                                break;
                            case ConnectorDirection.Right:
                                _direction = ConnectorDirection.Left;
                                break;
                        }
                    } else if (rotation.y == 270) { 
                        switch (this.direction) {
                            case ConnectorDirection.Up:
                                _direction = ConnectorDirection.Up;
                                break;
                            case ConnectorDirection.Down:
                                _direction = ConnectorDirection.Down;
                                break;
                            case ConnectorDirection.Front:
                                _direction = ConnectorDirection.Left;
                                break;
                            case ConnectorDirection.Back:
                                _direction = ConnectorDirection.Right;
                                break;
                            case ConnectorDirection.Left:
                                _direction = ConnectorDirection.Back;
                                break;
                            case ConnectorDirection.Right:
                                _direction = ConnectorDirection.Front;
                                break;
                        }
                    } else if (rotation.y == 0 || rotation.y == 360) {
                        _direction = this.direction;
                    }
                    newConstraint = (GridElementConstraint)new GridElementNeighborConstraint() { direction=_direction, type=_type };
                    break;

            }
            return newConstraint;
        }
        public bool IsSatisfied(List<GridElementConstraint> otherConstraints) {
            bool satisfied = false;
            foreach (GridElementConstraint constraint in otherConstraints) { 
                GridElementNeighborConstraint neighbor = (GridElementNeighborConstraint)constraint;
                int currentDirection = (int)this.direction-1;
                int neighborDirection = (int)neighbor.direction-1;
                satisfied |= (((currentDirection % 2 == 0) && ((int)neighbor.direction % 2 == 1) && (neighborDirection - currentDirection == 1))
                    || ((currentDirection % 2 == 1) && ((int)neighbor.direction % 2 == 0) && (neighborDirection - currentDirection == -1))
                    && (this.type == neighbor.type));
            }
            return satisfied;
        }
    }
}
