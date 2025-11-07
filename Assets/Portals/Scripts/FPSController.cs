using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Script")]
    [SerializeField] private InputHandler _inputHandler;    

    [Header("Movement")]
    private int _moveSpeed = 5;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    private float _xRotation = 0f;
    private float _mouseSensitivity = 25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Look();
    }

    void Move()
    {
        Vector2 move = _inputHandler.Move;
        Vector3 moveDirection = transform.forward * 
            move.y + transform.right * move.x;
        transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        Vector2 look = _inputHandler.Look;
        Rigidbody rb = GetComponent<Rigidbody>();

        if (look == Vector2.zero)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationY; //bloquer le freeze en y
            return;
        }
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationY; //debloquer le freeze en y

        transform.Rotate(Vector3.up * look.x * (_mouseSensitivity * Time.deltaTime));

        _xRotation -= look.y * (_mouseSensitivity * Time.deltaTime);
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _mainCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
}
 