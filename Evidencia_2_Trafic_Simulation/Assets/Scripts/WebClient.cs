using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct CarInfo
{
    public int id;
    public float[] pos;
    public string name;
}
[Serializable]
public struct TrafficLights
{
    public float[] pos;
    public string status;
}
[Serializable]
struct Step
{
    public CarInfo[] cars;
    public TrafficLights[] tf;
}

public class WebClient : MonoBehaviour
{
    public static WebClient instance;
    public GameObject[] carPrefabs;
    public GameObject ambPrefab;
    GameObject[] carsInstances;
    GameObject[] tfInstances;

    Step step;
    int[] ids;
    int[] cached_ids;
    int[] car_types;
    int[] cached_car_types;
    bool firstStep = true;
    bool received = false;
    float chrono = 0.0f;

    Vector3[] moveVects;
    float[] cached_rot_angles;
    float[] rot_angles;
    public float scale = 2.0f;
    public float fps;

    int index_by_id(int id)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == id)
                return i;
        }
        return -1;
    }

    float rotation(Vector3 move)
    {
        float res = 90.0f;

        if (move.x > 0) // Moves Right
        {
            if (move.z > 0)
            {
                res += 315.0f;
            }
            else if (move.z < 0)
            {
                res += 45.0f;
            }            
        }
        else if (move.x < 0) // Moves Left
        {
            res += 180.0f;
            if (move.z > 0)
            {
                res += 45.0f;
            }
            else if (move.z < 0)
            {
                res += 315.0f;
            }
        }
        else // Moves Vertically
        {
            if (move.z > 0)
            {
                res += 270.0f;
            }
            else if (move.z < 0)
            {
                res += 90.0f;
            }
        }

        return res;
    }

    // IEnumerator - yield return
    IEnumerator SendData()
    {
        WWWForm form = new WWWForm();
        form.AddField("bundle", "the data");
        //string url = "http://localhost:8585";
        string url = "http://localhost:8000/";

        //string url = "https://tec-server.mybluemix.net/api/points";
        //string url = "https://tec-server-demo.mybluemix.net/api/points";
        //using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();          // Talk to Python
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                step = JsonUtility.FromJson<Step>(www.downloadHandler.text);
                bool remain_still = true;
                // Debug.Log(step.cars.Length);
                
                if (!firstStep)
                {
                    moveVects = new Vector3[carsInstances.Length];
                    rot_angles = new float[carsInstances.Length];
                    for (int i = 0; i < carsInstances.Length; i++)
                    {
                        // Match id with car if it exists
                        int index = -1;
                        for (int j = 0; j < step.cars.Length; j++)
                        {
                            if (step.cars[j].id == ids[i])
                                index = j;
                        }
                        // If match exists then measure displacement
                        if (index > -1)
                        {
                            Vector3 goal = new Vector3(step.cars[index].pos[0], 0.5f, step.cars[index].pos[1]);
                            goal = goal * scale;
                            moveVects[i] = goal - carsInstances[i].transform.position;
                        }
                        else
                        {
                            moveVects[i] = Vector3.zero;
                        }
                        // Check if model freezes
                        if (moveVects[i].magnitude > 0)
                        {
                            remain_still = false;
                        }
                        // Assign new rotations
                        if (moveVects[i].magnitude == 0)
                        {
                            try
                            {
                                for (int j = 0; j < cached_ids.Length; j++)
                                {
                                    if (cached_ids[j] == ids[i])
                                    {
                                        rot_angles[i] = cached_rot_angles[j];
                                        carsInstances[i].transform.rotation = new Quaternion(0,0,0,1);
                                        carsInstances[i].transform.Rotate(new Vector3(0,cached_rot_angles[j],0), Space.World);
                                    }
                                }
                            }
                            catch {}
                        }
                        else
                        {
                            rot_angles[i] = rotation(moveVects[i]);
                            carsInstances[i].transform.rotation = new Quaternion(0,0,0,1);
                            carsInstances[i].transform.Rotate(new Vector3(0,rot_angles[i],0), Space.World);
                        }
                        // Debug.Log(carsInstances[i].name + " rot " + rot_angles[i]);
                    }
                }
                if (remain_still && !firstStep)
                {
                    #if UNITY_EDITOR
                    StartCoroutine(SendData());
                    #endif
                }
                else
                {
                    received = true;
                }
                Debug.Log("<-------RESPONSE------->");
            }
        }
    }

    public void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    async void Start()
    {
        carsInstances = new GameObject[0];
        rot_angles = new float[0];
        #if UNITY_EDITOR
        StartCoroutine(SendData());
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (received)
        {
            // Assign positions and get movement vectors
            if (firstStep || chrono > 1 / fps)
            {
                // for (int i = 0; i < step.tf.Length; i++)
                // {
                //     Debug.Log("Semaforo" + i + "- Status" + step.tf[i].status);
                // }
                received = false;
                chrono = 0.0f;
                cached_ids = new int[carsInstances.Length];
                cached_car_types = new int[carsInstances.Length];
                cached_rot_angles = (float[])rot_angles.Clone();
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    cached_ids[i] = int.Parse(carsInstances[i].name);
                    cached_car_types[i] = car_types[i];
                    // Debug.Log("ID:" + cached_ids[i] + "| Rotation" + cached_rot_angles[i]);
                    Destroy(carsInstances[i], 0.0f);
                }
                carsInstances = new GameObject[step.cars.Length];
                car_types = new int[step.cars.Length];
                ids = new int[step.cars.Length];
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]) * scale;

                    // Assigning rotation
                    if (step.cars[i].name == "Car")
                    {
                        int car_type = (int)UnityEngine.Random.Range(0, carPrefabs.Length - 0.1f);
                        for (int j = 0; j < cached_ids.Length; j++)
                        {
                            if (cached_ids[j] == step.cars[i].id)
                            {
                                car_type = cached_car_types[j];
                            }
                        }
                        car_types[i] = car_type;
                        Debug.Log(car_type);
                        carsInstances[i] = GameObject.Instantiate(carPrefabs[car_type], pos, new Quaternion(0,0,0,1));
                    }
                    else
                    {
                        carsInstances[i] = GameObject.Instantiate(ambPrefab, pos, new Quaternion(0,0,0,1));
                    }

                    try
                    {
                        int index = 0;
                        foreach (int id in cached_ids)
                        {
                            if (id == step.cars[i].id)
                                break;
                            index++;
                        }
                        carsInstances[i].transform.Rotate(0,rot_angles[index],0,Space.World);
                    }
                    catch {}

                    carsInstances[i].name = step.cars[i].id.ToString();
                    ids[i] = step.cars[i].id;
                }
                firstStep = false;
                #if UNITY_EDITOR
                StartCoroutine(SendData());
                #endif
            }
            // Move
            else
            {
                chrono += Time.deltaTime;
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    carsInstances[i].transform.position += moveVects[i] * Time.deltaTime * fps;
                }
            }
        }
    }
}
