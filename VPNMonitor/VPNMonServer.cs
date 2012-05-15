using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace VPNMonitor
{
    [ServiceContract]
    public interface VPNMonServer
    {
        [OperationContract]
        string keepAlive(string value, string vpnAddress);

        [OperationContract]
        bool fileCheck(string directory, string fileName, string md5);

        [OperationContract]
        string getFileString(string directory, string fileName);

        [OperationContract]
        Stream getMasterList();
    }
}
