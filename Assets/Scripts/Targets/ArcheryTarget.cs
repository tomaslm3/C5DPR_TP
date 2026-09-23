using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkMecanimAnimator))]
public class ArcheryTarget : Target
{
    private static readonly int FallTrigger = Animator.StringToHash("Fall");

    [Header("Despawn")]
    [SerializeField, Min(0f)]
    private float _despawnDelay = 3f;

    private NetworkMecanimAnimator _animator;

    private void Awake()
    {
        _animator = GetComponent<NetworkMecanimAnimator>();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();

        _animator.SetTrigger(FallTrigger);

        DespawnAfter(_despawnDelay);
    }
}