using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoboMine : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject player;
    private NavMeshAgent agent;
    public GameObject explosion;
    
    public float explosionRadius = 4f;
    public float proximityRange = 2f;
    public float maxDamage = 100;
    public float minDamage = 50;
    public float searchDistance = 10;

    private bool destroying = false;
    private bool isTargeting = false;
    private AudioSource targetingAudio;
    private Light internalLight;

    void Start()
    {
        player = GameObject.Find("Player");
        agent = GetComponent<NavMeshAgent>();
        
        targetingAudio = GetComponent<AudioSource>();
        internalLight = GetComponentInChildren<Light>();

        GetComponent<Health>().NotifiyOnZeroHealth += obj => SelfDestroy();
        GetComponent<Health>().NotifiyHit += obj => StartTargeting();
    }

    // Update is called once per frame
    void Update()
    {
        var distance = (player.transform.position - transform.position).magnitude;

        if (distance < searchDistance && !isTargeting)
        {
            StartTargeting();
        }
            
        if(isTargeting) 
            agent.SetDestination(player.transform.position);


        if ((player.transform.position - transform.position).magnitude < proximityRange)
        {
            SelfDestroy();
        }
    }

    private void StartTargeting()
    {
        isTargeting = true;

        targetingAudio.Play();
        internalLight.color = Color.red;
    }

    private void SelfDestroy()
    {
        if(destroying)
            return;

        destroying = true;
        
        var explosionGameObject = Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(explosionGameObject, 2);
        Destroy(gameObject);
        
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            
            Health healthObject = hit.transform.gameObject.GetComponent<Health>();
            if (healthObject == null)
                continue;
            
            var distanceToTarget = (hit.transform.position - transform.position).magnitude;

            if (distanceToTarget > explosionRadius)
                continue;

            var sqrAjustedDistance = Mathf.Pow(1 + distanceToTarget, 2);
            var multiplier = 1 / sqrAjustedDistance;
            var damageAmount = (maxDamage - minDamage) * multiplier + minDamage;

            healthObject.ReceiveDamage((int)damageAmount);
            
        }

    }
}
