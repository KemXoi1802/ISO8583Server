using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Sinks;
using Trx.Communication.Channels.Sinks.Framing;
using Trx.Communication.Channels.Tcp;
using Trx.Coordination.TupleSpace;
using Trx.Logging;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace ISO8583Server
{
    class Program
    {
        private const int Field39ResponseCode = 39;

        private int _requestsCnt;
        private bool _stop;

        /// <summary>
        /// Returns the number of requests made.
        /// </summary>
        public int RequestsCount
        {
            get { return _requestsCnt; }
        }

        public void Stop()
        {
            _stop = true;
        }

        private void Receiver(object state)
        {
            var pipeline = new Pipeline();
            pipeline.Push(new NboFrameLengthSink(2) { IncludeHeaderLength = false, MaxFrameLength = 1024 });
            pipeline.Push(
                new MessageFormatterSink(new Iso8583MessageFormatter((@"..\Formatters\Iso8583Bin1987.xml"))));
            var ts = new TupleSpace<ReceiveDescriptor>();

            var server = new TcpServerChannel(new Pipeline(), new ClonePipelineFactory(pipeline), ts,
                new FieldsMessagesIdentifier(new[] { 11, 41 }))
            {
                Port = 8583,
                LocalInterface = (string)state,
                Name = "ISO8583Server"
            };

            server.StartListening();

            while (!_stop)
            {
                ReceiveDescriptor rcvDesc = ts.Take(null, 100);
                if (rcvDesc == null)
                    continue;
                _requestsCnt++;
                var message = rcvDesc.ReceivedMessage as Iso8583Message;
                if (message == null)
                    continue;

                Iso8583Message response;
                if (message.IsAuthorization())
                {
                    Console.WriteLine("go to handle sale msg");
                    Sale auth = new Sale(message);
                    response = auth.GetMessage();
                }
                else if (message.IsNetworkManagement())
                {
                    Console.WriteLine("go to handle logon msg");
                    Logon logon = new Logon(message);
                    logon.BuildResponse();
                    response = logon.GetMessage();
                    Console.WriteLine(response.ToString());
                }
                else
                {
                    response = null;
                }

                //message.SetResponseMessageTypeIdentifier();
                //message.Fields.Add(Field39ResponseCode, "00");
                var addr = rcvDesc.ChannelAddress as ReferenceChannelAddress;
                if (addr == null)
                    continue;
                var child = addr.Channel as ISenderChannel;
                if (child != null)
                    child.Send(response);
            }

            // Stop listening and shutdown the connection with the sender.
            server.StopListening();
        }

        static void Main(string[] args)
        {
            LogManager.LoggerFactory = new Log4NetLoggerFactory();
            LogManager.Renderer = new Renderer();
            ILogger logger = LogManager.GetLogger("root");

            string localInterface = "localhost";
            if (args.Length > 0)
                localInterface = args[0];
            var a = new Program();

            var receiver = new Thread(a.Receiver);
            receiver.Start(localInterface);

            logger.Info("Acquirer is running, press any key to stop it...");
            Console.ReadLine();
            a.Stop();
            logger.Info(string.Format("Processed requests: {0}", a.RequestsCount));
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
