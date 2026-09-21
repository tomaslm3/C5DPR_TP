using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private float _sensitivity = 0.1f;
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    private Transform _player;
    private Transform _eyes;

    private float _pitch;

    public void SetTarget(
        Transform player,
        Transform eyes)
    {
        _player = player;
        _eyes = eyes;

        _pitch = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if(_player == null || _eyes == null)
            return;

        Look();
    }

    private void Look()
    {
        if(Mouse.current == null)
            return;

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        float mouseX =
            mouseDelta.x * _sensitivity;

        float mouseY =
            mouseDelta.y * _sensitivity;

        // Yaw.
        _player.Rotate(
            Vector3.up,
            mouseX
        );

        // Pitch.
        _pitch -= mouseY;

        _pitch = Mathf.Clamp(
            _pitch,
            _minPitch,
            _maxPitch
        );
    }

    private void LateUpdate()
    {
        if(_eyes == null)
            return;

        transform.position = _eyes.position;

        transform.rotation =
            Quaternion.Euler(
                _pitch,
                _player.eulerAngles.y,
                0f
            );
    }
}