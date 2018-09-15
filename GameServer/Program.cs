using MySql.Data.MySqlClient;
using NLog;
using RoyT.AStar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TE2Common;
using TricksterMap;

namespace TrickEmu2
{
    class Program
    {
        private static Socket _serverSocket;
        public static readonly List<Socket> _clientSockets = new List<Socket>();
        private static readonly byte[] _buffer = new byte[2048];
        public static readonly Dictionary<string, User> _users = new Dictionary<string, User>();

        public static MySqlConnection _MySQLConn;
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static Configuration config = null;

        public static ushort EntityId = 0x2000;

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "TrickEmu Game (S2)";

            config = new Configuration();
               
            // MySQL
            _MySQLConn = new MySqlConnection("server=" + config.DB["Host"] + ";port=3306;database=" + config.DB["Database"] + ";uid=" + config.DB["Username"] + ";pwd=" + config.DB["Password"] + ";");
            try
            {
                _MySQLConn.Open();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to connect to MySQL.");
                Console.ReadKey();
                Environment.Exit(1); // Exit with error code 1 because error
            }

            logger.Info("Loading CharacterInfo...");
            Data.LoadCharacterInfo();
            logger.Info("Loaded {0} CharacterInfo entries.", Data.CharacterInfo.Count);

            logger.Info("Loading maps...");
            Data.LoadMaps();
            logger.Info("Loaded {0} maps.", Data.Maps.Count);

            logger.Info("Building maps...");
            Data.BuildMaps();
            logger.Info("Built {0} maps.", Data.Maps.Count);

            logger.Info("Starting server...");

            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(config.Server["GamePort"])));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                logger.Info("Server has been started on port {0}.", config.Server["GamePort"]);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to start the server.");
                Console.ReadKey();
                Environment.Exit(1); // Exit with error code 1 because error
            }
            while (true) Console.ReadLine();
        }

        private static void CloseAllSockets()
        {
            foreach (Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            _serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _clientSockets.Add(socket);

            var user = new User()
            {
                ClientSession = new SessionInfo(),
                ServerSession = new SessionInfo()
            };

            user.Socket = socket;
            user.ClientSession.Client = user;
            user.ServerSession.Client = user;

            _users[socket.RemoteEndPoint.ToString()] = user;

            socket.BeginReceive(_buffer, 0, 2048, SocketFlags.None, ReceiveCallback, socket);
            logger.Info("A client has been accepted from port {0}.", socket.RemoteEndPoint.ToString());
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                _users.Remove(current.RemoteEndPoint.ToString());
                logger.Warn("Client {0} forcefully disconnected.", current.RemoteEndPoint.ToString());
                _clientSockets.Remove(current);
                _users.Remove(current.RemoteEndPoint.ToString());
                try
                {
                    current.Close();
                }
                catch { }

                return;
            }

            try
            {
                if (!current.Connected)
                {
                    return;
                }
            }
            catch
            {
                try
                {
                    _users.Remove(current.RemoteEndPoint.ToString());
                    current.Close();
                    _clientSockets.Remove(current);
                    _users.Remove(current.RemoteEndPoint.ToString());
                }
                catch { }
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            if (received > 0)
            {
                try
                {
                    var user = _users[current.RemoteEndPoint.ToString()];
                    Packets._PacketReader.HandlePacket(user, recBuf);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to handle packet. Perhaps a malformed packet was sent?");
                }
            }
            else
            {
                return;
            }

            try
            {
                current.BeginReceive(_buffer, 0, 2048, SocketFlags.None, ReceiveCallback, current);
            }
            catch { }
        }
    }
}
