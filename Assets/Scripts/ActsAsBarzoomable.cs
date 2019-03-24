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
    private bool allowReflectionUpdate = true;
    private Vector3 scheduledNewPosition;
    private Quaternion scheduledNewRotation;

    [SerializeField]
    public string resourceName = null;

    [SerializeField]
    public bool allowInternalStateReflection = false;

    public void scheduleReflectionUpdate(Vector3 position, Quaternion rotation)
    {
        scheduledNewPosition = position;
        scheduledNewRotation = rotation;
        allowReflectionUpdate = false;
    }
    private void Awake()
    {
        if (resourceName != null)
        {
            if (OrcusClient.client != null && allowInternalStateReflection)
            {
               OrcusClient.client.SyncGameObject(resourceName, gameObject, true);
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

        if (OrcusClient.client != null && allowInternalStateReflection)
        {
            OrcusClient.client.SyncGameObject(resourceName, gameObject, false, true);
        }
    }

    // Update is called once per frame
    void Update()
    {

        t = t + Time.deltaTime;
        if (t > 0.016f)
        {
            /// Update position, rotation without reflecting
            if (allowReflectionUpdate == false)
            {
                GetComponent<Transform>().position = scheduledNewPosition;
                GetComponent<Transform>().rotation = scheduledNewRotation;
            }

            if (resourceName != null)
            {
                curPos = GetComponent<Transform>().position;
                if (curPos != lastPos)
                {
                    lastPos = curPos;
                    if (OrcusClient.client != null && allowReflectionUpdate)
                    {
                        OrcusClient.client.SyncGameObject(resourceName, gameObject, false, false, true);
                    }
                    
                }
                if (currentRotation != lastRotation)
                {

                    lastRotation = currentRotation;

                    if (OrcusClient.client != null && allowReflectionUpdate)
                    {
                        OrcusClient.client.SyncGameObject(resourceName, gameObject, false, false, true);
                    }
                
                }
                if (!allowReflectionUpdate)
                {
                    allowReflectionUpdate = true;
                }
            }

            t = 0.0f;
        }


    }


    private void OnDestroy()
    {
        if (resourceName != null && allowInternalStateReflection)
        {
            OrcusClient.client.SyncGameObject(resourceName, gameObject, false, false, false, true);
        }
    }
}