using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AISteering : MonoBehaviour
{
    [Header("Steering Settings")]
    public float maxSpeed = 5f;
    public float maxForce = 3f;
    public float arrivalRadius = 2f;
    public float wanderRadius = 5f;
    public float wanderDistance = 10f;
    public float wanderAngle = 0f;
    
    [Header("Weights")]
    public float seekWeight = 1f;
    public float fleeWeight = 1f;
    public float wanderWeight = 0.5f;
    public float avoidanceWeight = 2f;
    
    [Header("Obstacle Avoidance")]
    public float avoidanceRadius = 5f;
    public float avoidanceAhead = 3f;
    public LayerMask obstacleLayer;
    
    private Vector3 velocity;
    private Vector3 target;
    
    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
    }
    
    void Update()
    {
        Vector3 steering = CalculateSteering();
        velocity += steering;
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }
    
    Vector3 CalculateSteering()
    {
        Vector3 steering = Vector3.zero;
        
        steering += Seek(target) * seekWeight;
        steering += Wander() * wanderWeight;
        steering += AvoidObstacles() * avoidanceWeight;
        
        steering = Vector3.ClampMagnitude(steering, maxForce);
        
        return steering;
    }
    
    Vector3 Seek(Vector3 targetPos)
    {
        Vector3 desired = targetPos - transform.position;
        float distance = desired.magnitude;
        
        if (distance < arrivalRadius)
        {
            desired = desired.normalized * (distance / arrivalRadius) * maxSpeed;
        }
        else
        {
            desired = desired.normalized * maxSpeed;
        }
        
        return desired - velocity;
    }
    
    Vector3 Flee(Vector3 targetPos)
    {
        Vector3 desired = transform.position - targetPos;
        desired = desired.normalized * maxSpeed;
        return desired - velocity;
    }
    
    Vector3 Wander()
    {
        wanderAngle += Random.Range(-0.5f, 0.5f) * Time.deltaTime * 5f;
        
        Vector3 circlePos = transform.position + velocity.normalized * wanderDistance;
        Vector3 displacement = new Vector3(
            Mathf.Cos(wanderAngle) * wanderRadius,
            0,
            Mathf.Sin(wanderAngle) * wanderRadius
        );
        
        return (circlePos + displacement - transform.position).normalized * maxForce;
    }
    
    Vector3 AvoidObstacles()
    {
        Vector3 avoidanceForce = Vector3.zero;
        
        Vector3 ahead = transform.position + velocity.normalized * avoidanceAhead;
        Vector3 ahead2 = transform.position + velocity.normalized * avoidanceAhead * 0.5f;
        
        Collider[] obstacles = Physics.OverlapSpheres(
            transform.position, 
            ahead, 
            ahead2, 
            avoidanceRadius, 
            obstacleLayer
        );
        
        foreach (Collider obstacle in obstacles)
        {
            Vector3 obstaclePos = obstacle.transform.position;
            float distance = Vector3.Distance(transform.position, obstaclePos);
            
            if (distance < avoidanceRadius + avoidanceAhead)
            {
                Vector3 away = transform.position - obstaclePos;
                away = away.normalized / distance;
                avoidanceForce += away;
            }
        }
        
        return avoidanceForce;
    }
    
    Collider[] OverlapSpheres(Vector3 pos1, Vector3 pos2, float radius, LayerMask layer)
    {
        List<Collider> results = new List<Collider>();
        
        Collider[] hits1 = Physics.OverlapSphere(pos1, radius, layer);
        Collider[] hits2 = Physics.OverlapSphere(pos2, radius, layer);
        
        results.AddRange(hits1);
        results.AddRange(hits2);
        
        return results.ToArray();
    }
    
    public void ApplyForce(Vector3 force)
    {
        velocity += force;
    }
    
    public Vector3 GetVelocity()
    {
        return velocity;
    }
    
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
    }
}

public class AICrowd : MonoBehaviour
{
    [Header("Crowd Settings")]
    public int agentCount = 20;
    public float spawnRadius = 50f;
    public GameObject agentPrefab;
    
    [Header("Behavior")]
    public float separationRadius = 3f;
    public float alignmentRadius = 10f;
    public float cohesionRadius = 10f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    
    private List<AISteering> agents = new List<AISteering>();
    
    void Start()
    {
        SpawnAgents();
    }
    
    void SpawnAgents()
    {
        for (int i = 0; i < agentCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = 0;
            
            GameObject agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);
            AISteering steering = agent.AddComponent<AISteering>();
            agents.Add(steering);
        }
    }
    
    void Update()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            AISteering agent = agents[i];
            if (agent == null) continue;
            
            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;
            int separationCount = 0;
            int alignmentCount = 0;
            int cohesionCount = 0;
            
            for (int j = 0; j < agents.Count; j++)
            {
                if (i == j) continue;
                AISteering other = agents[j];
                if (other == null) continue;
                
                float distance = Vector3.Distance(agent.transform.position, other.transform.position);
                
                if (distance < separationRadius)
                {
                    separation += agent.transform.position - other.transform.position;
                    separationCount++;
                }
                
                if (distance < alignmentRadius)
                {
                    alignment += other.GetVelocity();
                    alignmentCount++;
                }
                
                if (distance < cohesionRadius)
                {
                    cohesion += other.transform.position;
                    cohesionCount++;
                }
            }
            
            if (separationCount > 0)
            {
                separation /= separationCount;
                separation = separation.normalized * agent.maxSpeed;
                separation -= agent.GetVelocity();
                separation = Vector3.ClampMagnitude(separation, agent.maxForce);
            }
            
            if (alignmentCount > 0)
            {
                alignment /= alignmentCount;
                alignment = alignment.normalized * agent.maxSpeed;
                alignment -= agent.GetVelocity();
                alignment = Vector3.ClampMagnitude(alignment, agent.maxForce);
            }
            
            if (cohesionCount > 0)
            {
                cohesion /= cohesionCount;
                cohesion -= agent.transform.position;
                cohesion = cohesion.normalized * agent.maxSpeed;
                cohesion -= agent.GetVelocity();
                cohesion = Vector3.ClampMagnitude(cohesion, agent.maxForce);
            }
            
            Vector3 totalForce = 
                separation * separationWeight +
                alignment * alignmentWeight +
                cohesion * cohesionWeight;
            
            agent.ApplyForce(totalForce);
        }
    }
}

public class AINavigation : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public float waypointReachedDistance = 1f;
    
    [Header("Patrol Settings")]
    public bool patrol = true;
    public float waitTimeAtWaypoint = 2f;
    
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    
    void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }
    
    void Update()
    {
        if (!patrol || waypoints == null || waypoints.Length == 0)
            return;
        
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
            return;
        }
        
        if (!agent.pathPending && agent.remainingDistance <= waypointReachedDistance)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }
    
    void MoveToNextWaypoint()
    {
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }
    
    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypoint = 0;
        
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }
    
    public void StopPatrol()
    {
        patrol = false;
        agent.ResetPath();
    }
    
    public void ResumePatrol()
    {
        patrol = true;
        MoveToNextWaypoint();
    }
    
    public void GoToTarget(Vector3 target)
    {
        patrol = false;
        agent.SetDestination(target);
    }
}
