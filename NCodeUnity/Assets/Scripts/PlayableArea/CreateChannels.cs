using UnityEngine;


/// <summary>
/// Generates the channel regions.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CreateChannels : MonoBehaviour
{

    BoxCollider Area;

    public float X;
    public float Z;
    public float Y;

    public float ChannelsOnX;
    public float ChannelsOnZ;
    public float ChannelsOnY;

    private float ChannelWidthOnX;
    private float ChannelWidthOnZ;
    private float ChannelWidthOnY;

    int ChannelID = 6;

    void Start()
    {
        Area = GetComponent<BoxCollider>();
        X = Area.size.x;
        Z = Area.size.z;
        Y = Area.size.y;

        ChannelWidthOnX = X / ChannelsOnX;
        ChannelWidthOnZ = Z / ChannelsOnZ;
        ChannelWidthOnY = Y / ChannelsOnY;

        Vector3 firstChannelPos = new Vector3(Area.transform.position.x - Area.size.x / 2 + ChannelWidthOnX / 2, Area.transform.position.y - Area.size.y / 2 + ChannelWidthOnY / 2, Area.transform.position.z - Area.size.z / 2 + ChannelWidthOnZ / 2);


        bool firstChannelX = true;
        bool firstChannelZ = true;
        bool firstChannelY = true;

        for (int i = 0; i < ChannelsOnX; i++)
        {

            GameObject Channel = new GameObject("Channel " + ChannelID, typeof(BoxCollider));
            Channel.AddComponent<WorldChannel>().ID = ChannelID;
            Channel.transform.parent = transform;
            ChannelID++;

            if (!firstChannelX)
            {

                Channel.transform.position = firstChannelPos + new Vector3(ChannelWidthOnX * i, 0, 0);

            }
            else
            {
                Channel.transform.position = firstChannelPos;
            }

            BoxCollider extents = Channel.GetComponent<BoxCollider>() as BoxCollider;
            extents.size = new Vector3(ChannelWidthOnX, ChannelWidthOnY, ChannelWidthOnZ);
            extents.isTrigger = true;
            firstChannelX = false;

            ///

            for (int p = 1; p < ChannelsOnY; p++)
            {

                GameObject Channel1 = new GameObject("Channel " + ChannelID, typeof(BoxCollider));
                Channel1.AddComponent<WorldChannel>().ID = ChannelID;
                Channel1.transform.parent = transform;
                ChannelID++;

                if (!firstChannelY)
                {

                    Channel1.transform.position = firstChannelPos + new Vector3(Channel.transform.position.x - ChannelWidthOnX / 2, ChannelWidthOnY * p , 0);

                }
                else
                {
                    Channel1.transform.position = firstChannelPos + new Vector3(0, ChannelWidthOnY, 0);
                }

                BoxCollider extents2 = Channel1.GetComponent<BoxCollider>() as BoxCollider;
                extents2.size = new Vector3(ChannelWidthOnX, ChannelWidthOnY, ChannelWidthOnZ);
                extents2.isTrigger = true;
                firstChannelY = false;
            }

            for (int o = 1; o < ChannelsOnZ; o++)
            {

                GameObject Channel2 = new GameObject("Channel " + ChannelID, typeof(BoxCollider));
                Channel2.AddComponent<WorldChannel>().ID = ChannelID;
                Channel2.transform.parent = transform;
                ChannelID++;

                if (!firstChannelZ)
                {

                    Channel2.transform.position = firstChannelPos + new Vector3(Channel.transform.position.x - ChannelWidthOnX / 2, 0, ChannelWidthOnZ * o);

                }
                else
                {
                    Channel2.transform.position = firstChannelPos + new Vector3(Channel.transform.position.x - ChannelWidthOnX / 2, 0, ChannelWidthOnZ);
                }

                BoxCollider extents1 = Channel2.GetComponent<BoxCollider>() as BoxCollider;
                extents1.size = new Vector3(ChannelWidthOnX, ChannelWidthOnY, ChannelWidthOnZ);
                extents1.isTrigger = true;
                firstChannelZ = false;

                ///

                for (int p = 1; p < ChannelsOnY; p++)
                {

                    GameObject Channel3 = new GameObject("Channel " + ChannelID, typeof(BoxCollider));
                    Channel3.AddComponent<WorldChannel>().ID = ChannelID;
                    Channel3.transform.parent = transform;
                    ChannelID++;

                    if (!firstChannelY)
                    {

                        Channel3.transform.position = firstChannelPos + new Vector3(Channel.transform.position.x - ChannelWidthOnX / 2, ChannelWidthOnY * p, Channel2.transform.position.z - ChannelWidthOnZ / 2);

                    }
                    else
                    {
                        Channel3.transform.position = firstChannelPos + new Vector3(0, ChannelWidthOnY, 0);
                    }

                    BoxCollider extents2 = Channel3.GetComponent<BoxCollider>() as BoxCollider;
                    extents2.size = new Vector3(ChannelWidthOnX, ChannelWidthOnY, ChannelWidthOnZ);
                    extents2.isTrigger = true;
                    firstChannelY = false;
                }
            }
        }
    }
}