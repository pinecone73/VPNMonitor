using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using FileHash;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Principal;

/* TODO - Set timer to generate a masterlist every 6 hours
 * figure out a way to notify clients of a new update
 */

namespace VPNMonitor
{
    public class Service1 : VPNMonServer
    {
        /// <summary>
        /// Will take the store number and VPN address and store it away
        /// At present, will only return a string of "2"
        /// </summary>
        /// <param name="store">Store number from Client Machine Settings</param>
        /// <param name="vpnAddress">VPN address from Cisco VPN client</param>
        /// <returns>A number as a string that the client will use to process information</returns>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public string keepAlive(string store, string vpnAddress)
        {
            return string.Format("{0}", "2");
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public bool fileCheck(string directory, string fileName, string md5)
        {
            return FileHash.FileHash.fileCheck(directory, fileName, md5);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public string getFileString(string directory, string fileName)
        {
            return FileHash.FileHash.sharePath + directory + @"\" + fileName;
        }

        
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        private bool xmlGen()
        {
            return FileHash.FileHash.xmlGen();
        }
        
        [OperationBehavior(Impersonation=ImpersonationOption.Allowed)]
        private string genHash(string directory, string fileName)
        {
            return FileHash.FileHash.genHash(directory, fileName, false);
        }

        public Stream getMasterList()
        {
            FileStream stream;

            if (!File.Exists(FileHash.FileHash.xmlpath))  // MasterFiles.xml is missing
            {
                xmlGen();
            }
            stream = File.Open(FileHash.FileHash.xmlpath, FileMode.Open);

            return stream;
        }
    }
}
