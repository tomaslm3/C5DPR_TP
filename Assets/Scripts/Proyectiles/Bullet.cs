using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : NetworkBehaviour
{
    [Header("Bullet")]
    [SerializeField, Min(0.01f)]
    private float _initialForce = 25f;

    [SerializeField, Min(0.01f)]
    private float _lifeTime = 5f;

    [SerializeField, Min(1)]
    private int _damage = 1;

    private TickTimer _lifeTimer;
    private Rigidbody _rb;

    private bool _hasImpacted;

    public int Damage => _damage;

    [Networked]
    public PlayerRef Shooter { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.useGravity = true;

        _rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    public override void Spawned()
    {
        _lifeTimer = TickTimer.CreateFromSeconds(
            Runner,
            _lifeTime
        );

        Debug.Log(
            $"BULLET | StateAuthority: {Object.StateAuthority} | " +
            $"InputAuthority: {Object.InputAuthority} | " +
            $"Shooter: {Shooter}"
        );

        if(HasStateAuthority)
        {
            _rb.AddForce(
                transform.forward * _initialForce,
                ForceMode.Impulse
            );
        }
    }

    public void SetShooter(PlayerRef shooter)
    {
        if(!HasStateAuthority)
            return;

        Shooter = shooter;
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority)
            return;

        if(_lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.contactCount == 0)
            return;

        HandleImpact(
            collision.collider,
            collision.GetContact(0).point
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleImpact(
            other,
            other.ClosestPoint(transform.position)
        );
    }

    private void HandleImpact(
        Collider other,
        Vector3 hitPoint)
    {
        if(!HasStateAuthority || _hasImpacted)
            return;

        _hasImpacted = true;

        IHitTarget hitTarget = null;

        foreach(var behaviour in
                 other.GetComponentsInParent<MonoBehaviour>())
        {
            if(behaviour is IHitTarget target)
            {
                hitTarget = target;
                break;
            }
        }

        if(hitTarget != null)
        {
            hitTarget.OnHit(
                this,
                hitPoint
            );
        }

        Runner.Despawn(Object);
    }
}