using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public interface IActivityProvider {
        bool IsActive { get; }
        event System.Action OnActivityStateChanged;
    }
}