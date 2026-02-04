using UnityEngine;
using System.Collections;

public class PlayerMotor : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Transform playerCamera;
    [SerializeField] float standingCameraHeight = 1.6f;
    [SerializeField] float crouchCameraHeight = 1.0f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    public float speed = 5.0f;
    private bool isGrounded;

    [Header("Jump")]
    public float jumpHeight;
    public float gravity;


    bool crouching = false;
    float crouchTimer = 1.0f;
    bool lerpCrouch = false;
    bool sprinting = false;

    [Header("Movement")]
    public float maxSpeed;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 40f;

    public float groundedAccelMultiplier = 1.0f;
    public float airAccelMultiplier = 0.6f;

    private Vector3 horizontalVelocity;

    [SerializeField] float standingHeight = 2f;
    [SerializeField] float crouchHeight = 1f;

    [Header("Tag Stun")]
    [SerializeField] float tagStunDuration = 1f;

    private bool isStunned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        maxSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;

            float duration = 0.15f;
            float p = Mathf.Clamp01(crouchTimer / duration);
            p = p * p;

            float targetHeight = crouching ? crouchHeight : standingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, p);

            controller.center = new Vector3(
                0f,
                controller.height / 2f,
                0f
            );

            // CAMERA
            float camTargetY = crouching ? crouchCameraHeight : standingCameraHeight;
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, camTargetY, p);
            playerCamera.localPosition = camPos;

            if (p >= 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

    public void ProcessMove(Vector2 input)
    {
        if (isStunned)
            return;

        // Input direction (world space)
        Vector3 inputDir = new Vector3(input.x, 0f, input.y);
        inputDir = transform.TransformDirection(inputDir);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        // Target horizontal velocity
        Vector3 targetVelocity = inputDir * maxSpeed;

        // Acceleration selection
        float accelRate;
        if (isGrounded)
            accelRate = inputDir.magnitude > 0.1f ? acceleration : deceleration;
        else
            accelRate = inputDir.magnitude > 0.1f ? acceleration * airAccelMultiplier : deceleration * 0.1f;

        // Accelerate toward target
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            accelRate * Time.deltaTime
        );

        // HARD SPEED CAP
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeed);

        if (isGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;

        playerVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMove =
            horizontalVelocity +
            Vector3.up * playerVelocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }

    public void Jump()
    {
        if (isStunned)
            return;

        if (controller.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
        }
    }

    public void SetSprint(bool value)
    {
        if (crouching) value = false;

        sprinting = value;
        maxSpeed = sprinting ? runSpeed : walkSpeed;

        UnityEngine.Debug.Log($"Setting sprint to {value.ToString()} and the max speed just got set to {maxSpeed.ToString()}");
    }

    public void SetCrouch(bool value)
    {
        UnityEngine.Debug.Log($"Setting crouch to {value.ToString()}");
        crouching = value;
        crouchTimer = 0f;
        lerpCrouch = true;
    }

    public IEnumerator StunPlayer()
    {
        isStunned = true;

        // Stop existing motion immediately
        horizontalVelocity = Vector3.zero;
        playerVelocity = Vector3.zero;

        yield return new WaitForSeconds(tagStunDuration);

        isStunned = false;
    }
}
