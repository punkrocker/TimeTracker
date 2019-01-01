using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TimeTracker.Util
{
    class ConfigFile
    {
        public static XMLHandler.TXmlConfigHandler Languege
        {
            get
            {
                string fileName = "Lang/en-GB.xml";
                if (File.Exists(fileName))
                    return new XMLHandler.TXmlConfigHandler(fileName);
                else
                    throw new Exception("File Not Found");
            }
        }
    }
}
