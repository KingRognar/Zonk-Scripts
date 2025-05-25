using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Scores_Scr : NetworkBehaviour
{
    [SerializeField] private Transform scoresTrans;
    private Dictionary<ulong, TMP_Text> listOfScores = new Dictionary<ulong, TMP_Text> ();
    private int scoreToEnable = 0;

    public void EnableAnotherScore(ulong clientId)
    {
        if (scoreToEnable >= 3) return;

        TMP_Text text = scoresTrans.GetChild(scoreToEnable++).GetComponent<TMP_Text>();
        text.gameObject.SetActive(true);
        text.text = "Player " + (clientId + 1) + ": " + 0; //TODO: возможно надо собирать скор с игроков?
        listOfScores.Add(clientId, text);
    }
    [Rpc(SendTo.NotMe)]
    public void EnableAnotherScoreRpc(ulong clientId)
    {
        if (scoreToEnable >= 3) return;

        TMP_Text text = scoresTrans.GetChild(scoreToEnable++).GetComponent<TMP_Text>();
        text.gameObject.SetActive(true);
        text.text = "Player " + (clientId + 1) + ": " + 0;
        listOfScores.Add(clientId, text);
    }
    [Rpc(SendTo.NotMe)]
    public void UpdatePlayerScoreRpc(ulong clientId, int newScore)
    {
        if (!listOfScores.ContainsKey(clientId)) { Debug.Log("не нашёл скор с таким ID"); return; }

        listOfScores[clientId].text = "Player " + (clientId + 1) + ": " + newScore;
    }

}
