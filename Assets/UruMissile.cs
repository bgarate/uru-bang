using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UruMissile : MonoBehaviour
{
    public GameObject Explosion;
    public float Speed = 50;
    public float ExplosionRadius = 2.3f;
    public float MaxDamage = 80;
    public float MinDamage = 10;

    private bool exploded = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + transform.forward * Speed * Time.deltaTime;
    }


    void OnCollisionEnter()
    {
        if (exploded)
            return;

        exploded = true;
        GameObject instantiatedExplosion = Instantiate(Explosion, transform.position, transform.rotation);

        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius);

        foreach (Collider hit in hits)
        {
            Health healthObject = hit.transform.gameObject.GetComponent<Health>();
            if (healthObject == null)
                continue;

            var distanceToTarget = (hit.transform.position - transform.position).magnitude;

            if (distanceToTarget > ExplosionRadius)
                continue;

            var sqrAjustedDistance = Mathf.Pow(1 + distanceToTarget, 2);
            var multiplier = 1 / sqrAjustedDistance;
            var damageAmount = (MaxDamage - MinDamage) * multiplier + MinDamage;

            healthObject.ReceiveDamage((int)damageAmount);

        }

        Destroy(instantiatedExplosion, 1.5f);
        Destroy(gameObject);
    }
}
