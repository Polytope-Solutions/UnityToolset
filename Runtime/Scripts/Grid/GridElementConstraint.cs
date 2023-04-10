using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using System.Linq;
using System;

namespace PolytopeSolutions.Toolset.Grid {
    public interface GridElementConstraint {
        public GridElementConstraint Modify(int modifierType, object modifiers);
        public bool IsSatisfied(params object[] options);
    }
}
