using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Dpoch.SocketIO;
using System;
using Prestige;

public class Main : MonoBehaviour {

    SocketIO socket = new SocketIO("ws://acpt-barzoom.herokuapp.com:80/socket.io/?EIO=4&transport=websocket");
    float timeSinceLastRequest = 0;
    string key = "yummy2";

    List<string> joined_members = new List<string>();
    List<string> ready_members = new List<string>();

    string[] memberNames = null;
    string[] readyMemberNames = null;


    Dictionary<string, string> payload = new Dictionary<string, string>();

    enum EngineState
    {
        LOGIN,
        JOIN,
        WAITING,
        VISITOR,
        START_EXPERIENCE,
        SHARING_EXPERIENCE,
        END_EXPERIENCE
    }


    EngineState engineState = EngineState.LOGIN;
    
    public static Main main = null;

    private void Awake()
    {
        Main.main = this;
    }
    public void SyncObject(string resourceName, GameObject go, bool awake = false, bool start = false, bool fixedupdate = false, bool destroy = false)
    {
    
        payload["resource_name"] = resourceName;
        Vector3 position = go.GetComponent<Rigidbody>().transform.position;
        Quaternion rotation = go.GetComponent<Rigidbody>().transform.rotation;

        payload["pos-x"] = position.x.ToString();
        payload["pos-y"] = position.y.ToString();
        payload["pos-z"] = position.z.ToString();

        payload["rot-w"] = rotation.w.ToString();
        payload["rot-x"] = rotation.x.ToString();
        payload["rot-y"] = rotation.z.ToString();
        payload["rot-z"] = rotation.z.ToString();

        if (awake)
        {
            payload["action"] = 1.ToString(); //"awake";
            socket.Emit("/data", payload);
        }
        else if (start)
        {
            payload["action"] = 2.ToString(); //"start";
            socket.Emit("/data", payload);
        } else if (fixedupdate)
        {
            payload["action"] = 3.ToString(); //"fixedupdate";
            socket.Emit("/data", payload);
        }
        else if (destroy)
        {
            payload.Clear();    
            payload["resource_name"] = resourceName;
  
            payload["instanceID"] = go.GetInstanceID().ToString();

            payload["action"] = 4.ToString(); //"destroy";
            socket.Emit("/data", payload);
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
            engineState = EngineState.LOGIN;
        });

        socket.On("/join_ok", (ev) => {
            engineState = EngineState.JOIN;
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


        socket.On("/data_ok", (ev) => {

            // Do not accept any incoming data until we are ready
            if (engineState != EngineState.SHARING_EXPERIENCE)
            {
                return;
            }

            string json = ev.Data[0].ToObject<string>();

            Dictionary<string, string> payload = JsonUtility.FromJson<Dictionary<string,string> >(json);


            int remoteInstanceID = Int32.Parse(payload["instanceID"]);

            string action = payload["action"];
            int act = Int32.Parse(action);


            switch (act)
            {
                case 1:
                    // Awake
                    GameObject awakeGo = Prestige.GameObjectFactory.createFromPayload(payload);
                    break;
                case 2:
                    // Start

                    GameObject startGo = Prestige.GameObjectFactory.createFromRemoteInstanceID(remoteInstanceID);
                    startGo.SetActive(true);
                    break;
                case 3:
                    // Fixed Update
                    GameObject go = Prestige.GameObjectFactory.createFromPayload(payload);
                    break;
                case 4:
                    // Destroy
                 
                    Prestige.GameObjectFactory.DestroyGameObjectWithRemoteInstanceID(remoteInstanceID);
                    break;
                default:
                    break;
            }

        });




        socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
        socket.OnClose += () => Debug.Log("Socket closed!");
        socket.OnError += (err) => Debug.Log("Socket Error: " + err);

        socket.Connect();
	}

 
    // Update is called once per frame
    void Update () {

        // Every .33 seconds
        timeSinceLastRequest += Time.deltaTime;
        if (timeSinceLastRequest > 0.33f)
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


                        if (isEveryoneReady)
                        {
                            bool isLeader = (joined_members.Count == 1);
                            if (isLeader)
                            {
                                // Starting the MagicVerse
                                socket.Emit("/start_experience", key);
                            } else {
                                // Rejoining shared experience in progress
                            }

                            engineState = EngineState.START_EXPERIENCE;

                        } 
                    }
                    break;
                case EngineState.START_EXPERIENCE:
                    {
                        bool isLeader = (joined_members.Count == 1);
                        if (isLeader)
                        {
                            // Initialize as Leader
                        } else {
                            // Initialize as not the Leader
                        }
                        engineState = EngineState.SHARING_EXPERIENCE;
                    }
                    break;
                case EngineState.SHARING_EXPERIENCE:
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