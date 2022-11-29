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
    public bool crashed;
}
[Serializable]
public struct TrafficLights
{
    public float[] pos;
    public int status;
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
    GameObject[][] STLs;
    public GameObject[] STL1;
    public GameObject[] STL2;
    public GameObject[] STL3;
    public GameObject[] STL4;

    GameObject[] carsInstances;
    GameObject[] cachedCarsInstances;
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

    bool carRemains(GameObject car)
    {
        for (int i = 0; i < step.cars.Length; i++)
        {
            if (step.cars[i].id == int.Parse(car.name))
            {
                return true;
            }
        }
        return false;
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
                // Debug.Log(www.downloadHandler.text);
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
                        if (moveVects[i].magnitude != 0)
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
        STLs = new GameObject[][] {STL1, STL2, STL3, STL4};
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
                for (int i = 0; i < 4; i++)
                {
                    foreach (GameObject STL in STLs[i])
                    {
                        STL.SetActive(false);
                    }
                    if (step.tf[i * 3].status == 1)
                    {
                        STLs[i][2].SetActive(true);
                    }
                    else if (step.tf[i * 3].status == 2)
                    {
                        STLs[i][1].SetActive(true);
                    }
                    else if (step.tf[i * 3].status == 3)
                    {
                        STLs[i][0].SetActive(true);
                    }
                }
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
                    if (!carRemains(carsInstances[i]))
                    {
                        Destroy(carsInstances[i], 0.0f);
                    }
                }
                cachedCarsInstances = (GameObject[])carsInstances.Clone();
                carsInstances = new GameObject[step.cars.Length];
                car_types = new int[step.cars.Length];
                ids = new int[step.cars.Length];
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]) * scale;

                    // Assigning rotation
                    if (step.cars[i].name == "Car")
                    {
                        bool remained = false;
                        for (int j = 0; j < cachedCarsInstances.Length; j++)
                        {
                            if (int.Parse(cachedCarsInstances[j].name) == step.cars[i].id)
                            {
                                remained = true;
                                carsInstances[i] = cachedCarsInstances[j];
                                // carsInstances[i].transform.SetPositionAndRotation(pos,new Quaternion(0,0,0,1));
                                carsInstances[i].transform.position = pos;
                                break;
                            }
                        }
                        if (!remained)
                        {
                            int car_type = (int)UnityEngine.Random.Range(0, carPrefabs.Length - 0.1f);
                            carsInstances[i] = GameObject.Instantiate(carPrefabs[car_type], pos, new Quaternion(0,0,0,1));
                        }
                    }
                    else
                    {
                        bool remained = false;
                        for (int j = 0; j < cachedCarsInstances.Length; j++)
                        {
                            if (int.Parse(cachedCarsInstances[j].name) == step.cars[i].id)
                            {
                                remained = true;
                                carsInstances[i] = cachedCarsInstances[j];
                                // carsInstances[i].transform.SetPositionAndRotation(pos,new Quaternion(0,0,0,1));
                                carsInstances[i].transform.position = pos;
                                break;
                            }
                        }
                        if (!remained)
                        {
                            carsInstances[i] = GameObject.Instantiate(ambPrefab, pos, new Quaternion(0,0,0,1));
                        }
                    }

                    // try
                    // {
                    //     int index = 0;
                    //     foreach (int id in cached_ids)
                    //     {
                    //         if (id == step.cars[i].id)
                    //             break;
                    //         index++;
                    //     }
                    //     carsInstances[i].transform.Rotate(0,rot_angles[index],0,Space.World);
                    // }
                    // catch {}

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