#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace PolytopeSolutions.Toolset.Input {
    public class UIToWorldInputHandler : ExposedActionInputHandler<UIToWorldInputReceiver>, IPointerEnterHandler, IPointerExitHandler {
        ///////////////////////////////////////////////////////////////////////
        // Inform the input receiver that this handler is being hovered over
        public void OnPointerEnter(PointerEventData eventData) {
            #if DEBUG2
            this.Log(Pointer Enter");
            #endif
            ((IInputHandler)this).RegisterInputHandler();
        }
        // Inform the input receiver that this handler is not being hovered over anymore
        public void OnPointerExit(PointerEventData eventData) {
            #if DEBUG2
            this.Log(Pointer Exit");
            #endif
            ((IInputHandler)this).UnregisterInputHandler();
        }
    }
}