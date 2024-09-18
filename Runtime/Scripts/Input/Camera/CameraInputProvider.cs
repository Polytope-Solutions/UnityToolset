using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Input {
    [RequireComponent(typeof(CameraController))]
    public abstract class CameraInputProvider : InputReceiver {
        private CameraController cameraController;

        protected Camera Camera => this.cameraController.Camera;
        protected Vector3 ObjectProxyUp => this.cameraController.ObjectProxy.up;
        protected Vector3 ObjectProxyPosition => this.cameraController.ObjectProxy.position;
        protected Vector3 CameraProxyPosition => this.cameraController.CameraProxy.position;
        protected Vector3 CameraProxyRight => this.cameraController.CameraProxy.right;
        protected Vector3 CameraProxyForward => this.cameraController.CameraProxy.forward;
        protected Vector3 TargetProxyPosition => this.cameraController.TargetProxy.position;
        protected float TargetProximity => this.cameraController.TargetProximity;
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
        protected void ModifyRigDirect(Vector3 upDirection, Vector3 rotationOriginPrevious, Vector3 rotationOrigin, Quaternion rotation, float scale, float angleUpDown, bool isTilting) {
            this.cameraController.ObjectProxy.up = upDirection;

            if (isTilting) {
                this.cameraController.CameraProxy.RotateAround(
                    this.cameraController.TargetProxy.position,
                    this.cameraController.CameraProxy.right,
                    angleUpDown
                );

                rotationOrigin = Vector3.zero;
                rotationOriginPrevious = Vector3.zero;
                scale = 1;
                rotation = Quaternion.identity;
            }

            Vector3 objectOffset = this.ObjectProxyPosition - rotationOrigin;
            Vector3 targetOffset = this.TargetProxyPosition - rotationOrigin;
            Vector3 targetCameraOffset = this.CameraProxyPosition - this.TargetProxyPosition;

            Quaternion inverse = Quaternion.Inverse(rotation);
            //Quaternion rotation = Quaternion.AngleAxis(-angle, upDirection);
            //Vector3 delta = rotationOrigin - rotationOriginPrevious;


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

        Vector3 constrainLookDirection, constrainInPlaneNormal, constrainHorizontalDirection, constrainCameraLocalUp;
        float constrainDistance, constrainVerticalAngle;
        protected void ConstrainCameraToTarget(Vector2 verticalAngleRange) {
            // target constraint
            constrainLookDirection = this.TargetPositionClamped;
            constrainDistance = constrainLookDirection.magnitude;
            // Ensure camera is in correct angular position
            constrainInPlaneNormal = Vector3.Cross(constrainLookDirection, this.ObjectProxyUp);
            constrainHorizontalDirection = Vector3.ProjectOnPlane(constrainLookDirection, this.ObjectProxyUp);
            constrainVerticalAngle = Vector3.Angle(constrainHorizontalDirection, constrainLookDirection);
            constrainVerticalAngle = Mathf.Clamp(constrainVerticalAngle, verticalAngleRange.x, verticalAngleRange.y);
            constrainLookDirection = Quaternion.AngleAxis(-constrainVerticalAngle, constrainInPlaneNormal) * constrainHorizontalDirection.normalized;
            constrainCameraLocalUp = Vector3.Cross(constrainLookDirection, this.CameraProxyRight);
            constrainLookDirection *= constrainDistance;

            // Update the position and rotation of the camera
            this.cameraController.CameraProxy.position = this.cameraController.TargetProxy.position - constrainLookDirection;
            this.cameraController.CameraProxy.LookAt(this.cameraController.TargetProxy, constrainCameraLocalUp);
        }
        #endregion
    }
}
