using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        OnlinePlayer player = other.GetComponentInParent<OnlinePlayer>();

        if(player == null)
            return;

        player.SetNearAmmoBox(true);
    }

    private void OnTriggerExit(Collider other)
    {
        OnlinePlayer player = other.GetComponentInParent<OnlinePlayer>();

        if(player == null)
            return;

        player.SetNearAmmoBox(false);
    }
}