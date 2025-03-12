using System;
using System.Collections;
using UnityEngine;

using UnityEngine.AI;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    [RequireComponent(typeof(AvatarAnimationController))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class AvatarAnimationNavMeshMoveToTarget : MonoBehaviour, IAvatarMoveProvider {
        [SerializeField] private float proximityThreshold = 0.01f;
        [SerializeField] private float rotateSmoothTime = 0.2f;
        
        private AvatarAnimationController aniamationController;
        private NavMeshAgent agent;

        private float sqrProximityThreshold;
        private Vector3 targetPosition;

        private Coroutine prepareMoveCoroutine;
        private NavMeshPath preplanPath;
        private Quaternion startRotation, targetRotation;
        private float startTime, t = 0f;


        private void Awake() {
            this.sqrProximityThreshold = this.proximityThreshold * this.proximityThreshold;
            this.preplanPath = new NavMeshPath();
            this.agent = GetComponent<NavMeshAgent>();
            this.aniamationController = GetComponent<AvatarAnimationController>();
            this.aniamationController.SetAvatarMoveProvider(this);
        }
        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.targetPosition, 0.1f);
        }
        ///////////////////////////////////////////////////////////////////////
        private void UpdateDestination(Vector3 targetPosition) {
            this.targetPosition = targetPosition;
            this.agent.SetDestination(this.targetPosition);
        }

        public void MoveToTarget(Vector3 targetPosition) {
            if (this.prepareMoveCoroutine != null)
                StopCoroutine(this.prepareMoveCoroutine);
            this.prepareMoveCoroutine = StartCoroutine(PrepareMove(targetPosition));
        }

        public (Vector3 velocity, float maxSpeed) GetMoveState()
            => (this.agent.velocity, this.agent.speed);

        public void OnInterruptMove() {
            if (this.prepareMoveCoroutine != null)
                StopCoroutine(this.prepareMoveCoroutine);
            if ((transform.position - this.targetPosition).sqrMagnitude > this.sqrProximityThreshold) 
                UpdateDestination(transform.position);
        }

        private IEnumerator PrepareMove(Vector3 targetPosition) {
            this.preplanPath.ClearCorners();
            if (!this.agent.CalculatePath(targetPosition, this.preplanPath) || this.preplanPath.corners.Length == 0) { 
                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 10, NavMesh.AllAreas))
                    targetPosition = hit.position;
                if (!this.agent.CalculatePath(targetPosition, this.preplanPath) || this.preplanPath.corners.Length == 0)
                    yield break;
            }

            this.startTime = Time.time;
            this.startRotation = transform.rotation;
            Vector3 newForward = (this.preplanPath.corners[1] - transform.position).normalized;
            this.targetRotation = Quaternion.LookRotation(newForward, Vector3.up);
            do {
                this.t = Mathf.InverseLerp(0f, this.rotateSmoothTime, Time.time - this.startTime);
                transform.rotation = Quaternion.Slerp(this.startRotation, this.targetRotation, this.t);
                yield return null;
            } while (this.t < 1f);
            transform.rotation = this.targetRotation;
            UpdateDestination(targetPosition);
        }
    }
}