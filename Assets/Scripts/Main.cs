using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Dpoch.SocketIO;

public class Main : MonoBehaviour {

    SocketIO socket = new SocketIO("ws://acpt-barzoom.herokuapp.com:80/socket.io/?EIO=4&transport=websocket");
    float timeSinceLastRequest = 0;
    string key = "yummy2";
    bool joined = false;
    bool login = false;
    List<string> joined_members = new List<string>();
    List<string> ready_members = new List<string>();

    string[] memberNames = null;
    string[] readyMemberNames = null;

    Dictionary<int, int> inGameID2gameObjectIDMapper = new Dictionary<int, int>();
    Dictionary<int, GameObject> gameObjectID2GameObjectMapper = new Dictionary<int, GameObject>();
    Dictionary<int, int> gameObjectID2inGameIDMapper = new Dictionary<int, int>();

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
                gameObjectID2GameObjectMapper[go.GetInstanceID()] = gameObjects[i]; 
            }

            string newJson = "{ " +
                "\"inGameIDs\": " + JsonHelper.arrayToJson<int>(inGameIDs) + "," +
                "\"gameObjectIDs\": " + JsonHelper.arrayToJson<int>(inGameIDs) + "," +
            "}";

            
            socket.Emit("/persist", key + newJson);
        }
    }


    void syncGameObject(GameObject go)
    {
        if (engineState == EngineState.START_EXPERIENCE)
        {
            string json = JsonUtility.ToJson(go);
            socket.Emit("/data_gameobject", key + ", " + json);
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


        ///
        /// Advanced Data Reflection and Distribution
        ///


        socket.On("/persist_ok", (ev) => {

            // [ objectid, objectid, objectid]
            string myString = ev.Data[0].ToObject<string>();
            int[] objectIDs = JsonHelper.getJsonArray<int>(myString);
            Debug.Log("ready =" + readyMemberNames);

            for (int i = 0; i < objectIDs.Length; i++)
            {
                GameObject go = gameObjectID2GameObjectMapper[objectIDs[i]];
                syncGameObject(go);
            }
        });

        socket.On("/data_gameobject_ok", (ev) => {
            string json = ev.Data[0].ToObject<string>();

            GameObject go = JsonUtility.FromJson<GameObject>(json);

  
        });


        socket.On("/server_requests_persistence", (ev) => {
            string json = ev.Data[0].ToObject<string>();

            // 	{
            //		"inGameIDs" : [ 287, 565, 436],
            //		"gameObjectIDs" : [234, 236, 543]
            //  }

            Dictionary<string,int[]> payload = JsonUtility.FromJson<Dictionary<string, int[]>>(json);

            int[] inGameIDs = payload["inGameIDs"];
            int[] gameObjectIDs = payload["gameObjectIDs"];

            for (int i = 0; i < gameObjectIDs.Length; i++)
            {
                int gameObjectID = gameObjectIDs[i];
                int inGameID = inGameIDs[i];

                gameObjectID2inGameIDMapper[gameObjectID] = inGameID;
            }
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