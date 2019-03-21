using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Prestige
{
    public class GameObjectFactory : MonoBehaviour
    {

        public static Dictionary<int, GameObject> remoteInstanceID2GameObject = new Dictionary<int, GameObject>();

        public static GameObject createFromPayload(List<string> payload) {

              
            //  payload.action              0
            //  payload.resource_name       1
            //  payload.instance_id         2
            //  payload.pos_x               3
            //  payload.pos_y               4
            //  payload.pos_z               5
            //  payload.rot_w               6
            //  payload.rot_x               7
            //  payload.rot_y               8
            //  payload.rot_z               9            

            GameObject go = UnityEngine.Object.Instantiate(Resources.Load(payload[1]),  // Instance name
                                                    new Vector3(float.Parse(payload[3]),
                                                                float.Parse(payload[4]),
                                                                float.Parse(payload[5])), 
                                                                new Quaternion(float.Parse(payload[6]),
                                                                               float.Parse(payload[7]),
                                                                               float.Parse(payload[8]),
                                                                               float.Parse(payload[9]))) as GameObject;

            int remoteInstanceID = Int32.Parse(payload[2]);
            remoteInstanceID2GameObject[remoteInstanceID] = go;
            return go;
        }

        public static GameObject createFromRemoteInstanceID(int remoteInstanceID){
            GameObject go = remoteInstanceID2GameObject[remoteInstanceID] as GameObject;
            return go;
        }

        public static GameObject fixedUpdateOfRemoteInstanceID(int remoteInstanceID, List<string> payload)
        {
            Vector3 position = new Vector3(float.Parse(payload[3]),
                                           float.Parse(payload[4]),
                                           float.Parse(payload[5]));
            Quaternion quaternion = new Quaternion(float.Parse(payload[6]),
                                                   float.Parse(payload[7]),
                                                   float.Parse(payload[8]),
                                                   float.Parse(payload[9]));

            GameObject go = remoteInstanceID2GameObject[remoteInstanceID] as GameObject;
            go.GetComponent<Transform>().position = position;
            go.GetComponent<Transform>().rotation = quaternion;
            return go;
        }

        public static void DestroyGameObjectWithRemoteInstanceID(int remoteInstanceID) {
            Destroy(remoteInstanceID2GameObject[remoteInstanceID] as GameObject);
        }
    }
}