using UnityEngine;
using NCode.Core.Client;
using System.IO;
using NCode.Utilities;
using NCode.Core;
using UnityEngine.SceneManagement;
using NCode;
using NCode.KleosTypes.Virtual;
using System;
using NCode.Core.BaseClasses;
using KleosTypes.Virtual;

public class GameManager : MonoBehaviour
{
 
    /// <summary>
    /// The current build version of this client,
    /// </summary>
    public static string BuildVersion {  get { return ReadTextFile("version.txt"); } }

    public static string ClientID = "1234567890";


    // Use this for initialization
    void Start ()
    {
        //Key step
        NetworkManager.CreateInstance();

        //Set packets here
        NetworkManager.SetPacketHandler(Packet.ResponsePlayerInfo, PacketHandler);
        
        //Set delegates here
        NetworkManager.onConnect += OnConnect;
        NetworkManager.onSpawnPlayerResponse += SpawnPlayer;
        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        ConnectToServer("80.6.198.6", 5127);
        OverlayClientInfo();

        CoreProperties.PlayerInformation.steamid = ClientID;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    void Update ()
    {
        
	}
  
    void SpawnPlayer(NetworkObject player)
    {
        GameObject obj = (GameObject)Instantiate(CoreProperties.PrefabConfig.GetPrefabByID(player.prefabid));
        obj.transform.position = NUnityTools.V3ToVector3(player.position);
        obj.GetComponent<NetworkBehaviour>().networkObject = player;
        CoreProperties.PlayerGameObject = obj;
    }

    void PacketHandler(Packet response, BinaryReader reader)
    {
        switch (response)
        {
            case Packet.ResponsePlayerInfo:
                {
                    if (reader.ReadBoolean())
                    {
                        CoreProperties.PlayerInformation = (PlayerInfo)reader.ReadObject();
                        SceneManager.LoadScene(1);
                        break;
                    }
                    else
                    {
                        CoreProperties.PlayerInformation = new PlayerInfo();
                        SceneManager.LoadScene(2);
                        break;
                    }
                }
            case Packet.ResponseSpawnPlayerObject:
                {

                    break;
                }
        }
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {

        switch (scene.buildIndex)
        {
            case 0:
                {
                    //Main Menu
                    break;
                }
            case 1:
                {
                    NetworkManager.SpawnPlayer(new Vector3(-250f, 0f, 250), new Quaternion(0,0,0,0));
                    UIWindowManager.InitializeUI();

                    break;
                }
            case 2:
                {
                    
                    break;
                }
            
        }
    }

    public static void ConnectToServer(string ip, int port)
    {
        NetworkManager.Connect(ip, port);
    }

    private void OnConnect()
    {
        BinaryWriter writer = NetworkManager.BeginSend(Packet.RequestPlayerInfo);
        writer.WriteObject(CoreProperties.PlayerInformation);
        NetworkManager.EndSend(true);
    }

    void OverlayClientInfo()
    {
        gameObject.AddComponent<GUIText>();
        gameObject.transform.position = new Vector3(0f, 1f, 0.0f);
        gameObject.GetComponent<GUIText>().text = BuildVersion;
        gameObject.GetComponent<GUIText>().color = Color.black;
    }


    static string ReadTextFile(string sFileName)
    {
        //Check to see if the filename specified exists, if not try adding '.txt', otherwise fail
        string sFileNameFound = "";
        if (File.Exists(sFileName))
        {
            //Debug.Log("Reading '" + sFileName + "'.");
            sFileNameFound = sFileName; //file found
        }
        else if (File.Exists(sFileName + ".txt"))
        {
            sFileNameFound = sFileName + ".txt";
        }
        else
        {
            Debug.Log("Could not find file '" + sFileName + "'.");
            return null;
        }

        StreamReader sr;
        try
        {
            sr = new StreamReader(sFileNameFound);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Something went wrong with read.  " + e.Message);
            return null;
        }

        string fileContents = sr.ReadToEnd();
        sr.Close();

        return fileContents;
    }


  
}
