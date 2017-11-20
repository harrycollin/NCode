using NCode.Client;
using NCode.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPlayers : MonoBehaviour
{

	// Use this for initialization
	void Awake ()
    {
        NetworkManager.OnPlayerConnect += PrintPlayer;
        NetworkManager.OnPlayerDisconnect += PlayerDisconnected;
    }
	
	// Update is called once per frame
	void PrintPlayer (NPlayerInfo player)
    {
        print(string.Format("Player: {0} has connected.", player.PlayerID));
	}

    void PlayerDisconnected(NPlayerInfo player)
    {
        print(string.Format("Player: {0} has disconnected.", player.PlayerID));
    }
}
