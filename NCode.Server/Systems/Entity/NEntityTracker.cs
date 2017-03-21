using NCode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCode.Core.Entity;
using NCode.Core.TypeLibrary;

namespace NCode.Server
{
    public static class NEntityTracker
    {


        static bool _runThreads = false;
        
        static HashSet<KeyValuePair<Guid, NNetworkEntity>> _activeEntities = new HashSet<KeyValuePair<Guid, NNetworkEntity>>();


        /// <summary>
        /// Returns the distance between two NVector3 positions. 
        /// </summary>
        private static float CalculateDistance3D(NVector3 pos1, NVector3 pos2)
        {
            return (float)Math.Sqrt((pos1.X - pos2.X) * (pos1.X - pos2.X) + (pos1.Y - pos2.Y) * (pos1.Y - pos2.Y) + (pos1.Z - pos2.Z) * (pos1.Z - pos2.Z));
        }

       
    }
}
