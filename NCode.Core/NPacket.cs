namespace NCode.Core
{
    /// <summary>
    /// Contains all the posible packet definitions that can be sent/received. 
    /// </summary>
    public enum Packet
    {
        /// <summary>
        /// A simple empty packet. Can simply be used to keep the connection alive. 
        /// </summary>
        Empty, 
        
        /// <summary>
        /// Used to let the other half that a critical error has occured. This will lead to a immediate disconnect. 
        /// </summary>
        Error, 
        
        /// <summary>
        /// A safe way of letting the server know that you're going to disconnect. 
        /// </summary>
        Disconnect,   

        /// <summary>
        /// Sends and Receives Pings between server and client.
        /// </summary>
        Ping,

        /// <summary>
        /// First packet that should be sent to the server. Otherwise you'll get disconnected. Contains version, steam info etc. 
        /// </summary>
        RequestClientSetup,

        /// <summary>
        /// Response to RequestClientSetup. Sends back some infomation about you or disconnects you depending on your information. 
        /// </summary>
        ResponseClientSetup,

        /// <summary>
        /// Sent by the client requesting to setup a UDP connection
        /// </summary>
        RequestSetupUdp,

        /// <summary>
        /// Response from the server to setting up a UDP connection. 
        /// </summary>
        ResponseSetupUdp,

        /// <summary>
        /// Data to be forwarded to everyone. This is usually going to be an RFC.
        /// </summary>
        ForwardToAll,

        /// <summary>
        /// This will forward the data to anyone connected to the same channels as the player.
        /// </summary>
        ForwardToChannels,

        /// <summary>
        /// Request/Response packet for creating an entity
        /// </summary>
        CreateEntity,

        /// <summary>
        /// Request/Response for updating an Entity
        /// </summary>
        UpdateEntity,

        /// <summary>
        /// Request/Response for destroying an entity
        /// </summary>
        DestroyEntity,

        /// <summary>
        /// Request for transfering an Entity to another channel
        /// </summary>
        TransferEntity,

        /// <summary>
        /// Request/Response for Joining a channel
        /// </summary>
        JoinChannel,

        /// <summary>
        /// Request/Response for Joining a channel
        /// </summary>
        LeaveChannel,

           
    }
}
