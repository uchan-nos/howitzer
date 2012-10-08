using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Howitzer
{
    class Configuration
    {
        public DirectoryInfo DataDirectory
        {
            set;
            get;
        }

        // for singleton
        private static Configuration global = new Configuration();
        public static Configuration GetGlobal()
        {
            return global;
        }
    }
}
