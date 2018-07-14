using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player
{
    public string playerName { get; set; }
    public GameObject avatar;
    public Camera cam;
    public int connectionId;
    public static int Count = 0;
}

public class Client : MonoBehaviour {

    private const int MAX_CONNECTION = 1000;

    private int port = 5701;

    private int ourClientId;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private int connectionId;

    public GameObject playerPrefab;
    public static GameObject followpoint;

    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;

    private static bool created = false;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;
            Debug.Log("Awake: " + this.gameObject);
        }
    }

    public void OnClick()
    {
        string pName = GameObject.Find("InputField").GetComponent<InputField>().text;
        if (pName == "")
            return;

        playerName = pName;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);
        connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", port, 0, out error);

        isConnected = true;

        SceneManager.LoadScene("Demo");
    }

    private void Update()
    {
        if (!isConnected)
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
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);

                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "CNN":
                        SpawnPlayer(splitData[1], int.Parse(splitData[2]));
                        break;
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;
                    case "ASKPOSITION":
                        OnAskPosition(splitData);
                        break;
                }
                break;
        }
    }

    private void OnAskName(string[] data)
    {
        ourClientId = int.Parse(data[1]);

        Send("NAMEIS|" + playerName, reliableChannel);

        for(int i = 2; i < data.Length - 1; i++)
        {
            string[] d = data[i].Split('%');
            SpawnPlayer(d[0], int.Parse(d[1]));
        }
    }

    private void OnAskPosition(string[] data)
    {
        if (!isStarted)
            return;

        for(int i = 1; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');

            if (ourClientId == int.Parse(d[0]))
                break;

            Vector3 position = Vector3.zero;
            position.x = float.Parse(d[1]);
            position.y = float.Parse(d[2]);
            position.z = float.Parse(d[3]);

            players[int.Parse(d[0])].avatar.transform.position = position;
        }

        Vector3 myPosition = players[ourClientId].avatar.transform.position;
        string m = "MYPOSITION|" + myPosition.x.ToString() + '|' + myPosition.y.ToString() + '|' + myPosition.z.ToString();
        Send(m, unreliableChannel);
    }

    private void SpawnPlayer(string playerName, int cnnId)
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;

        if (cnnId == ourClientId)
        {
            go.AddComponent<CharControl>();
            isStarted = true;
        }

        Player p = new Player();


        p.avatar = go;
        p.avatar.transform.name = cnnId.ToString();
        p.playerName = playerName;
        p.connectionId += cnnId;
        p.cam = GameObject.Find(cnnId.ToString() + "/Camera").GetComponent<Camera>();

        Player.Count = cnnId;
        players.Add(cnnId, p);

        if (cnnId != ourClientId)
        {
            for (int i = 1; i < players.Count; i++)
            {
                players[i].cam.enabled = false;
            }
        }
    }

    private void PlayerDisconnected(int cnnId)
    {
        Destroy(players[cnnId].avatar);
        players.Remove(cnnId);
    }

    private void Send(string message, int channelId)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
    }
}