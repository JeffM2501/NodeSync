using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace NodeController
{
    public class ControllerConfig
    {
        public List<string> ListenPrefixes = new List<string>();
        public bool DecodeKeys = false;

        public class HostConnection
        {
            public string Name = string.Empty;
            public string PublicKey = string.Empty;
        }
        public List<HostConnection> Hosts = new List<HostConnection>();

        public string RootTempFolder = string.Empty;


        public static ControllerConfig ReadConfig(string path)
        {
            FileInfo file = new FileInfo(path);

            XmlSerializer xml = new XmlSerializer(typeof(ControllerConfig));
            ControllerConfig cfg = new ControllerConfig();

            if (!file.Exists)
            {
                cfg.ListenPrefixes.Add("http://localhost:80");
                HostConnection c = new HostConnection();
                c.Name = "A host";
                c.PublicKey = "A key";
                cfg.Hosts.Add(c);

                cfg.RootTempFolder = Path.GetTempPath();

                var fs = file.OpenWrite();
                xml.Serialize(fs, cfg);
                fs.Close();
         
            }
            else
            {
                var fs = file.OpenText();
                cfg = xml.Deserialize(fs) as ControllerConfig;
                fs.Close();
            }

            return cfg;
        }
    }
}
