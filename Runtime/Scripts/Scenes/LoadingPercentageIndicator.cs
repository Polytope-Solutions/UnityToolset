using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.UI;

namespace PolytopeSolutions.Toolset.Scenes {
    public class LoadingPercentageIndicator : PercentageIndicator {
        protected override float targetProgress => (float)SceneManagerExtender.Instance?.LoadingProgress;
    }
}