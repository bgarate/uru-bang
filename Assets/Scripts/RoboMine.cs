using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoboMine : MonoBehaviour
{
    // The explosion to instance when health reaches 0
    public GameObject explosion;
    
    // How far does it produces damage?
    public float explosionRadius = 4f;
    // Maximum allowed damage
    public float maxDamage = 100;
    // Base damage for everyone on radius
    public float minDamage = 50;
    // How far from target it explodes
    public float proximityRange = 2f;
    // Distance of activation
    public float searchDistance = 10;

    // Reference to player
    private GameObject player;
    // Reference to mine's NavMeshAgent
    private NavMeshAgent agent;
    // Reference to mine's audio source
    private AudioSource targetingAudio;
    // Reference to mine's light
    private Light internalLight;

    // Is already being destroyed?
    private bool destroying;
    // Has been activated and is pursuing the player?
    private bool isTargeting;

    void Start()
    {
        // Initialize references
        player = GameObject.Find("Player");
        agent = GetComponent<NavMeshAgent>();
        targetingAudio = GetComponent<AudioSource>();
        internalLight = GetComponentInChildren<Light>();
        
        // If reaches 0 health, explode!
        GetComponent<Health>().NotifiyOnZeroHealth += obj => SelfDestroy();
        // If player shoot at mine, start targeting!
        GetComponent<Health>().NotifiyHit += obj => StartTargeting();
    }

    void Update()
    {
        // Distance to the player
        var distance = (player.transform.position - transform.position).magnitude;
        
        // Is player too close?
        if (distance < searchDistance && !isTargeting)
        {
            StartTargeting();
        }
        
        // Let the agent calculate the path and move the mine
        if(isTargeting) 
            agent.SetDestination(player.transform.position);


        // If in radius, explode!
        if ((player.transform.position - transform.position).magnitude < proximityRange)
        {
            SelfDestroy();
        }
    }

    private void StartTargeting()
    {
        // If already targeting, do nothing
        if(isTargeting)
            return;

        // Obvious, isn't it?
        isTargeting = true;

        // Play an audio clip to let the player know whe are following him
        targetingAudio.Play();
        // And show a red light
        internalLight.color = Color.red;
    }

    private void SelfDestroy()
    {
        // If already destroying, do nothing
        if(destroying)
            return;

        // Destroy!
        destroying = true;
        
        // Create an exlplosion
        var explosionGameObject = Instantiate(explosion, transform.position, Quaternion.identity);
        // Make it last 2 seconds
        Destroy(explosionGameObject, 2);
        // Destroy the mine
        Destroy(gameObject);
        
        // Find colliders inside the explosion sphere
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            // Can take any damage?
            Health healthObject = hit.transform.gameObject.GetComponent<Health>();
            if (healthObject == null)
                continue;
            
            var distanceToTarget = (hit.transform.position - transform.position).magnitude;

            // Inside explosion sphere?
            if (distanceToTarget > explosionRadius)
                continue;

            // Apply square law to apply damage
            var sqrAjustedDistance = Mathf.Pow(1 + distanceToTarget, 2);
            var multiplier = 1 / sqrAjustedDistance;
            var damageAmount = (maxDamage - minDamage) * multiplier + minDamage;

            // Apply damage
            healthObject.ReceiveDamage((int)damageAmount);
            
        }

    }
}
