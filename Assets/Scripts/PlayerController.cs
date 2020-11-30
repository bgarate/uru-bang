using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, IKiller
{
    public float MovementSpeed = 10;
    public float Gravity = -9.8f;
    public LayerMask GroundMask;
    public float JumpSpeed = 5;
    public float GroundDistance = 0.1f;

    public int Kills = 0;
    public int Deaths = -1;

    private CharacterController characterController;
    private Animator animator;
    private List<GameObject> respawnPositions;
    private Health health;

    private bool isGrounded;
    private float speedY;

    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        respawnPositions = GameObject.FindGameObjectsWithTag("Respawn").ToList();

        Respawn();
        health = GetComponent<Health>();
        health.NotifiyHit += OnHit;
    }

    private IEnumerator Shake(float amplitude, float duration)
    {
        Camera camera = Camera.main;

        float timeStarted = Time.time;
        Vector3 originalPosition = camera.transform.localPosition;

        while (Time.time - timeStarted < duration)
        {
            Vector3 random = new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude),
                Random.Range(-amplitude, amplitude));
            Vector3 newPosition = originalPosition + random;

            camera.transform.localPosition = newPosition;

            yield return null;
        }

    }

    private void OnHit(GameObject obj)
    {
        StartCoroutine(Shake(0.1f, 0.3f));
    }

    void Update()
    {
        isGrounded = Physics.SphereCast(transform.position, characterController.radius, Vector3.down,
            out RaycastHit hitInfo, characterController.height / 2 - characterController.radius + GroundDistance,
            GroundMask, QueryTriggerInteraction.Ignore);

        speedY += Gravity * Time.deltaTime;

        if (isGrounded && speedY < 0)
        {
            speedY = -1f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
            speedY = JumpSpeed;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        var forward = transform.forward * vertical;
        var right = transform.right * horizontal;

        var movement = new Vector3();

        movement += (forward + right) * MovementSpeed * Time.deltaTime;

        movement.y += speedY * Time.deltaTime;
        characterController.Move(movement);

        animator.SetFloat("X", horizontal);
        animator.SetFloat("Y", vertical);

    }

    void Respawn()
    {
        Deaths++;

        int i = Random.Range(0, respawnPositions.Count);
        var destinationTransform = respawnPositions[i].transform;

        characterController.enabled = false;
        transform.position = destinationTransform.position;
        transform.rotation = destinationTransform.rotation;
        characterController.enabled = true;
    }

    void Kill()
    {
        Kills++;
    }

    public void NotifyKill(GameObject gameObject)
    {
        Kill();
    }

    public void NotifyDeath(Health health, IKiller killer)
    {
        health.Revive();
        Respawn();
    }
}
