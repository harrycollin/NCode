using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NCode;
using System.IO;

public class TextChat : MonoBehaviour
{
    public InputField ChannelID;
    public InputField MainMessage;
    public GameObject MessageBox;
    public GameObject MessageBoxElement;
    public Text steamID;

	void Start()
    {
        NClientManager.Connect("127.0.0.1", 5127);
    }
    void Awake()
    {
        NClientManager.SetPacketHandler(Packet.TextChat, PacketHandler);
        NClientManager.SetPacketHandler(Packet.TestData, PacketHandler);

    }

    void PacketHandler(Packet packet, BinaryReader reader)
    {
        if(packet == Packet.TextChat)
        {
            GameObject ele = (GameObject)Instantiate(MessageBoxElement);
            ele.transform.parent = MessageBox.transform;
            ele.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            ele.GetComponent<Text>().text = reader.ReadString();
            ele.GetComponent<Text>().color = Color.green;

        }
        if(packet == Packet.TestData)
        {
            Tools.Print(reader.ReadVector3().ToString());

            
        }
    }

    public void SendV3()
    {
        BinaryWriter writer = NClientManager.BeginSend(Packet.TestData, true);
        writer.Write(new Vector3(1, 43, 4));
        NClientManager.EndSend();
    }

    public void Send()
    {
        GameObject ele = (GameObject)Instantiate(MessageBoxElement);
        ele.transform.parent = MessageBox.transform;
        ele.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
        ele.GetComponent<Text>().text = MainMessage.text;
        ele.GetComponent<Text>().color = Color.blue;

        BinaryWriter writer = NClientManager.BeginSend(Packet.TextChat, true);
        writer.Write(MainMessage.text);
        NClientManager.EndSend();
    }

    public void JoinChannel()
    {
        NClientManager.JoinChannel(int.Parse(ChannelID.text));
    }

    public void LeaveChannel()
    {
        NClientManager.LeaveChannel(int.Parse(ChannelID.text));
    }

    public void ReceiveMessage(string message)
    {

    }
}

