using Unity.Netcode;
using UnityEngine;

public class BotPlayer_Scr : BasePlayer_Scr
{
    protected override void Initialize()
    {

        SetupCupAndDices();
        SetupHands();
        SetInitialPositions();
    }

    protected override void SetupCupAndDices()
    {
        cup = transform.GetChild(1).GetComponent<Cup_Scr>();
        cup.player = this;
        cup.transform.parent = transform;


        diceSet = new();
        for (int i = 0; i < 6; i++)
        {
            Transform diceTrans = transform.GetChild(2 + i);

            diceSet.Add(diceTrans.GetComponent<Dice_Scr>());
            diceSet[i].player = this;
            diceSet[i].transform.parent = transform;
            diceSet[i].id = i;
        }

        LoadDiceColoringSchemes();
    }
    protected override void SetupHands()
    {
        /*if (!transform.GetChild(1).TryGetComponent<Hands_Scr>(out hands))
            Debug.Log("не получилось взять реф рук", this);*/
        //hands = transform.GetChild(1).GetComponent<Hands_Scr>();

        ulong clientHandsId = 5 + (NetworkManager.Singleton.LocalClientId * 9);
        NetworkObject netHands;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientHandsId, out netHands)) Debug.Log("не получилось взять netHands");

        hands = netHands.GetComponent<Hands_Scr>();
        hands.transform.parent = transform;
    }
}
