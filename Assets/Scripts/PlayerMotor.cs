using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float speed = 5.0f;
    private bool isGrounded;
    public float gravity = -10f;
    public float jumpHeight = 0.5f;
    bool crouching = false;
    float crouchTimer = 1.0f;
    bool lerpCrouch = false;
    bool sprinting = false;

    [Header("Movement")]
    public float maxSpeed;
    public float walkSpeed = 5.0f;
    public float runSpeed = 8.0f;
    public float acceleration = 25f;
    public float deceleration = 40f;

    public float groundedAccelMultiplier = 1.0f;
    public float airAccelMultiplier = 0.6f;

    private Vector3 horizontalVelocity;

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
            float p = (float) (crouchTimer / 1.0);
            p *= p;
            float targetHeight = crouching ? 1f : 2f;
            controller.height = Mathf.Lerp(
                crouching ? 2f : 1f,
                crouching ? 1f : 2f,
                p
            );

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
        isGrounded = controller.isGrounded;
        float airControlMultiplier = isGrounded ? groundedAccelMultiplier : airAccelMultiplier;
        float accelRate;
        if (isGrounded)
            accelRate = inputDir.magnitude > 0.1f ? acceleration : deceleration;
        else
            accelRate = inputDir.magnitude > 0.1f ? acceleration * airAccelMultiplier : deceleration * 0.1f;


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
            playerVelocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
        }
    }

    public void SetSprint(bool value)
    {
        if (crouching) value = false;

        sprinting = value;
        maxSpeed = sprinting ? runSpeed : walkSpeed;
    }


    public void SetCrouch(bool value)
    {
        crouching = value;
        crouchTimer = 0f;
        lerpCrouch = true;
    }


}
