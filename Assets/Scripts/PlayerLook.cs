using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0.0f;

    public float xSensitivity = 30.0f;
    public float ySensitivity = 30.0f;

    void Start()
    {
        LockCursor();

        float sens = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        xSensitivity = sens;
        ySensitivity = sens;
    }

    void Update()
    {
        //
        //LockCursor();
    }

    public void SetSensitivity(float value)
    {
        xSensitivity = value;
        ySensitivity = value;
    }

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        // Vertical rotation (look up/down)
        xRotation -= mouseY * ySensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -80.0f, 80.0f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation (look left/right)
        transform.Rotate(Vector3.up * mouseX * xSensitivity * Time.deltaTime);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}