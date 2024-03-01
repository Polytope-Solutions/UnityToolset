using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using TMPro;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    public class TMPLinkOpenner : MonoBehaviour, IPointerClickHandler { 
        public void OnPointerClick(PointerEventData eventData) {
            TextMeshProUGUI pTextMeshPro = GetComponent<TextMeshProUGUI>();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, null);  
            if (linkIndex != -1) { // was a link clicked?
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }

    }
}
