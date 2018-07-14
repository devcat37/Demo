using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class ServerClient
{
    public int connectionId;
    public string playerName;
    public Vector3 position;
}


public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 1000;

    private int port = 5701;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private List<ServerClient> clients = new List<ServerClient>();

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.05f;

    private void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                Debug.Log("User : " + connectionId + " has connected!");
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);

                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;

                    case "MYPOSITION":
                        OnMyPosition(connectionId, float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]));
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent:
                OnDisconnection(connectionId);
                break;

            case NetworkEventType.BroadcastEvent:

                break;
        }

        if(Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            string m = "ASKPOSITION|";
            foreach(ServerClient sc in clients)
            {
                m += sc.connectionId.ToString() + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString() + '%' + sc.position.z.ToString() + '|';
            }
            m = m.Trim('|');
            Send(m, unreliableChannel, clients);
        }
    }

    private void OnNameIs(int cnnId, string playerName)
    {
        clients.Find(x => x.connectionId == cnnId).playerName = playerName;

        Send("CNN|" + playerName + '|' + cnnId, reliableChannel, clients);


        Debug.Log(playerName);
    }

    private void OnConnection(int cnnId)
    {
        ServerClient c = new ServerClient();
        c.connectionId = cnnId;
        c.playerName = "N/A";

        clients.Add(c);

        string msg = "ASKNAME|" + cnnId + "|";
        foreach (ServerClient sc in clients)
            msg += sc.playerName + '%' + sc.connectionId + '|';

        msg = msg.Trim('|');

        Send(msg, reliableChannel, cnnId);
    }

    private void OnMyPosition(int cnnId, float x, float y, float z)
    {
        clients.Find(c => c.connectionId == cnnId).position = new Vector3(x, y, z);
    }

    private void OnDisconnection(int cnnId)
    {
        clients.Remove(clients.Find(x=> x.connectionId == cnnId));

        Send("DC|" + cnnId, reliableChannel, clients);
    }
    private void Send(string message, int channelId, int cnnId)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionId == cnnId));
        Send(message, channelId, c);


    }
    private void Send(string message, int channelId, List<ServerClient> c)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (ServerClient sc in c)
        {
            NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
        }
    }
}

