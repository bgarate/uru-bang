using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UruWeaponAnimator : MonoBehaviour
{
    public GameObject Shot;

    public Animator animation;

    [HideInInspector]
    public float shotsPerSecond = 5;

    private double lastFire;
    public GameObject MuzzleFlare;
    private GameObject FlareObject;
    private GameObject InstantiatedMuzzleFlare;
    public bool IsFiring;
    public AudioSource shotAudio;

    // Start is called before the first frame update
    void Start()
    {
        animation.enabled = false;
        FlareObject = gameObject.transform.Find("Flare").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsFiring)
        {
            if (InstantiatedMuzzleFlare != null)
                Destroy(InstantiatedMuzzleFlare);
            InstantiatedMuzzleFlare = null;
            return;
        }


        if (InstantiatedMuzzleFlare == null)
            InstantiatedMuzzleFlare = GameObject.Instantiate(MuzzleFlare, FlareObject.transform.position,
                FlareObject.transform.rotation, FlareObject.transform);

        animation.enabled = true;
        animation.Play("FireShoot");
        animation.speed = shotsPerSecond;

        if (Time.time - lastFire < 1 / shotsPerSecond)
            return;

        lastFire = Time.time;
        shotAudio.Play();

    }

    public void InstantiateShot(RaycastHit hitInfo)
    {
        GameObject show = Object.Instantiate(Shot, hitInfo.point + hitInfo.normal * 0.01f,
            Quaternion.LookRotation(-hitInfo.normal), hitInfo.transform);
        GameObject.Destroy(show, 5);

    }
}
