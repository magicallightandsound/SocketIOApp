using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActsAsBarzoomable : MonoBehaviour
{


    private Vector3 curPos;
    private Vector3 lastPos;
    private float t = 0.0f;

    public static Dictionary<int, GameObject> gameObjectID2gameObject = new Dictionary<int, GameObject>();
    public static Dictionary<int, int> inGameID2gameObjectID = new Dictionary<int, int>();
    public static Dictionary<int, int> gameObjectID2inGameID = new Dictionary<int, int>();


    [SerializeField]
    public int inGameIdentifier;


    [SerializeField]
    public Main main;


    private void Awake()
    {
        if (inGameIdentifier > 0)
        {
            inGameID2gameObjectID[inGameIdentifier] = gameObject.GetInstanceID();
            gameObjectID2gameObject[gameObject.GetInstanceID()] = gameObject;
            gameObjectID2inGameID[gameObject.GetInstanceID()] = inGameIdentifier;

            main.SyncObject(inGameIdentifier, gameObject, true);
        }
    }

    // Use this for initialization
    void Start()
    {
        curPos = transform.GetComponent<Rigidbody>().position;
        lastPos = curPos;

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        t = t + Time.deltaTime;
        if (t > 1.0f)
        {
            if (inGameIdentifier > 0)
            {
                curPos = transform.GetComponent<Rigidbody>().position;
                if (curPos != lastPos)
                {
                    lastPos = curPos;
                    main.SyncObject(inGameIdentifier, gameObject, false, true);
                }
                
            }

            t = 0.0f;
        }


    }


    private void OnDestroy()
    {
        if (inGameIdentifier > 0)
        {
            int gameObjectID = inGameID2gameObjectID[inGameIdentifier];
            inGameID2gameObjectID.Remove(inGameIdentifier);
            gameObjectID2gameObject.Remove(gameObjectID);
            gameObjectID2inGameID.Remove(gameObjectID);

            main.SyncObject(inGameIdentifier, gameObject, false, false, true);
        }
    }
}