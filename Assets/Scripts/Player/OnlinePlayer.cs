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
    private Transform _aimPivot;

    [Header("Shooting")]
    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private Transform bulletSpawnPoint;

    [Header("Ammo")]
    [SerializeField, Min(1)]
    private int _magazineCapacity = 5;

    [SerializeField, Min(1)]
    private int _magazineCount = 3;

    [Networked, Capacity(3)]
    public NetworkArray<int> Magazines => default;

    [Networked]
    public int CurrentMagazineIndex { get; set; }

    public int CurrentAmmo =>
        Magazines[CurrentMagazineIndex];

    public int MagazineCapacity =>
        _magazineCapacity;

    public int MagazineCount =>
    _magazineCount;

    public int GetMagazineAmmo(int index) =>
        Magazines[index];

    [Networked, OnChangedRender(nameof(DebugScore))]
    public int Score
    {
        get => default;
        private set { }
    }

    [Networked]
    public float AimPitch { get; set; }

    private bool _isShootPressed;
    private bool _isReloadPressed;
    private bool _isRefillPressed;
    private bool _nearAmmoBox;

    private Vector2 _moveInput;
    private float _pendingPitch;

    private Rigidbody _rb;
    private CameraMovement _cameraMovement;

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

            for(int i = 0; i < _magazineCount; i++)
            {
                Magazines.Set(i, _magazineCapacity);
            }

            CurrentMagazineIndex = 0;

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

            if(Keyboard.current.rKey.wasPressedThisFrame)
                _isReloadPressed = true;

            if(Keyboard.current.eKey.wasPressedThisFrame)
                _isRefillPressed = true;
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
            _pendingPitch = _cameraMovement.Pitch;
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

        AimPitch = _pendingPitch;

        if(_isReloadPressed)
        {
            _isReloadPressed = false;
            TryReload();
        }

        if(_isRefillPressed)
        {
            _isRefillPressed = false;
            TryRefillAmmo();
        }

        if(_isShootPressed)
        {
            _isShootPressed = false;
            SpawnShot();
        }
    }

    public override void Render()
    {
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
    public void SetNearAmmoBox(bool isNear)
    {
        _nearAmmoBox = isNear;
    }

    private void TryReload()
    {
        for(int offset = 1; offset <= _magazineCount; offset++)
        {
            int candidateIndex =
                (CurrentMagazineIndex + offset) % _magazineCount;

            if(Magazines[candidateIndex] > 0)
            {
                CurrentMagazineIndex = candidateIndex;

                Debug.Log(
                    $"RELOAD | Cambiado a cargador {CurrentMagazineIndex} " +
                    $"({Magazines[CurrentMagazineIndex]} balas)"
                );

                return;
            }
        }

        Debug.Log("RELOAD | No hay cargadores con munición disponible.");
    }

    private void TryRefillAmmo()
    {
        if(!_nearAmmoBox)
        {
            Debug.Log("REFILL | No estás cerca de la caja de munición.");
            return;
        }

        for(int i = 0; i < _magazineCount; i++)
        {
            Magazines.Set(i, _magazineCapacity);
        }

        Debug.Log("REFILL | Cargadores rellenados.");
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

        if(Magazines[CurrentMagazineIndex] <= 0)
        {
            Debug.Log("SHOOT | Cargador vacío, presioná R para recargar.");
            return;
        }

        Magazines.Set(
            CurrentMagazineIndex,
            Magazines[CurrentMagazineIndex] - 1
        );

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