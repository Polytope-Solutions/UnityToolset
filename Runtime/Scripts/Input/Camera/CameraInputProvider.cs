using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PolytopeSolutions.Toolset.Input {
    [RequireComponent(typeof(CameraController))]
    public abstract class CameraInputProvider : InputReceiver {
        private CameraController cameraController;

        protected Camera Camera                 => this.cameraController.Camera;
        protected Vector3 ObjectProxyUp         => this.cameraController.ObjectProxy.up;
        protected Vector3 ObjectProxyPosition   => this.cameraController.ObjectProxy.position;
        protected Vector3 CameraProxyPosition   => this.cameraController.CameraProxy.position;
        protected Vector3 CameraProxyRight      => this.cameraController.CameraProxy.right;
        protected Vector3 CameraProxyForward    => this.cameraController.CameraProxy.forward;
        protected Vector3 TargetProxyPosition   => this.cameraController.TargetProxy.position;
        protected float TargetProximity         => this.cameraController.TargetProximity;
        protected Vector3 TargetPositionClamped => this.cameraController.TargetPositionClamped;

        protected override bool CanHaveHandlers => false; 

        protected override void Awake() {
            base.Awake();
            this.cameraController = GetComponent<CameraController>();
        }
        protected void Start() {
            OnInteractionPerformed();
        }
        //protected override void OnInteractionStarted() {
        //    if (!this.IsSelfManaged)
        //        InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, true);
        //}
        //protected override void OnInteractionEnded() {
        //    if (!this.IsSelfManaged)
        //        InputManager.Instance.InputReceiverRestoreExclusive();
        //}

        #region SPATIAL_ORIENTATION
        // Some ways to update camera rig positioning and orientation.
        // To be called from Input Handling the override of OnInteractionPerformed.
        protected void ModifyRig(Vector3 upDirection, float cameraAngleUpDown, float cameraAngleLeftRight, Vector3 cameraMoveDelta, Vector3 objectPosition) {
            this.cameraController.ObjectProxy.up = upDirection;

            this.cameraController.CameraProxy.RotateAround(
                this.cameraController.TargetProxy.position,
                this.cameraController.CameraProxy.right,
                cameraAngleUpDown
            );
            this.cameraController.CameraProxy.RotateAround(
                this.cameraController.TargetProxy.position,
                upDirection,
                cameraAngleLeftRight
            );

            this.cameraController.CameraProxy.position += cameraMoveDelta;

            this.cameraController.ObjectRigidbody.MovePosition(objectPosition);
        }
        protected void ModifyRigDirect(Vector3 upDirection, Vector3 rotationOriginPrevious, Vector3 rotationOrigin, Quaternion rotation, float scale) {
            this.cameraController.ObjectProxy.up = upDirection;

            Quaternion inverse = Quaternion.Inverse(rotation);
            //Quaternion rotation = Quaternion.AngleAxis(-angle, upDirection);
            //Vector3 delta = rotationOrigin - rotationOriginPrevious;
            Vector3 objectOffset = this.ObjectProxyPosition - rotationOrigin;
            Vector3 targetOffset = this.TargetProxyPosition - rotationOrigin;
            Vector3 targetCameraOffset = this.CameraProxyPosition - this.TargetProxyPosition;

            
            //objectOffset = this.TargetProxyPosition - rotationOrigin;
            //objectOffset *= scale;
            //objectOffset = rotation * objectOffset;
            //objectOffset -= delta;
            //this.cameraController.TargetProxy.position = rotationOriginPrevious + objectOffset;
            
            objectOffset *= scale;
            objectOffset = inverse * objectOffset;
            //objectOffset -= delta;
            this.cameraController.ObjectProxy.position = rotationOriginPrevious + objectOffset;

            targetOffset *= scale;
            targetOffset = inverse * targetOffset;
            //objectOffset -= delta;
            this.cameraController.TargetProxy.position = rotationOriginPrevious + targetOffset;

            targetCameraOffset *= scale;
            targetCameraOffset = inverse * targetCameraOffset;
            this.cameraController.CameraProxy.position = this.TargetProxyPosition + targetCameraOffset;
            this.cameraController.CameraProxy.LookAt(this.TargetProxyPosition, upDirection);// * this.cameraController.CameraProxy.rotation;
        }

        Vector3 lookDirection, inPlaneNormal, horizontalDirection, cameraLocalUp;
        float distance, verticalAngle;
        protected void ConstrainCameraToTarget(Vector2 verticalAngleRange) {
            // target constraint
            lookDirection = this.TargetPositionClamped;
            distance = lookDirection.magnitude;
            // Ensure camera is in correct angular position
            inPlaneNormal = Vector3.Cross(lookDirection, this.ObjectProxyUp);
            horizontalDirection = Vector3.ProjectOnPlane(lookDirection, this.ObjectProxyUp);
            verticalAngle = Vector3.Angle(horizontalDirection, lookDirection);
            verticalAngle = Mathf.Clamp(verticalAngle, verticalAngleRange.x, verticalAngleRange.y);
            lookDirection = Quaternion.AngleAxis(-verticalAngle, inPlaneNormal) * horizontalDirection.normalized;
            cameraLocalUp = Vector3.Cross(lookDirection, this.CameraProxyRight);
            lookDirection *= distance;

            // Update the position and rotation of the camera
            this.cameraController.CameraProxy.position = this.cameraController.TargetProxy.position - lookDirection;
            this.cameraController.CameraProxy.LookAt(this.cameraController.TargetProxy, cameraLocalUp);
        }
        #endregion
    }
}
