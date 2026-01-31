using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;
    private PlayerMotor motor;
    private PlayerLook look;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();

        onFoot.Jump.performed += ctx => motor.Jump();

        // HOLD crouch
        onFoot.Crouch.started += ctx => motor.SetCrouch(true);
        onFoot.Crouch.canceled += ctx => motor.SetCrouch(false);

        // HOLD sprint
        onFoot.Sprint.started += ctx => motor.SetSprint(true);
        onFoot.Sprint.canceled += ctx => motor.SetSprint(false);
    }


    // Update is called once per frame
    void Update()
    {
        // Tell the playermotor to move using the value from our movement action
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());

    }

    private void LateUpdate()
    {
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        playerInput.Enable();
        //onFoot.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
        //onFoot.Disable();
    }

}
