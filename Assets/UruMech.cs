using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class UruMech : MonoBehaviour
{

    public GameObject Missile;
    public LayerMask GroundMask;

    public float HipRotationSpeed = 30;
    public float MaxLosDistance = 100;
    public float Fov = 60;
    public float FireRate = 0.5f;
    public float MaxPatrollingTargetDistance = 50;
    public float Gravity = -9.8f;

    private GameObject eye;
    private GameObject hip;
    private CharacterController controller;

    private Quaternion hipRotation;
    private Quaternion targetHipRotation;
    private UnityEngine.AI.NavMeshAgent agent;
    private GameObject cannonA;
    private GameObject cannonB;

    private Animator animator;
    private Light statusLight;
    private GameObject player;
    private bool fireCannonANext;
    private float lastShot;
    private Vector3 nextTarget;
    private bool rotatingHipRight;
    private State currentState = State.Patrolling;
    private bool isGrounded;
    private float groundDistance = 0.1f;
    private float speedY;
    private enum State
    {
        TrackingPlayer,
        Searching,
        Patrolling
    }
    private float playerLastSeen = float.MinValue;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        controller = GetComponent<CharacterController>();
        hip = transform.Find("mech/Mech/Root/Pelvis/Hip").gameObject;
        eye = hip.transform.Find("Body/Eye").gameObject;
        cannonA = hip.transform.Find("Body/CannonA").gameObject;
        cannonB = hip.transform.Find("Body/CannonB").gameObject;
        hipRotation = hip.transform.rotation;
        targetHipRotation = hip.transform.rotation;
        agent = GetComponent<NavMeshAgent>();
        nextTarget = transform.position;
        statusLight = GetComponentInChildren<Light>();
        agent.updatePosition = false;
        animator = GetComponentInChildren<Animator>();
    }

    bool PlayerInLineOfSight()
    {
        Vector3 eyeToPlayer = player.transform.position - eye.transform.position;
        float angle = Vector3.Angle(eye.transform.forward, eyeToPlayer);

        if (angle > Fov || eyeToPlayer.magnitude > MaxLosDistance)
            return false;

        Ray ray = new Ray(eye.transform.position, eyeToPlayer);
        if (!Physics.Raycast(ray, out RaycastHit hit, MaxLosDistance))
            return false;

        return hit.transform.gameObject == player;

    }

    private void ResetHip()
    {
        var localHip = Quaternion.Euler(0, 0, 0);
        targetHipRotation = hip.transform.parent.rotation * localHip;
    }

    private void TrackPlayer()
    {
        Vector3 eyeToPlayer = hip.transform.position - player.transform.position;
        targetHipRotation = Quaternion.LookRotation(eyeToPlayer, hip.transform.up);

    }

    private void SearchPlayer()
    {

        var localHip = Quaternion.Euler(rotatingHipRight ? 90 : -90, 0, 0);
        targetHipRotation = hip.transform.parent.rotation * localHip;

        if (Quaternion.Angle(targetHipRotation, hip.transform.rotation) < 5)
            rotatingHipRight = !rotatingHipRight;
    }
    private void Shoot()
    {
        Vector3 eyeToPlayer = player.transform.position - eye.transform.position;
        float angle = Vector3.Angle(eye.transform.forward, eyeToPlayer);

        if (angle > 15)
            return;

        if (Time.time - lastShot < 1 / FireRate)
            return;

        Vector3 position = fireCannonANext ? cannonA.transform.position : cannonB.transform.position;
        Vector3 toPlayer = player.transform.position - position;
        Instantiate(Missile, position, Quaternion.LookRotation(toPlayer));

        lastShot = Time.time;
        fireCannonANext = !fireCannonANext;

    }

    private void LateUpdate()
    {
        hip.transform.rotation = hipRotation;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.SphereCast(transform.position, controller.radius, Vector3.down,
            out RaycastHit hitInfo, controller.height / 2 - controller.radius + groundDistance,
            GroundMask, QueryTriggerInteraction.Ignore);

        speedY += Gravity * Time.deltaTime;

        if (isGrounded && speedY < 0)
        {
            speedY = -1f;
        }

        if (PlayerInLineOfSight())
        {
            currentState = State.TrackingPlayer;
            playerLastSeen = Time.time;
        }
        else if (Time.time - playerLastSeen < 5)
        {
            currentState = State.Searching;
        }
        else
        {
            ResetHip();
            currentState = State.Patrolling;
        }

        switch (currentState)
        {
            case State.TrackingPlayer:
                statusLight.color = Color.red;
                TrackPlayer();
                WalkToPlayer();
                Shoot();
                break;
            case State.Searching:
                statusLight.color = Color.yellow;
                SearchPlayer();
                break;
            case State.Patrolling:
                statusLight.color = Color.green;
                WalkToTarget();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        RotateHip();

        Vector3 speed = Vector3.zero;

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            speed = agent.desiredVelocity * Time.deltaTime;
        }

        speed.y += speedY * Time.deltaTime;

        controller.Move(speed);
        agent.nextPosition = controller.transform.position;
        var speed2d = new Vector3(speed.x, 0, speed.z);
        animator.SetFloat("Speed", speed2d.magnitude * 10);
    }
    private void WalkToTarget()
    {
        if (agent.remainingDistance < 2 || agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            agent.updateRotation = true;
            nextTarget = RandomTarget();
            agent.SetDestination(nextTarget);
        }

    }
    private void RotateHip()
    {
        hipRotation = Quaternion.RotateTowards(hip.transform.rotation, targetHipRotation, Time.deltaTime * HipRotationSpeed);

    }
    Vector3 RandomTarget()
    {
        Vector3 randomPosition = transform.position + Random.insideUnitSphere * MaxPatrollingTargetDistance;

        if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, MaxPatrollingTargetDistance, 1))
            return RandomTarget();

        return hit.position;

    }


    private void WalkToPlayer()
    {
        Vector3 playerToMech = transform.position - player.transform.position;

        if (playerToMech.magnitude < 10)
            return;

        agent.updateRotation = false;
        playerToMech.Normalize();
        playerToMech *= 10;
        agent.SetDestination(player.transform.position + playerToMech);
    }

}
