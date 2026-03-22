using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class PatrolRoute : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private Vector3[] patrolPoints;
        [SerializeField] private float waitTimeAtPoint = 2f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        
        [Header("Patrol Behavior")]
        [SerializeField] private bool loopPatrol = true;
        [SerializeField] private bool randomizePatrol = false;
        [SerializeField] private bool pausePatrolOnAlert = true;
        
        [Header("Look Around")]
        [SerializeField] private bool lookAroundAtPoints = true;
        [SerializeField] private float lookAroundAngle = 45f;
        [SerializeField] private float lookAroundDuration = 1f;
        
        [Header("Visual Debug")]
        [SerializeField] private bool showPatrolPath = true;
        [SerializeField] private Color patrolPathColor = Color.yellow;
        
        private AIController aiController;
        private int currentPointIndex = 0;
        private bool isWaiting = false;
        private float waitEndTime = 0f;
        private bool isPatrolActive = true;
        private bool isLookingAround = false;
        private float lookAroundEndTime = 0f;
        private Vector3 lookAroundStartRotation;
        private List<int> unusedPointIndices = new List<int>();
        
        public Vector3[] GetPatrolPoints()
        {
            return patrolPoints;
        }
        
        public int GetPointCount()
        {
            return patrolPoints != null ? patrolPoints.Length : 0;
        }
        
        public Vector3 GetCurrentTargetPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                return Vector3.zero;
            }
            
            return patrolPoints[currentPointIndex];
        }
        
        public Vector3 GetNextTargetPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                return Vector3.zero;
            }
            
            int nextIndex = GetNextPointIndex();
            return patrolPoints[nextIndex];
        }
        
        public void Initialize(AIController controller)
        {
            aiController = controller;
            
            unusedPointIndices.Clear();
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                unusedPointIndices.Add(i);
            }
            
            if (randomizePatrol && unusedPointIndices.Count > 0)
            {
                int randomIndex = Random.Range(0, unusedPointIndices.Count);
                currentPointIndex = unusedPointIndices[randomIndex];
                unusedPointIndices.RemoveAt(randomIndex);
            }
            else
            {
                currentPointIndex = 0;
            }
        }
        
        public void StartPatrol()
        {
            isPatrolActive = true;
            isWaiting = false;
        }
        
        public void StopPatrol()
        {
            isPatrolActive = false;
            isWaiting = false;
        }
        
        public void UpdatePatrol()
        {
            if (!isPatrolActive || aiController == null)
            {
                return;
            }
            
            if (aiController.IsAlerted() && pausePatrolOnAlert)
            {
                return;
            }
            
            if (isWaiting)
            {
                HandleWaiting();
                return;
            }
            
            if (isLookingAround)
            {
                HandleLookAround();
                return;
            }
            
            MoveToCurrentPoint();
        }
        
        private void MoveToCurrentPoint()
        {
            Vector3 targetPoint = GetCurrentTargetPoint();
            Vector3 direction = targetPoint - aiController.transform.position;
            direction.y = 0f;
            
            if (direction.magnitude < 0.5f)
            {
                ArrivedAtPoint();
                return;
            }
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            aiController.transform.rotation = Quaternion.Slerp(
                aiController.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            
            aiController.MoveTo(targetPoint, moveSpeed);
        }
        
        private void ArrivedAtPoint()
        {
            if (lookAroundAtPoints)
            {
                StartLookAround();
                return;
            }
            
            StartWaiting();
        }
        
        private void StartWaiting()
        {
            isWaiting = true;
            waitEndTime = Time.time + waitTimeAtPoint;
            aiController.StopMovement();
        }
        
        private void HandleWaiting()
        {
            if (Time.time >= waitEndTime)
            {
                isWaiting = false;
                MoveToNextPoint();
            }
        }
        
        private void StartLookAround()
        {
            isLookingAround = true;
            lookAroundEndTime = Time.time + lookAroundDuration;
            lookAroundStartRotation = aiController.transform.eulerAngles;
            aiController.StopMovement();
        }
        
        private void HandleLookAround()
        {
            if (Time.time >= lookAroundEndTime)
            {
                isLookingAround = false;
                aiController.transform.rotation = Quaternion.Euler(lookAroundStartRotation);
                StartWaiting();
                return;
            }
            
            float lookProgress = (Time.time - (lookAroundEndTime - lookAroundDuration)) / lookAroundDuration;
            float angle = Mathf.Sin(lookProgress * Mathf.PI) * lookAroundAngle;
            
            Vector3 currentRotation = lookAroundStartRotation;
            currentRotation.y += angle;
            
            aiController.transform.rotation = Quaternion.Euler(currentRotation);
        }
        
        private void MoveToNextPoint()
        {
            int nextIndex = GetNextPointIndex();
            
            if (randomizePatrol)
            {
                if (unusedPointIndices.Count == 0)
                {
                    if (loopPatrol)
                    {
                        for (int i = 0; i < patrolPoints.Length; i++)
                        {
                            unusedPointIndices.Add(i);
                        }
                    }
                    else
                    {
                        isPatrolActive = false;
                        return;
                    }
                }
                
                int randomIndex = Random.Range(0, unusedPointIndices.Count);
                currentPointIndex = unusedPointIndices[randomIndex];
                unusedPointIndices.RemoveAt(randomIndex);
            }
            else
            {
                currentPointIndex = nextIndex;
                
                if (!loopPatrol && currentPointIndex >= patrolPoints.Length)
                {
                    isPatrolActive = false;
                }
            }
        }
        
        private int GetNextPointIndex()
        {
            if (loopPatrol)
            {
                return (currentPointIndex + 1) % patrolPoints.Length;
            }
            else
            {
                return currentPointIndex + 1;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showPatrolPath || patrolPoints == null || patrolPoints.Length == 0)
            {
                return;
            }
            
            Gizmos.color = patrolPathColor;
            
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.5f);
                
                int nextIndex = (i + 1) % patrolPoints.Length;
                if (i < patrolPoints.Length - 1 || loopPatrol)
                {
                    Gizmos.DrawLine(patrolPoints[i], patrolPoints[nextIndex]);
                }
            }
        }
        
        public bool IsPatrolActive()
        {
            return isPatrolActive;
        }
        
        public int GetCurrentPointIndex()
        {
            return currentPointIndex;
        }
        
        public float GetMoveSpeed()
        {
            return moveSpeed;
        }
        
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        public void SetWaitTime(float time)
        {
            waitTimeAtPoint = time;
        }
        
        public void SetLoopPatrol(bool loop)
        {
            loopPatrol = loop;
        }
        
        public void SetRandomizePatrol(bool random)
        {
            randomizePatrol = random;
        }
    }
}
