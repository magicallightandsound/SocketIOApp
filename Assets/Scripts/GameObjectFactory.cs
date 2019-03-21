using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Prestige
{
    public class GameObjectFactory : UnityEngine.Object
    {
        public static Dictionary<int, GameObject> remoteInstanceID2GameObject = new Dictionary<int, GameObject>();

        public static GameObject createFromPayload(Dictionary<string, string> payload) {

            GameObject go = UnityEngine.Object.Instantiate(Resources.Load(payload["resource_name"]),
                                                    new Vector3(float.Parse(payload["pos_x"]),
                                                                float.Parse(payload["pos_y"]),
                                                                float.Parse(payload["pos_z"])), 
                                                                new Quaternion(float.Parse(payload["pos_w"]),
                                                                               float.Parse(payload["pos_x"]),
                                                                               float.Parse(payload["pos_y"]),
                                                                               float.Parse(payload["pos_z"]))) as GameObject;

            int remoteInstanceID = Int32.Parse(payload["instance_id"]);
            remoteInstanceID2GameObject[remoteInstanceID] = go;
            return go;
        }

        public static GameObject createFromRemoteInstanceID(int remoteInstanceID){
            GameObject go = remoteInstanceID2GameObject[remoteInstanceID] as GameObject;
            return go;
        }

        public static GameObject fixedUpdateOfRemoteInstanceID(int remoteInstanceID, Dictionary<string, string> payload)
        {
            Vector3 position = new Vector3(float.Parse(payload["pos_x"]),
                                           float.Parse(payload["pos_y"]),
                                           float.Parse(payload["pos_z"]));
            Quaternion quaternion = new Quaternion(float.Parse(payload["pos_w"]),
                                                   float.Parse(payload["pos_x"]),
                                                   float.Parse(payload["pos_y"]),
                                                   float.Parse(payload["pos_z"]));

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