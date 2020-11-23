using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{

    public PlayerController Player;
    public Camera camera;

    private double lastFire;
    private float spread;
    public float maxSpread = 30f;
    public float timeMaxSpread = 1;
    public float shotsPerSecond = 5;
    public Image crosshair;
    public int shotDamage = 15;
    
    public WeaponAnimator animator;

    // Start is called before the first frame update
    void Start()
    {
        
        lastFire = Time.time;
        Player = GameObject.Find("Player").GetComponent<PlayerController>();
        animator = GetComponent<WeaponAnimator>();

    }

    // Update is called once per frame
    void Update()
    {

     
        bool fire = Input.GetButton("Fire1");
        CalculateSpread(fire);

        crosshair.rectTransform.localScale = new Vector3(1, 1, 1) * (spread / maxSpread) * 5 + new Vector3(1, 1, 1);

        animator.IsFiring = fire;
        animator.shotsPerSecond = shotsPerSecond;

        if (!fire)
            return;

        if (Time.time - lastFire < 1 / shotsPerSecond)
            return;

        Vector3 raycastDirection = camera.transform.forward;
        Vector3 randomRotation = (Random.rotation.eulerAngles / 360 - new Vector3(0.5f, 0.5f, 0.5f)) * spread;
        raycastDirection = Quaternion.Euler(randomRotation) * raycastDirection;
        
        bool collision = Physics.Raycast(camera.transform.position, raycastDirection, out RaycastHit hitInfo, 100);

        if (!collision)
            return;

        animator.InstantiateShot(hitInfo);

        Health healthObject = hitInfo.transform.gameObject.GetComponent<Health>();

        if (healthObject != null)
        {
            healthObject.ReceiveDamage(shotDamage, Player);
        }

        lastFire = Time.time;

    }

    private void CalculateSpread(bool fire)
    {
        float spreadTime = Time.deltaTime / timeMaxSpread;

        if (!fire)
            spread -= maxSpread * spreadTime;
        else
            spread += maxSpread * spreadTime;

        spread = Mathf.Clamp(spread, 0, maxSpread);
    }
}