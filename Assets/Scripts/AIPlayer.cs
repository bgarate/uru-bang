using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class AIPlayer : MonoBehaviour
{
    public float MaxLosDistance = 100;
    public float Fov = 60;
    public float FireRate = 5f;
    public float MaxWalkingDistance = 50;
    public int ShotDamage = 15;
    public float MaxSpread = 10;
    public float MinSpread = 5;
    public float TimeMaxSpread = 1;
    
    private WeaponAnimator weaponAnimator;
    private bool isShooting;
    private Quaternion targetRotation;
    private Health health;
    private List<GameObject> respawnPositions;
    private Light statusLight;
    private float spread;
    private float lastShot;
    private GameObject player;
    private NavMeshAgent agent;
    private Animator animator;
    private State currentState = State.Patrolling;
    private Vector3 nextTarget;
    private Vector2 lastAnimatorParameters;
    private GameObject eye;

    private enum State
    {
        TrackingPlayer,
        Searching,
        Patrolling
    }
    private float playerLastSeen = float.MinValue;
    void Start()
    {
        player = GameObject.Find("Player");

        agent = GetComponent<NavMeshAgent>();
        nextTarget = transform.position;
        eye = transform.Find("Eye").gameObject;
        statusLight = GetComponentInChildren<Light>();
        animator = GetComponent<Animator>();
        targetRotation = transform.rotation;
        weaponAnimator = GetComponentInChildren<WeaponAnimator>();

        health = GetComponent<Health>();
        health.NotifiyOnZeroHealth += NotifyDeath;
        respawnPositions = GameObject.FindGameObjectsWithTag("Respawn").ToList();
        Respawn();
    }

    Vector3 RandomTarget()
    {
        Vector3 randomPosition = transform.position + Random.insideUnitSphere * MaxWalkingDistance;

        if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, MaxWalkingDistance, 1))
            return RandomTarget();

        return hit.position;
        
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

    void Update()
    {
      

        if (PlayerInLineOfSight())
        {
            currentState = State.TrackingPlayer;
            playerLastSeen = Time.time;
        }
        else if(Time.time - playerLastSeen < 5)
        {
            currentState = State.Searching;
        }
        else
        {
            currentState = State.Patrolling;
        }

        isShooting = false;
        weaponAnimator.IsFiring = false;
        
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

        CalculateSpread(isShooting);

        Vector3 speed = Vector3.zero;


        if (agent.remainingDistance > agent.stoppingDistance)
        {
            speed = agent.desiredVelocity;
            speed = transform.InverseTransformDirection(speed);
        }

        Vector2 desiredParameters = new Vector2(speed.x, speed.z);
        lastAnimatorParameters = Vector2.Lerp(lastAnimatorParameters, desiredParameters, Time.deltaTime);

        animator.SetFloat("X", lastAnimatorParameters.x);
        animator.SetFloat("Y", lastAnimatorParameters.y);

        if (!agent.updateRotation)
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
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

    private void WalkToPlayer()
    {
        Vector3 playerToMech = transform.position - player.transform.position;

        if(playerToMech.magnitude < 10)
            return;

        agent.updateRotation = false;

        playerToMech.Normalize();
        playerToMech *= 10;
        agent.SetDestination(player.transform.position + playerToMech);
        
    }

    private void TrackPlayer()
    {
        agent.updateRotation = false;

        Vector3 eyeToPlayer = player.transform.position - eye.transform.position;
        targetRotation = Quaternion.LookRotation(eyeToPlayer);
    }

    private void SearchPlayer()
    {
        agent.updateRotation = false;
        transform.Rotate(transform.up, agent.angularSpeed * Time.deltaTime);
    }

    private void CalculateSpread(bool fire)
    {
        float spreadTime = Time.deltaTime / TimeMaxSpread;

        if (!fire)
            spread -= MaxSpread * spreadTime;
        else
            spread += MaxSpread * spreadTime;

        spread = Mathf.Clamp(spread, MinSpread, MaxSpread);
    }
    private void Shoot()
    {
        Vector3 eyeToPlayer = player.transform.position - eye.transform.position;
        float angle = Vector3.Angle(eye.transform.forward, eyeToPlayer);

        if (angle > 15)
        {
            isShooting = false;
            weaponAnimator.IsFiring = false;
            return;
        }

        isShooting = true;
        weaponAnimator.IsFiring = true;

        if (Time.time - lastShot < 1 / FireRate) 
            return;


        Vector3 raycastDirection = eye.transform.forward;
        Vector3 randomRotation = (Random.rotation.eulerAngles / 360 - new Vector3(0.5f, 0.5f, 0.5f)) * spread;
        raycastDirection = Quaternion.Euler(randomRotation) * raycastDirection;

        bool collision = Physics.Raycast(eye.transform.position, raycastDirection, out RaycastHit hitInfo, 100);

        if (!collision)
            return;

        weaponAnimator.InstantiateShot(hitInfo);

        Health healthObject = hitInfo.transform.gameObject.GetComponent<Health>();

        if (healthObject != null)
        {
            healthObject.ReceiveDamage(ShotDamage);
        }


        lastShot = Time.time;
        
    }

    public void NotifyDeath(GameObject obj)
    {
        health.Revive();

        Respawn();
    }
    void Respawn()
    {
        int i = Random.Range(0, respawnPositions.Count);
        var destinationTransform = respawnPositions[i].transform;
        agent.Warp(destinationTransform.position);
    }
}
