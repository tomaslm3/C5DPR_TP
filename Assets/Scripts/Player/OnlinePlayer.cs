using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class OnlinePlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.01f)]
    private float _speed = 4f;

    [Header("View")]
    [SerializeField]
    private PlayerView _playerView;

    [SerializeField]
    private Transform _eyes;

    [Header("Shooting")]
    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private Transform bulletSpawnPoint;

    [Networked, OnChangedRender(nameof(DebugScore))]
    public int Score
    {
        get => default;
        private set { }
    }

    private bool _isShootPressed;
    private Vector2 _moveInput;

    private Rigidbody _rb;

    public override void Spawned()
    {
        Debug.Log(
            $"ONLINE PLAYER | " +
            $"StateAuthority: {Object.StateAuthority} | " +
            $"InputAuthority: {Object.InputAuthority}"
        );

        _rb = GetComponent<Rigidbody>();

        if(HasStateAuthority)
        {
            Score = 0;

            Camera.main
                .GetComponent<CameraMovement>()
                .SetTarget(
                    transform,
                    _eyes
                );
        }

        GameManager.Instance?.AddToList(this);
    }

    private void Update()
    {
        if(!HasStateAuthority)
            return;

        ReadInput();
    }

    private void ReadInput()
    {
        _moveInput = Vector2.zero;

        if(Keyboard.current != null)
        {
            if(Keyboard.current.aKey.isPressed)
                _moveInput.x -= 1f;

            if(Keyboard.current.dKey.isPressed)
                _moveInput.x += 1f;

            if(Keyboard.current.sKey.isPressed)
                _moveInput.y -= 1f;

            if(Keyboard.current.wKey.isPressed)
                _moveInput.y += 1f;
        }

        _moveInput =
            Vector2.ClampMagnitude(
                _moveInput,
                1f
            );

        if(Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            _isShootPressed = true;
        }

        if(_playerView != null)
        {
            _playerView.SetMovementParameter(
                _moveInput
            );
        }
    }

    private void FixedUpdate()
    {
        if(!HasStateAuthority || _rb == null)
            return;

        Vector3 movement =
            transform.forward * _moveInput.y +
            transform.right * _moveInput.x;

        movement.Normalize();

        Vector3 velocity =
            _rb.linearVelocity;

        velocity.x =
            movement.x * _speed;

        velocity.z =
            movement.z * _speed;

        _rb.linearVelocity = velocity;
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority ||
            !_isShootPressed)
            return;

        _isShootPressed = false;

        SpawnShot();
    }

    public void AddScore(int points)
    {
        if(!HasStateAuthority ||
            points <= 0)
            return;

        Score += points;
    }

    private void SpawnShot()
    {
        if(bulletPrefab == null ||
            bulletSpawnPoint == null)
        {
            Debug.LogError(
                "OnlinePlayer needs a bullet prefab and spawn point assigned.",
                this
            );

            return;
        }

        PlayerRef shooter = Object.InputAuthority;

        Runner.Spawn(
            bulletPrefab,
            bulletSpawnPoint.position,
            bulletSpawnPoint.rotation,
            shooter,
            (runner, networkObject) =>
            {
                Bullet bullet =
                    networkObject.GetComponent<Bullet>();

                if(bullet != null)
                {
                    bullet.SetShooter(shooter);
                }
            }
        );

        if(_playerView != null)
        {
            _playerView.PlayShoot();
        }
    }
    private void DebugScore()
    {
        Debug.Log(
            $"Score: {Score}"
        );
    }
}