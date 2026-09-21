using UnityEngine;

public class BottleTarget : Target
{
    [Header("Bottle")]
    [SerializeField] private GameObject _explosionVFX;

    [Header("Despawn")]
    [SerializeField] private float _despawnDelay = 0.15f;

    protected override void OnDestroyed()
    {
        base.OnDestroyed();

        Debug.Log("Bottle exploded!");

        if(_explosionVFX != null)
        {
            Instantiate(
                _explosionVFX,
                transform.position,
                Quaternion.identity
            );
        }

        DespawnAfter(_despawnDelay);
    }
}