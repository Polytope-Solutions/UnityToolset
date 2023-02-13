using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UI_TMPRO
using TMPro;
#endif

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
        public static void SetText(this GameObject goItem, string text){
            {
                Text textHolder = goItem.GetComponent<Text>();
                if (textHolder != null)
                    textHolder.text = text;
            }
            {
                TextMesh textHolder = goItem.GetComponent<TextMesh>();
                if (textHolder != null)
                    textHolder.text = text;
            }
            #if UI_TMPRO
            {
                TextMeshPro textHolder = goItem.GetComponent<TextMeshPro>();
                if (textHolder != null)
                    textHolder.text = text;
            }
            {
                TextMeshProUGUI textHolder = goItem.GetComponent<TextMeshProUGUI>();
                if (textHolder != null)
                    textHolder.text = text;
            }
            #endif
		}
    }
}