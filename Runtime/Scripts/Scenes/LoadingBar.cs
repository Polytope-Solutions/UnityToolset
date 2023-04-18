using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.UI;

namespace PolytopeSolutions.Toolset.Scenes {
    public class LoadingBar : ProgressBar {
        protected override float progress => (float)SceneManagerExtender.instance?.Progress;
    }
}