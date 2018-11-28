using System;
using System.Threading;
using System.IO;
using System.ComponentModel;

using Dione.Utilities;
using DioneData.Protobuf.VehicleCAN;
using DRAIVE.Link;
using DioneData.Protobuf.GNSS;
using Baselabs.Data.Coordinates;
using TUC.Data.HDMap;
using System.Collections.Generic;
using ServiceHDMapPedPathPredition;

namespace erviceHDMapPedPathPredition
{
    class GPS_SimpleReceiver : TrajectoryPredictor
    {
        //DRAIVE socket vars definition
        private DRAIVE.Link.ConfigNode _socketConfig;
        private DRAIVE.Link.Socket _socket;
        private DRAIVE.Link.Subscription _subscription;
        private string _producerPort;
        private string _subscriberPort;
        private bool _syncMode;

        //Defining GPS points
        public PointQ gpsCoordinate_previous;
        public PointQ gpsCoordinates;
        public List<PointQ> allEndPoints;
        //worker thread vars definition        
        BackgroundWorker _worker;
        private CancellationTokenSource _cancelSource;

        public GPS_SimpleReceiver(string configfilepath, string configName)
        {
            ConfigureNode(configfilepath, configName);
        }

        public void StartNode()
        {
            //Get input stream ID of Pedestrian GPS msg from config file      

            var WGS84StreamIDs = _socketConfig.getChild("WGS84StreamIDs").asUInt();

            //create subscription
            _subscription = _socket.subscribe(WGS84StreamIDs);

            //connect socket to bus
            //set msg receive time out
            _subscription.setReceiveMessageTimeout(100);
            _socket.connect(_producerPort, _subscriberPort);

            //init and start worker thread
            _worker = new BackgroundWorker();
            _worker.DoWork += DoSimpleReceiving;
            _cancelSource = new CancellationTokenSource();
            _worker.RunWorkerAsync(_cancelSource.Token);
        }

        private void ConfigureNode(string configfilepath, string configName)
        {
            try
            {
                if (_subscription == null)
                {

                    if (!File.Exists(configfilepath))
                        throw new AggregateException("Configuration filename does not exist!");

                    var configFile = new ConfigurationFile(configfilepath);
                    var confRoot = configFile.getRootNode();
                    _socketConfig = confRoot.getChild(configName);

                    var bus = _socketConfig.getString("Bus");
                    var busConf = confRoot.getChild(bus);

                    _producerPort = busConf.getString("BusProducer");
                    _subscriberPort = busConf.getString("BusConsumer");
                    _syncMode = busConf.getBoolean("SyncMode");

                    //create socket connection
                    _socket = new Socket(_syncMode ? Link.LINK_SOCKET_SYNC : Link.LINK_SOCKET_ASYNC);
                }
                else
                    throw new Exception("DraiveInterface already initialised.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error during node configuration! Details: " + ex);
            }

        }
        
        private void DoSimpleReceiving(object sender, DoWorkEventArgs e)
        {

            if (!(e.Argument is CancellationToken))
                throw new ArgumentException("e.Argument is CancellationToken");

            var cancelToken = (CancellationToken)e.Argument;

            //receiving loop as long as the subscrition is active or a chancelation is requested
            while (_subscription.isActive())
            {
                var msg = _subscription.receiveMessage();
                if (msg != null)
                {

                    //Receiving Pedestrian GPS msg using Player service
                    var Stream = msg.getStreamID();
                    var datatype = (msg.getDataTypeID());
                    if (datatype == 16002349372918614042) //Datatype with GNSS Datatype
                    {
                        var gnss = (DioneData.Protobuf.GNSS.GlobalPositionData)ProtobufMessageConverter.MessageToProtobuf<DioneData.Protobuf.GNSS.GlobalPositionData>(msg);
                       
                        //  var gnss = new DioneData.Protobuf.GNSS.GlobalPositionData();
                        var lat_gps = gnss.Wgs84Latitude;
                        var lo_gps  = gnss.Wgs84Longitude;
                        var height_gps = gnss.Wgs84Height;

                        var wg = new WGS84();
                        wg.Latitude  = lat_gps;
                        wg.Longitude = lo_gps;
                        wg.Height = height_gps;

                        //Covert WGS-84 to UTM coordinates
                        var utm = UTM.FromWGS84(wg, 33); 

                        var reader = new HDMapReader(utm, gpsCoordinate_previous);

                        //For storing previous coordinate HISTORIZATION
                        PointQ History_gps = new PointQ();
                        History_gps.X = utm.X;
                        History_gps.Y = utm.Y;
                        gpsCoordinate_previous = History_gps;

                        //gpsCoordinate_previous.Y = utm.Y;
                        //Console.WriteLine("time stamp: " + time.ToString() + "\t" + "vehicle velocity: " + vel);

                        //  Console.WriteLine("WGS-Stream: " + Stream.ToString() + "\t" + "Pedestrian Latitude: " + lat);
                        //  Console.WriteLine("WGS-Stream: " + Stream.ToString() + "\t" + "Pedestrian Longitude: " + lo);
                        //  Console.WriteLine("WGS-Stream: " + Stream.ToString() + "\t" + "Pedestrian Altitude: " + al);

                        Console.WriteLine("UTM Coordinates of GPS: " + utm);

                    }
                }
                if (cancelToken.IsCancellationRequested)
                    break;
            }
        }

        public void StopNode()
        {
            //disconnect socket
            _socket?.disconnect();
        }

    }
}




