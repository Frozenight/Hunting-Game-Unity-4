using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Deer_controller : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;               //  Nav mesh agent component
    private float startWaitTime = 10;                 //  Wait time of every action
    public float timeToRotate = 2;                  //  Wait time when the enemy detect near the player without seeing
    public float speedWalk = 2;                     //  Walking speed, speed in the nav mesh agent
    public float speedRun = 9;                      //  Running speed

    private float viewRadius = 100;                   //  Radius of the enemy view
    private float canSeeRadius = 25;
    public float viewAngle = 90;                    //  Angle of the enemy view
    public float safeDistnace = 15;
    public LayerMask playerMask;                    //  To detect the player with the raycast
    public LayerMask obstacleMask;                  //  To detect the obstacules with the raycast
    public float meshResolution = 1.0f;             //  How many rays will cast per degree
    public int edgeIterations = 4;                  //  Number of iterations to get a better performance of the mesh filter when the raycast hit an obstacule
    public float edgeDistance = 0.5f;               //  Max distance to calcule the a minumun and a maximum raycast when hits something
    public Vector3 m_PlayerPosition;


    public Transform[] waypoints;                   //  All the waypoints where the enemy patrols
    int m_CurrentWaypointIndex;                     //  Current waypoint where the enemy is going to
    [SerializeField]
    private Transform playerPosition;

    float m_WaitTime;                               //  Variable of the wait time that makes the delay
    bool m_playerInRange = false;
    bool m_IsSafe = true;
    bool m_wasSeen = false;
    bool m_hasRunAwayPoint = false;

    private Animator anim;

    private bool isDead = false;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        m_WaitTime = startWaitTime;                 //  Set the wait time variable that will change

        m_CurrentWaypointIndex = 2;                 //  Set the initial waypoint
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;             //  Set the navemesh speed with the normal speed of the enemy
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);    //  Set the destination to the first waypoint
        Move(speedWalk);
    }

    private void Update()
    {
        if (isDead)
            return;
        EnviromentView();                       //  Check whether or not the player is in the enemy's field of vision
        if (m_playerInRange)
            RunAway();
        else if (!m_IsSafe)
            RunAway();
        else
            Patrolling();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
            Die();
    }

    public void Die()
    {
        if (isDead)
            return;
        isDead = true;
        Vector3 heading = playerPosition.position - transform.position;
        float dir = AngleDir(transform.forward, heading, transform.up);
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
        if (dir == -1)
            anim.SetBool("IsDeadRight", true);
        else if (dir == 1)
            anim.SetBool("IsDeadLeft", true);
        else
            anim.SetBool("IsDeadLeft", true);
    }

    public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
        {
            return 1.0f;
        }
        else if (dir < 0.0f)
        {
            return -1.0f;
        }
        else
        {
            return 0.0f;
        }
    }

    void Stop()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
        anim.SetBool("IsEating", true);
        anim.SetBool("IsWalking", false);
    }

    void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
        anim.SetBool("IsEating", false);
        anim.SetBool("IsWalking", true);
    }

    void Patrolling()
    {
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            //  If the enemy arrives to the waypoint position then wait for a moment and go to the next
            if (m_WaitTime <= 0)
            {
                NextPoint();
                Move(speedWalk);
                m_WaitTime = startWaitTime;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
        m_hasRunAwayPoint = false;
    }

    void NextPoint()
    {
        bool done = false;
        int nextPoint = 0;
        while (!done)
        {
            nextPoint = Random.Range(0, waypoints.Length - 2);
            if (nextPoint != m_CurrentWaypointIndex)
                done = true;
        }
        m_CurrentWaypointIndex = nextPoint;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
    }

    void RunAway()
    {
        if (!m_hasRunAwayPoint)
            RunAwayPoint();
        // Coming into here for the 1st time doesnt have the right SetDestination
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            //  If the enemy arrives to the waypoint position then wait for a moment and go to the next
            if (m_WaitTime <= 0)
            {
                Move(speedRun);
                m_WaitTime = startWaitTime;
                m_hasRunAwayPoint = false;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }

    void RunAwayPoint()
    {
        m_CurrentWaypointIndex = waypoints.Length - 1;
        bool done = false;
        m_WaitTime = startWaitTime;
        int offset = 1;
        int offSet = 20;
        int endCycleCheck = 0;
        NavMeshHit hit;
        while (!done)
        {
            Vector3 targetDir = playerPosition.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);

            Vector3 playerPos = transform.position;
            Vector3 playerDirection = -transform.forward;
            Quaternion playerRotation = transform.rotation;
            Vector3 spawnPos = playerPos + playerDirection * offset * offSet;

            waypoints[m_CurrentWaypointIndex].position = spawnPos;
            if (NavMesh.SamplePosition(waypoints[m_CurrentWaypointIndex].position, out hit, 1f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
                done = true;
            }
            else
            {
                offset += 2;
            }
            endCycleCheck++;
            if (endCycleCheck >= 20)
            {
                navMeshAgent.SetDestination(waypoints[0].position);
                break;
            }
        }

        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        Move(speedRun);
        m_hasRunAwayPoint = true;
    }

    void EnviromentView()
    {
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);   //  Make an overlap sphere around the enemy to detect the playermask in the view radius

        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float dstToPlayer = Vector3.Distance(transform.position, player.position);          //  Distance of the enmy and the player

            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, canSeeRadius, obstacleMask) && (dstToPlayer < canSeeRadius))
                {
                    m_playerInRange = true;             //  The player has been seen by the enemy and then the deer starts running away
                    m_wasSeen = true;
                }
                else
                {
                    //If the player is behind a obstacle the player position will not be registered
                    m_playerInRange = false;
                }
            }
            else
                m_playerInRange = false;
            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                /*
                 *  If the player is further than the view radius, then the enemy will no longer keep the player's current position.
                 *  Or the enemy is a safe zone, the enemy will no chase
                 * */
                m_playerInRange = false;                //  Change the sate of chasing
            }
            if (m_wasSeen)
            {
                if (dstToPlayer >= safeDistnace)
                {
                    m_IsSafe = true;
                    m_wasSeen = false;
                }
                else
                {
                    m_IsSafe = false;
                }
            }
        }
    }
}
