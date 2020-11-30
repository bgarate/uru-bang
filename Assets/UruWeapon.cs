using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UruWeapon : MonoBehaviour
{
    private UruPlayerController Player;
    public Camera camera;

    private double lastFire;
    private float spread;
    public float maxSpread = 30f;
    public float timeMaxSpread = 1;
    public float shotsPerSecond = 5;
    public Image crosshair;
    public int shotDamage = 15;

    private UruWeaponAnimator animator;


    void Start()
    {
        lastFire = Double.MinValue;
        Player = GameObject.Find("Player").GetComponent<UruPlayerController>();
        animator = GetComponent<UruWeaponAnimator>();

    }

    void Update()
    {
        bool fire = Input.GetButton("Fire1");
        CalculateSpread(fire);
    }

    private void CalculateSpread(bool fire)
    {
        float spreadTime = Time.deltaTime / timeMaxSpread;

        if (!fire)
            spread -= maxSpread * spreadTime;
        else
            spread += maxSpread * spreadTime;

        spread = Mathf.Clamp(spread, 0, maxSpread);

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
}
