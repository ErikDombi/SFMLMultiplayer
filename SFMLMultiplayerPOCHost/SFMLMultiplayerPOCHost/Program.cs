using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFMLMultiplayerPOCHost
{
    class Program
    {
        static Dictionary<TcpClient, SocketConnection> Sockets = new Dictionary<TcpClient, SocketConnection>();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Listener");
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 6654);

            TcpClient client;
            tcpListener.Start();

            while (true) // Add your exit flag here
            {
                client = tcpListener.AcceptTcpClient();
                int i = Sockets.Count;
                Sockets.Add(client, new SocketConnection() { Index = i });
                Task.Run(() => ThreadProc(client, i));
            }
        }

        private static void ThreadProc(TcpClient obj, int Index)
        {
            using (var client = obj)
            {
                var SocketObject = Sockets[client];
                using (var stream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        PlayerData tmp = new PlayerData() { playerCount = Sockets.Values.ToList().Count, players = Sockets.Values.ToList() };
                        string tmpStr = JsonConvert.SerializeObject(tmp, Formatting.Indented);
                        Console.WriteLine(tmpStr);

                        try
                        {
                            byte[] readBuffer = new byte[1024];
                            stream.Read(readBuffer, 0, readBuffer.Length);
                            string bufferString = Encoding.ASCII.GetString(readBuffer);
                            var data = JsonConvert.DeserializeObject<RecieveJson>(bufferString);
                            if(SocketObject.x != data.x)
                                Console.WriteLine($"[{SocketObject.Index}] Moved by X: {SocketObject.x - data.x}");
                            if (SocketObject.y != data.y)
                                Console.WriteLine($"[{SocketObject.Index}] Moved by Y: {SocketObject.y - data.y}");
                            SocketObject.x = data.x;
                            SocketObject.y = data.y;
                            PlayerData pd = new PlayerData() { playerCount = Sockets.Values.Where(t => t.Index != SocketObject.Index).ToList().Count, players = Sockets.Values.Where(t => t.Index != SocketObject.Index).ToList() };
                            string writeBuffer = JsonConvert.SerializeObject(pd, Formatting.Indented).Length.ToString().PadLeft(5, '0') + JsonConvert.SerializeObject(pd, Formatting.Indented); ;
                            byte[] buffer = Encoding.ASCII.GetBytes(writeBuffer);
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        catch
                        {
                            client.Close();
                            Console.WriteLine("Client Disconnected. Client Index: " + SocketObject.Index);
                            Sockets.Remove(client);
                        }
                    }
                }   
            }
        }
    }
}
