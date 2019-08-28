using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trx.Messaging.Iso8583;

namespace ISO8583Server
{
    class Sale 
    {
        private Iso8583Message _msg;
        public Sale(Iso8583Message msg)
        {
            this._msg = msg;
        }

        private void BuildResponse()
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
