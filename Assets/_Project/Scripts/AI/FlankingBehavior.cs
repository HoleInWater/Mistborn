using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.AI
{
    public class FlankingBehavior : MonoBehaviour
    {
        [Header("Flanking Configuration")]
        [SerializeField] private float flankingDistance = 5f;
        [SerializeField] private float flankingAngle = 45f;
        [SerializeField] private float approachSpeed = 1.5f;
        [SerializeField] private float attackDelay = 0.5f;
        
        [Header("Flanking Stages")]
        [SerializeField] private float detectionRange = 20f;
        [SerializeField] private float engageRange = 10f;
        [SerializeField] private float abortRange = 3f;
        
        [Header("Coordination")]
        [SerializeField] private bool coordinateWithAllies = true;
        [SerializeField] private float allyCommunicationRadius = 15f;
        [SerializeField] private int maxFlankingAllies = 2;
        
        [Header("Adaption")]
        [SerializeField] private bool adaptToPlayerMovement = true;
        [SerializeField] private float predictionTime = 0.5f;
        
        private enum FlankingState
        {
            Idle,
            Detecting,
            Positioning,
            Attacking,
            Retreating
        }
        
        private FlankingState currentState = FlankingState.Idle;
        private Transform target;
        private Vector3 flankPosition;
        private bool isFlanking = false;
        private List<Transform> flankingAllies = new List<Transform>();
        private UnityEngine.AI.NavMeshAgent agent;
        private Enemy.Enemy enemy;
        
        private void Start()
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            enemy = GetComponent<Enemy.Enemy>();
        }
        
        private void Update()
        {
            switch (currentState)
            {
                case FlankingState.Idle:
                    CheckForTargets();
                    break;
                case FlankingState.Detecting:
                    TrackTarget();
                    break;
                case FlankingState.Positioning:
                    MoveToFlankPosition();
                    break;
                case FlankingState.Attacking:
                    ExecuteAttack();
                    break;
                case FlankingState.Retreating:
                    RetreatFromCombat();
                    break;
            }
            
            if (coordinateWithAllies)
            {
                UpdateFlankingAllies();
            }
        }
        
        private void CheckForTargets()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, detectionRange);
            
            foreach (var col in targets)
            {
                if (col.CompareTag("Player"))
                {
                    target = col.transform;
                    currentState = FlankingState.Detecting;
                    
                    if (coordinateWithAllies)
                    {
                        BroadcastFlankingIntent();
                    }
                    break;
                }
            }
        }
        
        private void TrackTarget()
        {
            if (target == null)
            {
                currentState = FlankingState.Idle;
                return;
            }
            
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance <= engageRange)
            {
                CalculateFlankPosition();
                currentState = FlankingState.Positioning;
            }
            else if (distance > detectionRange)
            {
                currentState = FlankingState.Idle;
                target = null;
            }
        }
        
        private void CalculateFlankPosition()
        {
            Vector3 toTarget = (target.position - transform.position).normalized;
            Vector3 perpendicular = Vector3.Cross(Vector3.up, toTarget);
            
            float direction = DetermineFlankDirection();
            
            Vector3 flankDir = perpendicular * direction;
            flankPosition = target.position + flankDir * flankingDistance - toTarget * 3f;
            flankPosition.y = transform.position.y;
            
            if (adaptToPlayerMovement && target.GetComponent<Rigidbody>() != null)
            {
                Rigidbody targetRb = target.GetComponent<Rigidbody>();
                Vector3 predictedPosition = target.position + targetRb.velocity * predictionTime;
                flankPosition = predictedPosition + flankDir * flankingDistance - toTarget * 3f;
                flankPosition.y = transform.position.y;
            }
            
            if (IsValidPosition(flankPosition))
            {
                isFlanking = true;
            }
            else
            {
                flankPosition = GetAlternativeFlankPosition(direction);
            }
        }
        
        private float DetermineFlankDirection()
        {
            if (!coordinateWithAllies)
            {
                return Random.value < 0.5f ? 1f : -1f;
            }
            
            int leftFlankers = 0;
            int rightFlankers = 0;
            
            foreach (var ally in flankingAllies)
            {
                if (ally == null)
                    continue;
                
                Vector3 toAlly = (ally.position - target.position).normalized;
                Vector3 toSelf = (transform.position - target.position).normalized;
                
                float cross = Vector3.Cross(toAlly, toSelf).y;
                
                if (cross > 0)
                    leftFlankers++;
                else
                    rightFlankers++;
            }
            
            return leftFlankers <= rightFlankers ? 1f : -1f;
        }
        
        private Vector3 GetAlternativeFlankPosition(float preferredDirection)
        {
            float[] angles = { flankingAngle, -flankingAngle, flankingAngle * 1.5f, -flankingAngle * 1.5f };
            
            foreach (float angle in angles)
            {
                Vector3 rotatedDir = Quaternion.Euler(0, angle, 0) * (target.forward);
                Vector3 testPosition = target.position + rotatedDir * flankingDistance;
                
                if (IsValidPosition(testPosition))
                {
                    return testPosition;
                }
            }
            
            return transform.position;
        }
        
        private bool IsValidPosition(Vector3 position)
        {
            UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
            
            if (agent != null)
            {
                agent.CalculatePath(position, path);
                return path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
            }
            
            return true;
        }
        
        private void MoveToFlankPosition()
        {
            if (target == null || agent == null)
            {
                currentState = FlankingState.Idle;
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            float distanceToFlank = Vector3.Distance(transform.position, flankPosition);
            
            if (distanceToFlank <= 1f)
            {
                currentState = FlankingState.Attacking;
                Invoke(nameof(ExecuteFlankAttack), attackDelay);
                return;
            }
            
            if (distanceToTarget < abortRange)
            {
                currentState = FlankingState.Retreating;
                return;
            }
            
            if (agent != null)
            {
                agent.SetDestination(flankPosition);
                agent.speed = approachSpeed;
            }
        }
        
        private void ExecuteFlankAttack()
        {
            if (enemy != null && target != null)
            {
                enemy.transform.LookAt(target);
                enemy.Attack();
            }
            
            isFlanking = false;
            currentState = FlankingState.Detecting;
        }
        
        private void ExecuteAttack()
        {
            if (target == null)
            {
                currentState = FlankingState.Idle;
                return;
            }
            
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance > engageRange * 1.5f)
            {
                currentState = FlankingState.Detecting;
            }
        }
        
        private void RetreatFromCombat()
        {
            if (target == null)
            {
                currentState = FlankingState.Idle;
                return;
            }
            
            Vector3 retreatDirection = (transform.position - target.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 5f;
            
            if (agent != null)
            {
                agent.SetDestination(retreatPosition);
            }
            
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance > engageRange)
            {
                currentState = FlankingState.Detecting;
            }
        }
        
        private void BroadcastFlankingIntent()
        {
            Collider[] allies = Physics.OverlapSphere(transform.position, allyCommunicationRadius);
            
            foreach (var ally in allies)
            {
                FlankingBehavior flankingBehavior = ally.GetComponent<FlankingBehavior>();
                if (flankingBehavior != null && flankingBehavior != this)
                {
                    flankingBehavior.RegisterFlankingAlly(transform);
                }
            }
        }
        
        private void UpdateFlankingAllies()
        {
            flankingAllies.RemoveAll(ally => ally == null);
            
            Collider[] allies = Physics.OverlapSphere(transform.position, allyCommunicationRadius);
            
            foreach (var ally in allies)
            {
                FlankingBehavior flankingBehavior = ally.GetComponent<FlankingBehavior>();
                if (flankingBehavior != null && flankingBehavior != this && flankingBehavior.IsFlanking)
                {
                    if (!flankingAllies.Contains(ally.transform))
                    {
                        flankingAllies.Add(ally.transform);
                    }
                }
            }
        }
        
        public void RegisterFlankingAlly(Transform ally)
        {
            if (!flankingAllies.Contains(ally) && flankingAllies.Count < maxFlankingAllies)
            {
                flankingAllies.Add(ally);
            }
        }
        
        public bool IsFlanking => isFlanking;
        public FlankingState CurrentState => currentState;
    }
}
