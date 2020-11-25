using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private GameObject player;
    private PlayerController playerComponent;
    private Health health; 
    public GameObject healthBar;
    public Text deathsValue;
    public Text killsValue;
    public GameObject PostprocessingVolume;
    public float maxVignetting = 0.5f;
    public Vignette vignette;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        health = player.GetComponent<Health>();
        playerComponent = player.GetComponent<PlayerController>();

//        var postProcess = PostprocessingVolume.GetComponent<Volume>();
//        vignette = (Vignette)postProcess.profile.components.Single(v => v is Vignette);
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.transform.localScale = new Vector3(health.CurrentHealth / (float)health.MaxHealth, 1, 1);
        deathsValue.text = playerComponent.Deaths.ToString();
        killsValue.text = playerComponent.Kills.ToString();

//        vignette.color.value= Color.red;
//        var ratio = 1 - health.CurrentHealth / health.MaxHealth;
//        vignette.intensity.value = Mathf.Lerp(0, maxVignetting, ratio);
    }
}
