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
        /// Sends udp port to clients.
        /// </summary>
        SetupUDP,   
        
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
        /// Request to create an NetworkObject. 
        /// </summary>
        RequestCreateObject,

        /// <summary>
        /// Response to RequestCreateObject. Either declines or sends back your object data to be spawned. 
        /// </summary>
        ResponseCreateObject,

        /// <summary>
        /// Requests to delete an object. 
        /// </summary>
        RequestDestroyObject,

        /// <summary>
        /// Response to RequestDestroyObject. 
        /// </summary>
        ResponseDestroyObject,   

        /// <summary>
        /// Server to client packet for updating network objects. Also used to initially send a object. 
        /// </summary>
        ClientObjectUpdate,

        /// <summary>
        /// Client to server packet for updating network objects. 
        /// </summary>
        ServerObjectUpdate,   

        /// <summary>
        /// Global packet for Remote Function Calls. Set as first priority on all switch cases. 
        /// </summary>
        RFC,

        /// <summary>
        /// Server to client packet notifying of a newly connected player
        /// </summary>
        PlayerUpdate,

        /// <summary>
        /// Sets the position of the current player object. This is required to categories the player into a channel
        /// </summary>
        PlayerPositionUpdate,

        /// <summary>
        /// Request to set the player object. This is tracked so that the player is synchronising with the right players.
        /// </summary>
        RequestSetPlayerObject,

        /// <summary>
        /// Response to whether the object can be set as the player's 'playerObject'.
        /// </summary>
        ResponseSetPlayerObject,

        /// <summary>
        /// Request to spawn. This is using the playerobject set by the client.s
        /// </summary>
        RequestSpawnPlayerObject,

        /// <summary>
        /// The response to the spawn request. This response will trigger the spawn. 
        /// </summary>
        ResponseSpawnPlayerObject,

        /// <summary>
        /// Data to be forwarded to everyone. This is usually going to be an RFC.
        /// </summary>
        ForwardToAll,

        /// <summary>
        /// This will forward the data to anyone connected to the same channels as the player.
        /// </summary>
        ForwardToChannels,

        // Kleos Packets Below //

        /// <summary>
        /// The first packet sent after the initial setup of the client. Should send any required information such as SteamID so the server can identify you. 
        /// </summary>
        RequestPlayerInfo,

        /// <summary>
        /// The response to RequestPlayerInfo. This will contain any information needed for the client so that it can decide what to do next. For example if you're a new player then 
        /// the server will reply with information requesting you send your character data.
        /// </summary>
        ResponsePlayerInfo,

        /// <summary>
        /// Request to save your characters data. This should be sent as a serialised PlayerInfo object. 
        /// </summary>
        SavePlayerData,

       
    }
}
