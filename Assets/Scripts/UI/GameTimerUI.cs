using TMPro;
using Fusion;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _timerText;

    private void Update()
    {
        if(GameManager.Instance == null)
            return;

        float remainingTime =
            GameManager.Instance.GetRemainingTime();

        if(remainingTime <= 0f)
        {
            _timerText.text = "0:00";
            return;
        }

        int minutes =
            Mathf.FloorToInt(remainingTime / 60f);

        int seconds =
            Mathf.FloorToInt(remainingTime % 60f);

        _timerText.text =
            $"{minutes}:{seconds:00}";
    }
}