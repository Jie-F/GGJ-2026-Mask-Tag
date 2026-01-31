using System;
using System.Threading;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float speed = 5.0f;
    private bool isGrounded;
    public float gravity = -9.8f;
    public float jumpHeight = 1.5f;
    bool crouching = false;
    float crouchTimer = 1.0f;
    bool lerpCrouch = false;
    bool sprinting = false;

    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 12f;
    public float deceleration = 16f;

    private Vector3 horizontalVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = (float) (crouchTimer / 1.0);
            p *= p;
            if (crouching)
            {
                controller.height = Mathf.Lerp(controller.height, 1, p);
            }
            else
            {
                controller.height = Mathf.Lerp(controller.height, 2, p);
            }

            if (p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0.0f;
            }
        }
    }

    public void ProcessMove(Vector2 input)
    {
        // Input -> world direction
        Vector3 inputDir = new Vector3(input.x, 0f, input.y);
        inputDir = transform.TransformDirection(inputDir);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        // Target velocity
        Vector3 targetVelocity = inputDir * maxSpeed;

        // Choose accel or decel
        //float accelRate = inputDir.magnitude > 0.1f ? acceleration : deceleration;
        float airControlMultiplier = isGrounded ? 1f : 0.4f;
        float accelRate = (inputDir.magnitude > 0.1f ? acceleration : deceleration) * airControlMultiplier;

        // Smooth acceleration / sliding
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            accelRate * Time.deltaTime
        );

        // Gravity
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        playerVelocity.y += gravity * Time.deltaTime;

        // Combine horizontal + vertical
        Vector3 finalMove = horizontalVelocity + Vector3.up * playerVelocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }


    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    public void Crouch()
    {
        crouching = !crouching;
        crouchTimer = 0.0f;
        lerpCrouch = true;
    }

    public void Sprint()
    {
        sprinting = !sprinting;
        maxSpeed = sprinting ? 8f : 5f;
    }

}
