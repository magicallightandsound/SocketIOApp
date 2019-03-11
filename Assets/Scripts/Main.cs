using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Dpoch.SocketIO;

public class Main : MonoBehaviour {


    //public delegate void Moving(GameObject gameObject);    //public static event CursorStopHover OnCursorStopHover;

    SocketIO socket = new SocketIO("ws://acpt-barzoom.herokuapp.com:80/socket.io/?EIO=4&transport=websocket");
    float timeSinceLastRequest = 0;
    string key = "yummy2";
    bool joined = false;
    bool login = false;
    List<string> joined_members = new List<string>();
    List<string> ready_members = new List<string>();
    bool start_experience = false;
    bool end_experience = false;
    bool shared_experience_leader = false;
    bool shared_experience_follower = false;
    string[] memberNames = null;
    string[] readyMemberNames = null;

    // GameObject to be synced
    GameObject gameObjectToSync = null;


    Dictionary<int, GameObject> idToGameObjectMapper = new Dictionary<int, GameObject>();

    enum EngineState
    {
        LOGIN,
        JOIN,
        WAITING,
        VISITOR,
        START_EXPERIENCE,
        END_EXPERIENCE
    }

    EngineState engineState = EngineState.LOGIN;


    /// Reflect an array gameobject to all players in the Room
    /// 
    /// The game objects' identifier and in-game identifier are persisted on the
    /// lobby server, so that all the other players know which object to move
    /// 
    void syncGameObjectIDs(int[] inGameIDs, GameObject[] gameObjects)
    {
        if (engineState == EngineState.START_EXPERIENCE)
        {
            // Map the game object instance id to the game object
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject go = gameObjects[i] as GameObject;
                idToGameObjectMapper[go.GetInstanceID()] = gameObjects[i]; 
            }

            string newJson = "{ " +
                "\"inGameIDs\": " + JsonHelper.arrayToJson<int>(inGameIDs) + "," +
                "\"gameObjectIDs\": " + JsonHelper.arrayToJson<int>(inGameIDs) + "," +
            "}";

            // Sending the game object is done in the /persist_ok event
            socket.Emit("/persist", key + newJson);
        }
    }


    void syncGameObject(GameObject go)
    {
        if (engineState == EngineState.START_EXPERIENCE)
        {
            string json = JsonUtility.ToJson(go);
            socket.Emit("/Data", key + ", " + json);
        }
    }


    // Use this for initialization
    void Start () {

        Debug.Log("Ready");

        socket.OnOpen += () => {
            socket.Emit("/login", key);
        };

        socket.On("/login_ok", (ev) => {
            socket.Emit("/join", key + "," + "police2");
            login = true;
        });

        socket.On("/join_ok", (ev) => {
            joined = true;
        });

        socket.On("/pong", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log("Received pong");


        });

        socket.On("/members_ok", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
 
            memberNames = JsonHelper.getJsonArray<string>(myString);
            Debug.Log("ready =" + memberNames);
        });

        socket.On("/whois_ready", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            readyMemberNames = JsonHelper.getJsonArray<string>(myString);
            Debug.Log("ready =" + readyMemberNames);

        });

        socket.On("/persist_ok", (ev) => {

            // [ objectid, objectid, objectid]
            string myString = ev.Data[0].ToObject<string>();
            int[] objectIDs = JsonHelper.getJsonArray<int>(myString);
            Debug.Log("ready =" + readyMemberNames);

            for (int i = 0; i < objectIDs.Length; i++)
            {
                GameObject go = idToGameObjectMapper[objectIDs[i]];
                syncGameObject(go);
            }
        });

        socket.On("/data_ok", (ev) => {
            string myString = ev.Data[0].ToObject<string>();

            GameObject go = JsonUtility.FromJson<GameObject>(myString);

  
        });

        socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
        socket.OnClose += () => Debug.Log("Socket closed!");
        socket.OnError += (err) => Debug.Log("Socket Error: " + err);

        socket.Connect();
	}
	
	// Update is called once per frame
	void Update () {

        // Every two seconds
        timeSinceLastRequest += Time.deltaTime;
        if (timeSinceLastRequest > 2)
        {
            timeSinceLastRequest = 0f;

            socket.Emit("/ping", "hello");
            Debug.Log("Sent ping");

            string key = "yummy2";
            switch (engineState)
            {
                case EngineState.LOGIN:
                    {
                        Debug.Log("LOGIN");
                        socket.Emit("/login", key);
                        engineState = EngineState.JOIN;
                    }
                    break;
                case EngineState.JOIN:
                    {
                        Debug.Log("JOIN");
                        socket.Emit("/join", key + ", myfunkyroom");
                        engineState = EngineState.WAITING;
                    }
                    break;
                case EngineState.WAITING:
                    {
                        Debug.Log("WAITING");
                        socket.Emit("/player_ready", key);
                        socket.Emit("/members", key);
                        socket.Emit("/whois_ready", "key");

                        bool isEveryoneReady = (joined_members.Count == this.ready_members.Count);
                        bool isLeader = (joined_members.Count == 1);

                        if (isEveryoneReady && isLeader)
                        {
                            socket.Emit("/start_experience", key);
                        }
                    }
                    break;
                case EngineState.START_EXPERIENCE:
                    {
                         
                    }
                    break;
                case EngineState.END_EXPERIENCE:
                    {

                    }
                    break;
                default:
                    break;
            }            
        }
	}
}


public class JsonHelper
{
    //Usage:
    //YouObject[] objects = JsonHelper.getJsonArray<YouObject> (jsonString);
    public static T[] getJsonArray<T>(string json)
    {

        // Incoming strings are escaped, we must escape them
        Regex regex = new Regex(@"");
        string result = Regex.Unescape(json);
        string cookedJson = result.Trim('"');

        string newJson = "{ \"array\": " + cookedJson + "}";
        

        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    //Usage:
    //string jsonString = JsonHelper.arrayToJson<YouObject>(objects);
    public static string arrayToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.array = array;
        return JsonUtility.ToJson(wrapper);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}