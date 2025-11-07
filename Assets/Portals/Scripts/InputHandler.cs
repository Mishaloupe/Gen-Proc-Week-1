using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InputHandler : MonoBehaviour
{
    private Vector2 _move = new();
    private Vector2 _look = new();

    public Vector2 Move => _move;
    public Vector2 Look => _look;

    public void OnMove(CallbackContext ctx)
    {
        _move = ctx.ReadValue<Vector2>();
    }

    public void OnLook(CallbackContext ctx)
    {
        _look = ctx.ReadValue<Vector2>();
    }
}
