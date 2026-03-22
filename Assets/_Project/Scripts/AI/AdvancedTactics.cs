using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.AI
{
    public enum TacticType
    {
        Aggressive,
        Defensive,
        Balanced,
        Ambush,
        Flanking,
        Rush,
        TargetPriority,
        AreaControl
    }
    
    public class AdvancedTactics : MonoBehaviour
    {
        [Header("Tactics Configuration")]
        [SerializeField] private TacticType currentTactic = TacticType.Balanced;
        [SerializeField] private float tacticChangeInterval = 10f;
        [SerializeField] private float tacticAdaptiveness = 0.3f;
        
        [Header("Target Selection")]
        [SerializeField] private bool prioritizeLowHealth = true;
        [SerializeField] private bool prioritizeAllomancers = true;
        [SerializeField] private bool prioritizeRanged = false;
        [SerializeField] private float threatMultiplier = 2f;
        
        [Header("Positioning")]
        [SerializeField] private float preferredAttackRange = 3f;
        [SerializeField] private float maintainDistance = 5f;
        [SerializeField] private float flankingAngle = 45f;
        
        [Header("Group Tactics")]
        [SerializeField] private bool useGroupTactics = true;
        [SerializeField] private float groupCohesionRadius = 10f;
        [SerializeField] private int minGroupSize = 2;
        
        [Header("Environment Awareness")]
        [SerializeField] private bool avoidHazards = true;
        [SerializeField] private bool useHighGround = true;
        [SerializeField] private float coverWeight = 1.5f;
        
        [Header("Ability Usage")]
        [SerializeField] private bool saveAbilitiesForOpportune = true;
        [SerializeField] private float abilityTriggerThreshold = 0.7f;
        [SerializeField] private float aoeAbilityThreshold = 0.4f;
        
        private Enemy.Enemy enemy;
        private Transform currentTarget;
        private List<Transform> nearbyAllies = new List<Transform>();
        private List<Transform> nearbyEnemies = new List<Transform>();
        private Vector3 optimalPosition;
        private float lastTacticChange = 0f;
        private float tacticEfficiency = 1f;
        
        private void Start()
        {
            enemy = GetComponent<Enemy.Enemy>();
        }
        
        private void Update()
        {
            if (enemy == null)
                return;
            
            if (Time.time - lastTacticChange > tacticChangeInterval)
            {
                EvaluateAndChangeTactic();
            }
            
            UpdateAwareness();
            CalculateOptimalPosition();
        }
        
        private void UpdateAwareness()
        {
            nearbyAllies.Clear();
            nearbyEnemies.Clear();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, groupCohesionRadius);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Enemy") && col.gameObject != gameObject)
                {
                    nearbyAllies.Add(col.transform);
                }
                else if (col.CompareTag("Player"))
                {
                    nearbyEnemies.Add(col.transform);
                }
            }
        }
        
        private void EvaluateAndChangeTactic()
        {
            lastTacticChange = Time.time;
            
            float healthPercent = enemy.CurrentHealth / enemy.MaxHealth;
            float allyCount = nearbyAllies.Count;
            float enemyCount = nearbyEnemies.Count;
            
            TacticType newTactic = DetermineBestTactic(healthPercent, allyCount, enemyCount);
            
            if (newTactic != currentTactic)
            {
                currentTactic = newTactic;
                ApplyTactic();
            }
        }
        
        private TacticType DetermineBestTactic(float healthPercent, float allyCount, float enemyCount)
        {
            if (healthPercent < 0.3f)
            {
                return TacticType.Defensive;
            }
            
            if (allyCount >= minGroupSize && useGroupTactics)
            {
                if (enemyCount > allyCount)
                {
                    return TacticType.Flananking;
                }
                else
                {
                    return TacticType.Rush;
                }
            }
            
            if (enemyCount > 2)
            {
                return TacticType.AreaControl;
            }
            
            if (healthPercent > 0.7f && allyCount > 0)
            {
                return TacticType.Aggressive;
            }
            
            return TacticType.Balanced;
        }
        
        private void ApplyTactic()
        {
            switch (currentTactic)
            {
                case TacticType.Aggressive:
                    preferredAttackRange = 2f;
                    maintainDistance = 2f;
                    break;
                case TacticType.Defensive:
                    preferredAttackRange = 4f;
                    maintainDistance = 6f;
                    break;
                case TacticType.Balanced:
                    preferredAttackRange = 3f;
                    maintainDistance = 4f;
                    break;
                case TacticType.Flananking:
                    preferredAttackRange = 3f;
                    maintainDistance = 3f;
                    flankingAngle = 60f;
                    break;
                case TacticType.Rush:
                    preferredAttackRange = 0f;
                    maintainDistance = 0f;
                    break;
                case TacticType.Ambush:
                    preferredAttackRange = 2f;
                    maintainDistance = 0f;
                    break;
                case TacticType.AreaControl:
                    preferredAttackRange = 5f;
                    maintainDistance = 7f;
                    break;
            }
        }
        
        private void CalculateOptimalPosition()
        {
            if (currentTarget == null)
                return;
            
            Vector3 toTarget = currentTarget.position - transform.position;
            float distanceToTarget = toTarget.magnitude;
            
            Vector3 desiredPosition = transform.position;
            
            switch (currentTactic)
            {
                case TacticType.Aggressive:
                    desiredPosition = currentTarget.position;
                    break;
                    
                case TacticType.Defensive:
                    desiredPosition = currentTarget.position - toTarget.normalized * maintainDistance;
                    break;
                    
                case TacticType.Flananking:
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, toTarget.normalized);
                    float direction = Random.value < 0.5f ? 1f : -1f;
                    Vector3 flankDirection = perpendicular * direction;
                    desiredPosition = currentTarget.position + flankDirection * flankingAngle - toTarget.normalized * maintainDistance;
                    break;
                    
                case TacticType.Rush:
                    desiredPosition = currentTarget.position;
                    break;
                    
                case TacticType.AreaControl:
                    desiredPosition = currentTarget.position;
                    break;
            }
            
            if (avoidHazards)
            {
                desiredPosition = AdjustForHazards(desiredPosition);
            }
            
            if (useHighGround)
            {
                desiredPosition = AdjustForHighGround(desiredPosition);
            }
            
            if (useGroupTactics && nearbyAllies.Count >= minGroupSize)
            {
                desiredPosition = AdjustForGroupCohesion(desiredPosition);
            }
            
            optimalPosition = desiredPosition;
        }
        
        private Vector3 AdjustForHazards(Vector3 position)
        {
            Collider[] hazards = Physics.OverlapSphere(position, 2f);
            
            foreach (var hazard in hazards)
            {
                if (hazard.GetComponent<World.Hazard>() != null)
                {
                    Vector3 awayFromHazard = (position - hazard.transform.position).normalized;
                    position += awayFromHazard * 2f;
                }
            }
            
            return position;
        }
        
        private Vector3 AdjustForHighGround(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                if (hit.point.y > transform.position.y + 1f)
                {
                    position = hit.point;
                }
            }
            
            return position;
        }
        
        private Vector3 AdjustForGroupCohesion(Vector3 position)
        {
            Vector3 groupCenter = transform.position;
            
            foreach (var ally in nearbyAllies)
            {
                if (ally != null)
                {
                    groupCenter += ally.position;
                }
            }
            groupCenter /= (nearbyAllies.Count + 1);
            
            Vector3 cohesionForce = (groupCenter - position).normalized * 0.3f;
            position += cohesionForce;
            
            return position;
        }
        
        public Transform SelectTarget(List<Transform> availableTargets)
        {
            if (availableTargets == null || availableTargets.Count == 0)
                return null;
            
            if (availableTargets.Count == 1)
                return availableTargets[0];
            
            Transform bestTarget = null;
            float highestThreat = 0f;
            
            foreach (var target in availableTargets)
            {
                float threat = CalculateThreatLevel(target);
                
                if (threat > highestThreat)
                {
                    highestThreat = threat;
                    bestTarget = target;
                }
            }
            
            currentTarget = bestTarget;
            return bestTarget;
        }
        
        private float CalculateThreatLevel(Transform target)
        {
            float threat = 1f;
            
            if (prioritizeLowHealth)
            {
                PlayerStats stats = target.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    float healthPercent = stats.CurrentHealth / stats.MaxHealth;
                    threat *= (1f - healthPercent) + 1f;
                }
            }
            
            if (prioritizeAllomancers)
            {
                Allomancy.Allomancer allomancer = target.GetComponent<Allomancy.Allomancer>();
                if (allomancer != null && allomancer.IsBurningMetal)
                {
                    threat *= threatMultiplier;
                }
            }
            
            float distance = Vector3.Distance(transform.position, target.position);
            threat *= Mathf.Max(0.5f, 1f - distance / 50f);
            
            return threat;
        }
        
        public Vector3 GetOptimalPosition()
        {
            return optimalPosition;
        }
        
        public TacticType GetCurrentTactic()
        {
            return currentTactic;
        }
        
        public bool ShouldUseAbility()
        {
            if (!saveAbilitiesForOpportune)
                return true;
            
            float opportunityScore = CalculateOpportunityScore();
            return opportunityScore >= abilityTriggerThreshold;
        }
        
        public bool ShouldUseAOEAbility()
        {
            if (nearbyEnemies.Count < 2)
                return false;
            
            float opportunityScore = CalculateOpportunityScore();
            return opportunityScore >= aoeAbilityThreshold;
        }
        
        private float CalculateOpportunityScore()
        {
            float score = 0f;
            
            float healthPercent = enemy.CurrentHealth / enemy.MaxHealth;
            score += (1f - healthPercent) * 0.3f;
            
            float distanceToTarget = currentTarget != null 
                ? Vector3.Distance(transform.position, currentTarget.position) 
                : 10f;
            
            if (distanceToTarget <= preferredAttackRange)
                score += 0.3f;
            
            if (nearbyAllies.Count >= minGroupSize)
                score += 0.2f;
            
            if (enemy.IsStunned)
                score -= 0.5f;
            
            return Mathf.Clamp01(score);
        }
    }
}
