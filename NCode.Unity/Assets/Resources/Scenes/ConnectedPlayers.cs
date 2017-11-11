using NCode.Client;
using NCode.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPlayers : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        NetworkManager.OnPlayerConnect += PrintPlayer;
	}
	
	// Update is called once per frame
	void PrintPlayer (NPlayerInfo player)
    {
        print(string.Format("Player: {0} has connected.", player.PlayerID));
	}
}
