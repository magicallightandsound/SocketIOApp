using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActsAsBarzoomable : MonoBehaviour
{


    private Vector3 curPos;
    private Vector3 lastPos;
    private Quaternion currentRotation;
    private Quaternion lastRotation;

    private float t = 0.0f;

    [SerializeField]
    public string resourceName = null;

    [SerializeField]
    public bool reflect = false;

    public bool reflectUpdate = true;
    public Vector3 upcomingPosition;
    public Quaternion upcomingRotation;

    private void Awake()
    {
        if (resourceName != null)
        {
            if (Main.main != null && reflect)
            {
               Main.main.SyncObject(resourceName, gameObject, true);
            }
            
        }
    }

    // Use this for initialization
    void Start()
    {
        curPos = GetComponent<Transform>().position;
        lastPos = curPos;

        currentRotation = GetComponent<Transform>().rotation;
        lastRotation = currentRotation;

        if (Main.main != null && reflect)
        {
            Main.main.SyncObject(resourceName, gameObject, false, true);
        }
    }

    // Update is called once per frame
    void Update()
    {

        t = t + Time.deltaTime;
        if (t > 0.016f)
        {
            /// Update position, rotation without reflecting
            if (reflectUpdate == false)
            {
                GetComponent<Transform>().position = upcomingPosition;
                GetComponent<Transform>().rotation = upcomingRotation;
            }

            if (resourceName != null)
            {
                curPos = GetComponent<Transform>().position;
                if (curPos != lastPos)
                {
                    lastPos = curPos;
                    if (Main.main != null && reflectUpdate)
                    {
                        Main.main.SyncObject(resourceName, gameObject, false, false, true);
                    }
                    
                }
                if (currentRotation != lastRotation)
                {

                    lastRotation = currentRotation;

                    if (Main.main != null && reflectUpdate)
                    {
                        Main.main.SyncObject(resourceName, gameObject, false, false, true);
                    }
                
                }
                if (!reflectUpdate)
                {
                    reflectUpdate = true;
                }
            }

            t = 0.0f;
        }


    }


    private void OnDestroy()
    {
        if (resourceName != null && reflect)
        {
            Main.main.SyncObject(resourceName, gameObject, false, false, false, true);
        }
    }
}