using System;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public interface IAvatarMoveProvider {
        public (Vector3 velocity, float maxSpeed) GetMoveState();
        public void OnInterruptMove();
    }
}