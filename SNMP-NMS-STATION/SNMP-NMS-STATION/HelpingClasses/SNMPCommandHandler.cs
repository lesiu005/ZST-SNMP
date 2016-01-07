using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SNMP_NMS_STATION
{
    class SNMPCommandHandler
    {
        public Variable[,] SNMP_GET_TABLE(string address)
        {
            var result = Messenger.GetTable(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                new OctetString("public"),
                new ObjectIdentifier(address),
                60000,
                1,
                null);

            return result;
        }
        public Variable SNMP_GET (string adres)
        {
            var result = Messenger.Get(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres)) },
                           60000);
            return result[0];
        }
        public void SNMP_SET (string adres, string value)
        {
            var result = Messenger.Set(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres), new OctetString(value)) },
                           60000);
        }
        
        
    }
}
