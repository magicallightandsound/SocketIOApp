using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Dpoch.SocketIO;
using System;
using Prestige;


public class Payload
{
    public string resource_name = null;
    public string pos_x = null;
    public string pos_y = null;
    public string pos_z = null;
    public string rot_w = null;
    public string rot_x = null;
    public string rot_y = null;
    public string rot_z = null;
    public string action = null;
    public string instance_id = null;
}

public class Main : MonoBehaviour {

    SocketIO socket = new SocketIO("ws://acpt-barzoom.herokuapp.com:80/socket.io/?EIO=4&transport=websocket");
    float timeSinceLastRequest = 0;
    string key = "yummy3";  // On one computer, set this to be 'yummy2' and on the other computer set this key to 'yummy3'
    string room = "myfunkyroom8";

    private GameObject sharedCube;

    [SerializeField]
    public GameObject sharedCubePrefab;

    List<string> memberNames = new List<string>();
    List<string> readyMemberNames = new List<string>();

    bool isLeader = false;

    Dictionary<string, string> payload = new Dictionary<string, string>();

    float timeOut = 2.0f;

    enum EngineState
    {
        LOGIN,
        JOIN,
        INIT,
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
    
        Vector3 position = go.GetComponent<Rigidbody>().transform.position;
        Quaternion rotation = go.GetComponent<Rigidbody>().transform.rotation;


        var payload = new Payload() {
            resource_name = resourceName,
            pos_x = position.x.ToString(),
            pos_y = position.y.ToString(),
            pos_z = position.z.ToString(),
            rot_w = rotation.w.ToString(),
            rot_x = rotation.x.ToString(),
            rot_y = rotation.y.ToString(),
            rot_z = rotation.z.ToString(),
            instance_id = go.GetInstanceID().ToString(),
            action = "0"
        };

        if (awake)
        {
            payload.action = "1"; //"awake";
         }
        else if (start)
        {
            payload.action = "2"; //"start";
       } else if (fixedupdate)
        {
            payload.action = "3"; //"fixedupdate";
        }
        else if (destroy)
        {
            var pl = new Payload()
            {
                resource_name = resourceName,
                instance_id = go.GetInstanceID().ToString(),
                action = "4"
            };

            socket.Emit("/data", key + "," + 
                        payload.action + "," + 
                        payload.resource_name + "," + 
                        payload.instance_id);
            return;
        }
        socket.Emit("/data", key + "," + 
                    payload.action + "," + 
                    payload.resource_name + "," + 
                    payload.instance_id + "," + 
                    payload.pos_x + "," + 
                    payload.pos_y + "," + 
                    payload.pos_z + "," +
                    payload.rot_w + "," + 
                    payload.rot_x + "," + 
                    payload.rot_y + "," + 
                    payload.rot_z );
    }

    // Use this for initialization
    void Start () {

        Debug.Log("Ready");

        socket.OnOpen += () => {
//
        };

        socket.On("/login_ok", (ev) => {
            Debug.Log("LOGIN_OK");

            engineState = EngineState.JOIN;
        });

        socket.On("/join_ok", (ev) => {
            Debug.Log("JOIN_OK");
            socket.Emit("/members", key);
            
        });


        socket.On("/info_ok", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log(myString);


        });

        socket.On("/warn_ok", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log(myString);


        });

        socket.On("/debug_ok", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log(myString);


        });

        socket.On("/pong", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            //Debug.Log("Received pong");


        });

        socket.On("/members_ok", (ev) => {
            Debug.Log("MEMBERS_OK");
            string myString = ev.Data[0].ToObject<string>();
            memberNames = new List<string>();
            memberNames.AddRange(JsonHelper.getJsonArray<string>(myString));
            Debug.Log("members =" + myString);

            if (engineState == EngineState.JOIN)
            {
                engineState = EngineState.INIT;
            }
        });

        socket.On("/whois_ready_ok", (ev) => {
            Debug.Log("WHOIS_READY_OK");
            string myString = ev.Data[0].ToObject<string>();
            readyMemberNames = new List<string>();
            readyMemberNames.AddRange(JsonHelper.getJsonArray<string>(myString));
            Debug.Log("ready =" + myString);

        });

        socket.On("/player_ready_ok", (ev) => {
            Debug.Log("PLAYER_READY_OK");
            //string myString = ev.Data[0].ToObject<string>();
            //readyMemberNames = JsonHelper.getJsonArray<string>(myString);
            //Debug.Log("ready =" + readyMemberNames);
            engineState = EngineState.WAITING;
        });

        socket.On("/start_experience_ok", (ev) => {
            Debug.Log("START_EXPERIENCE_OK");
            engineState = EngineState.START_EXPERIENCE;
        });



        socket.On("/data_ok", (ev) => {

            // Do not accept any incoming data until we are ready
            if (engineState != EngineState.SHARING_EXPERIENCE)
            {
                return;
            }

            string json = ev.Data[0].ToObject<string>();

            string[] paramsArray = json.Split(',');
            List<string> parameters = new List<string>(paramsArray.Length);

            int act = Int32.Parse(parameters[0]);
            //parametersstring resourceName = parameters[1];
            int remoteInstanceID = Int32.Parse(parameters[2]);

            switch (act)
            {
                case 1:
                    // Awake
                    GameObject awakeGo = Prestige.GameObjectFactory.createFromPayload(parameters);
                    break;
                case 2:
                    // Start

                    GameObject startGo = Prestige.GameObjectFactory.createFromRemoteInstanceID(remoteInstanceID);
                    startGo.SetActive(true);
                    break;
                case 3:
                    // Fixed Update
                    GameObject go = Prestige.GameObjectFactory.remoteInstanceID2GameObject[remoteInstanceID];
                    if (go != null)
                    {
                        Prestige.GameObjectFactory.fixedUpdateOfRemoteInstanceID(remoteInstanceID, parameters);
                    } else {

                        // We have rejoined a Shared Experience, we must initialize the GameObject before
                        // we can update its position/rotation

                        GameObject gogo = Prestige.GameObjectFactory.createFromPayload(parameters);
                        gogo.SetActive(true);

                    }
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
        if (timeSinceLastRequest > timeOut)
        {
            timeSinceLastRequest = 0f;

            socket.Emit("/ping", "hello");
            //Debug.Log("Sent ping");

            switch (engineState)
            {
                case EngineState.LOGIN:
                    {

                        Debug.Log("LOGIN");
                        socket.Emit("/login", key);

                    }
                    break;
                case EngineState.JOIN:
                    {
                        Debug.Log("JOIN");
                        socket.Emit("/join", key + ", " + room);

                    }
                    break;
                case EngineState.INIT: 
                    {
                        Debug.Log("INIT");


                        // Initialize, pre Experience

                        socket.Emit("/player_ready", key);

                    }
                    break;
                case EngineState.WAITING:
                    {
                        Debug.Log("WAITING");

                        socket.Emit("/members", key);
                        socket.Emit("/whois_ready", key);

                        if (!isLeader)
                        {
                            isLeader = (memberNames.Count == 1);
                        }

                        bool isEveryoneReady = (memberNames.Count == readyMemberNames.Count && memberNames.Count > 1);

                        if (isEveryoneReady)  
                        {

                            if (isLeader)
                            {
                                // Starting the MagicVerse
                                socket.Emit("/start_experience", key + ", " + "{}");
                            } else {


                                // Rejoining shared experience in progress
                                engineState = EngineState.SHARING_EXPERIENCE;
                            }



                        } 
                    }
                    break;
                case EngineState.START_EXPERIENCE:
                    {
                        Debug.Log("START_EXPERIENCE");
                        if (isLeader)  //isLeader
                        {
                            // Initialize the experience, as Leader
                            sharedCube = Instantiate(sharedCubePrefab);

                        } else {
                            // We're not the leader, however we will automatically
                            // build the scene, as we respond to incoming /data payloads
                        }


                        engineState = EngineState.SHARING_EXPERIENCE;
                    }
                    break;
                case EngineState.SHARING_EXPERIENCE:
                    {
                        Debug.Log("SHARING_EXPERIENCE");
                    }
                    break;
                case EngineState.END_EXPERIENCE:
                    {
                        Debug.Log("END_EXPERIENCE");
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