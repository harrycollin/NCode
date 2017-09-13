using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NCode.Core;
using Buffer = NCode.Core.Buffer;
using NCode.Core.Utilities;

namespace NCode.Server.Core
{
    public sealed class NPacketProcessor
    {
        /// <summary>
        /// A dictionary of custom packet listeners. The key is the packet. 
        /// </summary>
        private readonly Dictionary<Packet, OnPacket> _packetHandlers = new Dictionary<Packet, OnPacket>();

        public delegate void OnPacket(Packet response, BinaryReader reader);

        public bool ProcessPacket(NPlayer player, Buffer buffer, bool reliable)
        {
            //Begin reading the packet. Returns the BinaryReader loaded with the memorystream to be read. 
            BinaryReader reader = buffer.BeginReading();

            //Identifies the packet.
            Packet packetType = (Packet) reader.ReadByte();
            
            if (packetType == 0) return true;
            //Filters out any packets that have custom handlers. 
            OnPacket callback;

            if (_packetHandlers.TryGetValue(packetType, out callback) && callback != null)
            {
                callback(packetType, reader);
                return true;
            }
            switch (packetType)
            {
                default:
                    {
                        Tools.Print($"Packet with the ID:{packetType} has not been defined for processing.", Tools.MessageType.error);
                        return false;
                    }
            }
            return true;
        }
    }
}
