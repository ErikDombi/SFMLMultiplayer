using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFMLMultiplayerPOCHost
{
    public class PlayerData {
        public List<SocketConnection> players;
        public int playerCount;
    }

    public class SocketConnection
    {
        public int Index;
        public double x = 0.000000;
        public double y = 0.000000;
    }

    public class RecieveJson
    {
        public double x;
        public double y;
    }
}
