using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Health : MonoBehaviour
{
    [HideInInspector]
    public float CurrentHealth;

    public float MaxHealth = 100;
    public float RecoverySpeed = 0;
    public float RecoveryTimeout = 5;
    public bool DestroyAfterZeroHealth = true;

    public GameObject explosion;

    public event NotifyDeletagate NotifiyOnZeroHealth;
    public event NotifyDeletagate NotifiyHit;
    
    private PlayerController playerController;

    private float lastDamage = 0;

    public delegate void NotifyDeletagate(GameObject obj);
    void Start()
    {
        CurrentHealth = MaxHealth;
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        float recovery = 0;

        if (Time.time - lastDamage > RecoveryTimeout)
            recovery = RecoverySpeed * Time.deltaTime;
            
        CurrentHealth = Mathf.Clamp(CurrentHealth + recovery, 0, MaxHealth);
    }

    public void ReceiveDamage(int amount, IKiller killer = null)
    {
        lastDamage = Time.time;

        CurrentHealth -= amount;
        NotifiyHit?.Invoke(gameObject);

        if (CurrentHealth <= 0)
        {
            if (explosion != null)
            {
                var obj = Instantiate(explosion, transform.position, transform.rotation);
                Destroy(obj, 2);
            }

            NotifiyOnZeroHealth?.Invoke(gameObject);

            if(DestroyAfterZeroHealth)
                Destroy(gameObject);
            
            killer?.NotifyKill(gameObject);
            playerController?.NotifyDeath(this, killer);
        }

    }

    public void Revive()
    {
        CurrentHealth = MaxHealth;
    }
}