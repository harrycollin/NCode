using NCode.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode
{
    class NCodeServerMain : NMainThread
    {
        /// <summary>
        /// Server Entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            NCodeServerMain app = new NCodeServerMain();
            app.Start();
        }


        public bool Start()
        {
            base.Start(5127);
            return false;
        }
    }
}
