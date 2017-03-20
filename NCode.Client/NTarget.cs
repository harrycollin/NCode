namespace NCode.Client
{
    public enum Target
    {
        /// <summary>
        /// Echo the packet to everyone in the room.
        /// </summary>

        All,

        /// <summary>
        /// Echo the packet to everyone in the room and everyone who joins later.
        /// </summary>

        AllSaved,

        /// <summary>
        /// Echo the packet to everyone in the room except the sender.
        /// </summary>

        Others,

        /// <summary>
        /// Echo the packet to everyone in the room (except the sender) and everyone who joins later.
        /// </summary>

        OthersSaved,

        /// <summary>
        /// Echo the packet to the room's host.
        /// </summary>

        Host,

        /// <summary>
        /// Broadcast this packet to everyone connected to the server.
        /// </summary>

        Broadcast,

        /// <summary>
        /// Send this packet to administrators.
        /// </summary>

        Admin,
    }
}
