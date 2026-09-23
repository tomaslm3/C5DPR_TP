using UnityEngine;

public interface IHitTarget
{
    int Points { get; }

    void OnHit(
        Bullet bullet,
        Vector3 hitPoint
    );
}