using TMPro;
using UnityEngine;

public class WaitingForPlayersUI : MonoBehaviour
{
    [SerializeField] private GameObject _root; // panel o texto a mostrar u ocultar
    [SerializeField] private TMP_Text _statusText;

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            SetVisible(false);
            return;
        }

        bool isWaiting = !GameManager.Instance.GameStarted;

        SetVisible(isWaiting);

        if (isWaiting && _statusText != null)
        {
            _statusText.text =
                $"Esperando a los mejores jugadores... " +
                $"({GameManager.Instance.ConnectedPlayers}/{GameManager.Instance.RequiredPlayers})";
        }
    }

    private void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }
}