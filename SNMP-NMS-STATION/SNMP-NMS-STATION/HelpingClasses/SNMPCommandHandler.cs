using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpSharpNet;
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
                new Lextm.SharpSnmpLib.OctetString("public"),
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
                           new Lextm.SharpSnmpLib.OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres)) },
                           60000);
            return result[0];
        }
        public string SNMP_SET(string adres, string value)
        {
            // Prepare target
            UdpTarget target = new UdpTarget((IPAddress)new IpAddress("127.0.0.1"));
            // Create a SET PDU
            Pdu pdu = new Pdu(PduType.Set);
            // Set a value to integer
            pdu.VbList.Add(new Oid(adres), new SnmpSharpNet.Integer32(int.Parse(value)));
            
            // Set Agent security parameters
            AgentParameters aparam = new AgentParameters(SnmpVersion.Ver2, new SnmpSharpNet.OctetString("public"));
            // Response packet
            SnmpV2Packet response;
            try
            {
                // Send request and wait for response
                response = target.Request(pdu, aparam) as SnmpV2Packet;
            }
            catch (Exception ex)
            {
                // If exception happens, it will be returned here
                target.Close();
                return String.Format("Request failed with exception: {0}", ex.Message);
                
                
            }
            // Make sure we received a response
            if (response == null)
            {
                return ("Error in sending SNMP request.");
            }
            else
            {
                // Check if we received an SNMP error from the agent
                if (response.Pdu.ErrorStatus != 0)
                {
                    return (String.Format("SNMP agent returned ErrorStatus {0} on index {1}",
                        response.Pdu.ErrorStatus, response.Pdu.ErrorIndex));
                }
                else
                {
                    // Everything is ok. Agent will return the new value for the OID we changed
                    return (String.Format("Agent response {0}: {1}",
                        response.Pdu[0].Oid.ToString(), response.Pdu[0].Value.ToString()));
                }
            }
        }

        }
}
