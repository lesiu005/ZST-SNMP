using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNMP_NMS_STATION;
using Lextm.SharpSnmpLib;
using Newtonsoft.Json;

namespace RestWebService.SNMPAccessLayer
{
    public class AccessLayer
    {
        private SNMPCommandHandler SNMPHandler;
        public AccessLayer()
        {
            this.SNMPHandler = new SNMPCommandHandler();
        }
        public String Get(String uidValue)
        {
            ResultData resultData = new ResultData(SNMPHandler.SNMP_GET(uidValue));
            return JsonConvert.SerializeObject(resultData);
        }
    }
    /// <summary>
    /// Represent Variable.ToString() data in more managable way
    /// </summary>
    public class ResultData
    {
        public String ID { get; set; }
        public String Data { get; set; }
        public ResultData(Variable value)
        {
            this.ID = value.Id.ToString();
            this.Data = value.Data.ToString();
        }
    }
}
