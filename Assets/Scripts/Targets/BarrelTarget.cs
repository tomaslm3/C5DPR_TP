using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BarrelTarget : Target
{
    [Header("Push")]
    [SerializeField] private float _pushForce = 5f;

    [Header("Despawn")]
    [SerializeField] private float _despawnDelay = 3f;

    private Rigidbody _rigidbody;

    public override void Spawned()
    {
        base.Spawned();

        _rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnDamaged(
        int damage,
        Vector3 hitPoint)
    {
        base.OnDamaged(
            damage,
            hitPoint
        );

        Vector3 direction =
            (transform.position - hitPoint).normalized;

        direction.y = 0f;

        _rigidbody.AddForce(
            direction * _pushForce,
            ForceMode.Impulse
        );
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();

        Debug.Log(
            $"Barrel destroyed. " +
            $"Despawning in {_despawnDelay} seconds."
        );

        DespawnAfter(_despawnDelay);
    }
}