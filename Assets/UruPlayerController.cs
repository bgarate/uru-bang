﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UruPlayerController : MonoBehaviour
{
    public float MovementSpeed = 10;
    public float Gravity = -9.8f;
    public LayerMask GroundMask;
    public float JumpSpeed = 5;
    public float GroundDistance = 0.1f;

    private CharacterController characterController;
    private Animator animator;

    private bool isGrounded;
    private float speedY;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
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
}