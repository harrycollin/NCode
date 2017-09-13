using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCode.Server.Core;

namespace NCode.Server.Systems
{
    interface IPlayerManagementSystem
    {
        event NPlayer.PlayerRemoved playerRemoved;
    }
}
