using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trx.Messaging.Iso8583;

namespace ISO8583Server
{
    class Logon
    {
        private Iso8583Message _msg;
        public Logon(Iso8583Message msg)
        {
            this._msg = msg;
        }

        public void BuildResponse()
        {
            this._msg.SetResponseMessageTypeIdentifier();
            this._msg.Fields.Add(FieldID.Field39, "00");
        }

        public Iso8583Message GetMessage()
        {
            return this._msg;
        }
    }
}
