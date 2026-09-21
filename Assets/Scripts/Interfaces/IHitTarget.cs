using UnityEngine;

/// <summary>
/// Implement this on any target that can be hit by a bullet.
/// </summary>
public interface IHitTarget
{
    int Points { get; }

    void OnHit(
        Bullet bullet,
        Vector3 hitPoint
    );
}