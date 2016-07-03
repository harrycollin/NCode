namespace NCode
{
    public enum Packet
    {
        Empty, 
        
        Error, 
        
        Disconnect,       

        RequestJoinChannel,

        ResponseJoinChannel,

        RequestLeaveChannel,

        ResponseLeaveChannel,

        TestString,

        RequestPing,

        ResponsePing,

        RequestClientInfo,

        ResponseClientInfo,

        RequestCreateObject,

        ResponseCreateObject,

        RequestDestroyObject,

        ResponseDestroyObject,

        /// <summary>
        /// Server to client packet for updating network objects. Also used to initially send a object. 
        /// </summary>
        ClientObjectUpdate,

        ServerObjectUpdate,

        Broadcast,

        TextChat,

        
    }
}
