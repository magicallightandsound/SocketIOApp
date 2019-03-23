using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Prestige
{
    public class GameObjectFactory : MonoBehaviour
    {

        public static Dictionary<int, GameObject> remoteInstanceID2GameObject = new Dictionary<int, GameObject>();
        public static Dictionary<int, int> localInstanceID2SourceInstanceID = new Dictionary<int, int>();

        public static GameObject createFromPayload(List<string> payload) {

            Debug.Log("GameObject createFromPayload(List<string> payload");


            //  payload.action              0
            //  payload.resource_name       1
            //  payload.instance_id         2
            //  payload.source_instance_id  3
            //  payload.pos_x               4
            //  payload.pos_y               5
            //  payload.pos_z               6
            //  payload.rot_w               7
            //  payload.rot_x               8
            //  payload.rot_y               9
            //  payload.rot_z               10            

            GameObject go = UnityEngine.Object.Instantiate(Resources.Load(payload[1]),  // Instance name
                                                    new Vector3(float.Parse(payload[4]),
                                                                float.Parse(payload[5]),
                                                                float.Parse(payload[6])), 
                                                                new Quaternion(float.Parse(payload[7]),
                                                                               float.Parse(payload[8]),
                                                                               float.Parse(payload[9]),
                                                                               float.Parse(payload[10]))) as GameObject;

            int remoteInstanceID = Int32.Parse(payload[2]);
            remoteInstanceID2GameObject[remoteInstanceID] = go;

            int sourceInstanceID = Int32.Parse(payload[3]);
            localInstanceID2SourceInstanceID[go.GetInstanceID()] = sourceInstanceID;
            return go;
        }

        public static GameObject createFromRemoteInstanceID(int remoteInstanceID){

            Debug.Log("GameObject createFromRemoteInstanceID(int remoteInstanceID)");
            GameObject go = remoteInstanceID2GameObject[remoteInstanceID] as GameObject;
            return go;
        }

        public static GameObject fixedUpdateOfRemoteInstanceID(int remoteInstanceID, List<string> payload)
        {
            Debug.Log("GameObject fixedUpdateOfRemoteInstanceID(int remoteInstanceID, List<string> payload)");
            Vector3 position = new Vector3(float.Parse(payload[4]),
                                           float.Parse(payload[5]),
                                           float.Parse(payload[6]));
            Quaternion quaternion = new Quaternion(float.Parse(payload[7]),
                                                   float.Parse(payload[8]),
                                                   float.Parse(payload[9]),
                                                   float.Parse(payload[10]));

            GameObject go = remoteInstanceID2GameObject[remoteInstanceID] as GameObject;
            go.GetComponent<Rigidbody>().position = position;
            go.GetComponent<Rigidbody>().rotation = quaternion;
            return go;
        }

        public static void DestroyGameObjectWithRemoteInstanceID(int remoteInstanceID) {
            Destroy(remoteInstanceID2GameObject[remoteInstanceID] as GameObject);
        }
    }
}