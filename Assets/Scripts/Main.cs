using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Dpoch.SocketIO;

public class Main : MonoBehaviour {

    SocketIO socket = new SocketIO("ws://acpt-barzoom.herokuapp.com:80/socket.io/?EIO=4&transport=websocket");
    float timeSinceLastRequest = 0;

	// Use this for initialization
	void Start () {

        Debug.Log("Ready");




        socket.OnOpen += () => {
            socket.Emit("/ping", "hello");
            Debug.Log("Sent ping");
        };

        socket.On("/pong", (ev) => {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log("Received pong");
            Debug.Log(myString); // hello

        });


        socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
        socket.OnClose += () => Debug.Log("Socket closed!");
        socket.OnError += (err) => Debug.Log("Socket Error: " + err);

        socket.Connect();
	}
	
	// Update is called once per frame
	void Update () {

        timeSinceLastRequest += Time.deltaTime;
        if (timeSinceLastRequest > 2)
        {
            timeSinceLastRequest = 0f;

            socket.Emit("/ping", "hello");
            Debug.Log("Sent ping");
        }
		
	}
}
