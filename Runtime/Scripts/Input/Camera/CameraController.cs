using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    [RequireComponent(typeof(Rigidbody))]
    public abstract class CameraController : InputReceiver {
        // NB! If overriding OnEnable call the base version in derived classes
        // ignore input handlers as this object itself is both receiver and handler.

        [Header("General")]
        [SerializeField] protected Transform tCamera;
        [SerializeField] protected bool useProxies;

        protected new Rigidbody objectRigidbody;
        protected Camera cCamera;

        private readonly float epsilon = 0.0001f;
        private Coroutine movementMonitor;
        private float farCornerDistanceCache;
        public event Action onCameraViewChanged = null;

        protected Transform tObjectProxy;
        protected Transform tCameraProxy;

        private Vector3 objectMovementVelocity, objectRotationVelocity;
        private Vector3 cameraMovementVelocity, cameraRotationVelocity;
        [SerializeField] protected float smoothTime = 0.1f;

        protected override void OnEnable() { 
            base.OnEnable();
            ObjectSetup();
        }
        protected virtual void LateUpdate() {
            ApplyProxies();
        }
        protected virtual void ObjectSetup() {
            this.cCamera = this.tCamera?.GetComponent<Camera>();
            if (this.tCamera == null){
                this.cCamera = Camera.main;
                this.tCamera = this.cCamera?.transform;
            }

            if (this.useProxies) {
                if (!this.tObjectProxy) { 
                    this.tObjectProxy = TryFindOrAddByName("ObjectProxy").transform;
                    if (transform.parent)
                        this.tObjectProxy.SetParent(transform.parent);
                    this.tObjectProxy.gameObject.tag = transform.gameObject.tag;
                    this.tObjectProxy.gameObject.layer = transform.gameObject.layer;
                    this.tObjectProxy.position = transform.position;
                    this.tObjectProxy.rotation = transform.rotation;
                    this.tObjectProxy.localScale = transform.localScale;
                }
                if (!this.tCameraProxy) { 
                    this.tCameraProxy = this.tObjectProxy.gameObject.TryFindOrAddByName("CameraProxy").transform;
                    this.tCameraProxy.position = this.tCamera.position;
                    this.tCameraProxy.rotation = this.tCamera.rotation;
                    this.tCameraProxy.localScale = this.tCamera.localScale;
                }

                Rigidbody currentRigidbody = GetComponent<Rigidbody>();
                this.objectRigidbody = this.tObjectProxy.gameObject.AddComponent<Rigidbody>();
                this.objectRigidbody.mass = currentRigidbody.mass;
                this.objectRigidbody.drag = currentRigidbody.drag;
                this.objectRigidbody.angularDrag = currentRigidbody.angularDrag;
                this.objectRigidbody.useGravity = currentRigidbody.useGravity;
                this.objectRigidbody.constraints = currentRigidbody.constraints;
                this.objectRigidbody.interpolation = currentRigidbody.interpolation;
                this.objectRigidbody.collisionDetectionMode = currentRigidbody.collisionDetectionMode;
                this.objectRigidbody.isKinematic = currentRigidbody.isKinematic;
                this.objectRigidbody.automaticCenterOfMass = currentRigidbody.automaticCenterOfMass;
                this.objectRigidbody.automaticInertiaTensor = currentRigidbody.automaticInertiaTensor;
                currentRigidbody.isKinematic = true;
                Collider currentCollider = GetComponent<Collider>();
                if (currentCollider) { 
                    Collider objectCollider = this.tObjectProxy.gameObject.CopyComponent<Collider>(currentCollider);
                    currentCollider.enabled = false;
                }
            }
            else {
                this.tObjectProxy = transform;
                this.tCameraProxy = this.tCamera;
            }

            float frustumHeight = 2.0f * this.cCamera.farClipPlane * Mathf.Tan(this.cCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * this.cCamera.aspect;
            this.farCornerDistanceCache = Mathf.Sqrt(
                Mathf.Pow(this.cCamera.farClipPlane, 2) +
                Mathf.Pow(frustumHeight / 2, 2) +
                Mathf.Pow(frustumWidth / 2, 2)
            );

            if (this.movementMonitor == null)
                this.movementMonitor = StartCoroutine(MovementMonitor());
        }
        protected virtual void ApplyProxies() {
            if (this.useProxies) {
                transform.position = Vector3.SmoothDamp(transform.position, this.tObjectProxy.position, ref this.objectMovementVelocity, this.smoothTime);
                transform.rotation = Quaternion.Euler(
                    Mathf.SmoothDampAngle(transform.rotation.eulerAngles.x, this.tObjectProxy.rotation.eulerAngles.x, ref this.objectRotationVelocity.x, this.smoothTime),
                    Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, this.tObjectProxy.rotation.eulerAngles.y, ref this.objectRotationVelocity.y, this.smoothTime),
                    Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, this.tObjectProxy.rotation.eulerAngles.z, ref this.objectRotationVelocity.z, this.smoothTime)
                );
                //transform.localRotation = Quaternion.RotateTowards(transform.localRotation, this.tObjectProxy.localRotation, this.maxDegreesDelta * Time.deltaTime);
                this.tCamera.position = Vector3.SmoothDamp(this.tCamera.position, this.tCameraProxy.position, ref this.cameraMovementVelocity, this.smoothTime);
                this.tCamera.rotation = Quaternion.Euler(
                    Mathf.SmoothDampAngle(this.tCamera.rotation.eulerAngles.x, this.tCameraProxy.rotation.eulerAngles.x, ref this.cameraRotationVelocity.x, this.smoothTime),
                    Mathf.SmoothDampAngle(this.tCamera.rotation.eulerAngles.y, this.tCameraProxy.rotation.eulerAngles.y, ref this.cameraRotationVelocity.y, this.smoothTime),
                    Mathf.SmoothDampAngle(this.tCamera.rotation.eulerAngles.z, this.tCameraProxy.rotation.eulerAngles.z, ref this.cameraRotationVelocity.z, this.smoothTime)
                );
                //this.tCamera.localRotation = Quaternion.RotateTowards(this.tCamera.localRotation, this.tCameraProxy.localRotation, this.maxDegreesDelta * Time.deltaTime);
            }
        }
        private IEnumerator MovementMonitor() {
            Vector3 lastPosition, currentPosition;
            Quaternion lastRotation, currentRotation;
            lastPosition = this.tCamera.position;
            lastRotation = this.tCamera.rotation;
            while (true) {
                currentPosition = this.tCamera.position;
                currentRotation = this.tCamera.rotation;

                if (this.onCameraViewChanged != null
                    && ((currentPosition - lastPosition).sqrMagnitude > this.epsilon
                        || (currentRotation.ToVector4() - lastRotation.ToVector4()).sqrMagnitude > this.epsilon))
                    this.onCameraViewChanged?.Invoke();
                lastPosition = currentPosition;
                lastRotation = currentRotation;
                yield return new WaitForEndOfFrame();
            }
        }
        ///////////////////////////////////////////////////////////////////////
        public List<Vector3> FieldOfViewBoundaries(Plane plane) { 
            List<Vector3> boundaryPoints = new List<Vector3>();
            foreach (Vector3 corner in 
                new List<Vector3> { 
                    Vector3.zero, 
                    new Vector3(0, 1, 0), 
                    new Vector3(1, 1, 0), 
                    new Vector3(1, 0, 0) 
                }) {
                Ray temp = this.cCamera.ViewportPointToRay(corner);
                if (plane.Raycast(temp, out float distance)){
                    distance = Mathf.Clamp(distance, 0, this.farCornerDistanceCache);
                    boundaryPoints.Add(temp.origin + temp.direction * distance);
                } else {
                    // Ray does not intersect with plane - snap the corner to the relevant plane
                    boundaryPoints.Add(
                        plane.ClosestPointOnPlane(temp.origin + temp.direction * this.farCornerDistanceCache)
                    );
                }
            }
            return boundaryPoints;
        }
    }
}