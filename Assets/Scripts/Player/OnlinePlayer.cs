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

    [Header("Aiming")]
    [SerializeField]
    private Transform _aimPivot; // NUEVO: pivote que rota en X (pitch), padre del arma/spawn point

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

    [Networked]
    public float AimPitch { get; set; } // NUEVO: pitch replicado a todos los clientes

    private bool _isShootPressed;
    private Vector2 _moveInput;
    private float _pendingPitch; // NUEVO: último pitch leído localmente

    private Rigidbody _rb;
    private CameraMovement _cameraMovement; // NUEVO

    public override void Spawned()
    {
        Debug.Log(
            $"ONLINE PLAYER | " +
            $"StateAuthority: {Object.StateAuthority} | " +
            $"InputAuthority: {Object.InputAuthority}"
        );

        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        if(HasStateAuthority)
        {
            Score = 0;

            var mainCam = Camera.main;
            _cameraMovement = mainCam.GetComponent<CameraMovement>();

            _cameraMovement.SetTarget(
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

        if(_cameraMovement != null)
        {
            _pendingPitch = _cameraMovement.Pitch; // NUEVO
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
        if(!HasStateAuthority)
            return;

        AimPitch = _pendingPitch; // NUEVO: se replica a todos los clientes

        if(_isShootPressed)
        {
            _isShootPressed = false;
            SpawnShot();
        }
    }

    public override void Render()
    {
        // NUEVO: aplica el pitch al pivote en TODOS los clientes,
        // para que el torso/arma se vea inclinado también en remotos.
        if(_aimPivot != null)
        {
            _aimPivot.localRotation =
                Quaternion.Euler(AimPitch, 0f, 0f);
        }
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
            bulletSpawnPoint.rotation, // ya incluye el pitch heredado del AimPivot
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