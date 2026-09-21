using UnityEngine;
using Fusion;

public abstract class Target : NetworkBehaviour, IHitTarget
{
    [Header("Target")]
    [SerializeField, Min(1)]
    private int _maxLife = 1;

    [SerializeField, Min(0)]
    private int _points = 100;

    [Networked]
    public int CurrentLife { get; private set; }

    public int MaxLife => _maxLife;

    public int Points => _points;

    public bool IsAlive => CurrentLife > 0;

    private bool _destroyed;

    private TickTimer _despawnTimer;

    public override void Spawned()
    {
        if(HasStateAuthority)
        {
            CurrentLife = _maxLife;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority)
            return;

        if(_despawnTimer.Expired(Runner))
        {
            _despawnTimer = TickTimer.None;

            Runner.Despawn(Object);
        }
    }

    public void OnHit(
        Bullet bullet,
        Vector3 hitPoint)
    {
        if(bullet == null)
            return;

        if(!bullet.Shooter.IsValid)
            return;

        if(_destroyed)
            return;

        if(HasStateAuthority)
        {
            ApplyHit(
                bullet.Damage,
                hitPoint,
                bullet.Shooter
            );

            return;
        }

        RPC_RequestHit(
            bullet.Damage,
            hitPoint,
            bullet.Shooter
        );
    }

    [Rpc(
        RpcSources.All,
        RpcTargets.StateAuthority
    )]
    private void RPC_RequestHit(
        int damage,
        Vector3 hitPoint,
        PlayerRef shooter)
    {
        if(!HasStateAuthority)
            return;

        if(_destroyed)
            return;

        if(!IsAlive)
            return;

        if(damage <= 0)
            return;

        ApplyHit(
            damage,
            hitPoint,
            shooter
        );
    }

    private void ApplyHit(
        int damage,
        Vector3 hitPoint,
        PlayerRef shooter)
    {
        if(_destroyed)
            return;

        if(!IsAlive)
            return;

        CurrentLife -= damage;

        Debug.Log(
            $"{name} received {damage} damage. " +
            $"Life: {CurrentLife}/{MaxLife} | " +
            $"Shooter: {shooter}"
        );

        OnDamaged(
            damage,
            hitPoint
        );

        if(CurrentLife <= 0)
        {
            CurrentLife = 0;
            _destroyed = true;

            // Los puntos se otorgan inmediatamente.
            AwardPoints(
                shooter,
                Points
            );

            // Cada Target decide qué hacer y
            // cuándo desaparecer.
            OnDestroyed();
        }
    }

    private void AwardPoints(
        PlayerRef shooter,
        int points)
    {
        if(!shooter.IsValid)
            return;

        Debug.Log(
            $"AWARD SCORE | Shooter: {shooter} | " +
            $"Points: {points}"
        );

        RPC_AwardPoints(
            shooter,
            points
        );
    }

    [Rpc]
    private void RPC_AwardPoints(
        [RpcTarget] PlayerRef target,
        int points)
    {
        if(target != Runner.LocalPlayer)
            return;

        if(!Runner.TryGetPlayerObject(
            target,
            out NetworkObject playerObject))
        {
            Debug.LogWarning(
                $"No se encontró PlayerObject para {target}"
            );

            return;
        }

        OnlinePlayer player =
            playerObject.GetComponent<OnlinePlayer>();

        if(player == null)
        {
            Debug.LogWarning(
                $"El PlayerObject de {target} " +
                $"no tiene OnlinePlayer."
            );

            return;
        }

        Debug.Log(
            $"ADDING SCORE | Player: {target} | " +
            $"Points: {points}"
        );

        player.AddScore(points);
    }

    protected void DespawnAfter(float seconds)
    {
        if(!HasStateAuthority)
            return;

        if(seconds <= 0f)
        {
            Runner.Despawn(Object);
            return;
        }

        _despawnTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                seconds
            );
    }

    protected virtual void ApplyDamage(
        int damage,
        Vector3 hitPoint)
    {
    }

    protected virtual void OnDamaged(
        int damage,
        Vector3 hitPoint)
    {
    }

    protected virtual void OnDestroyed()
    {
        Debug.Log(
            $"{name} destroyed. " +
            $"Points: {Points}"
        );
    }
}