using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class TCPClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    private Dictionary<string, float> enemyLevel = new Dictionary<string, float>
    {
        {"Enemy1(Clone)", 0f},
        {"Enemy2(Clone)", 1f},
        {"Enemy3(Clone)", 2f},
        {"Enemy4(Clone)", 3f},
        {"Enemy5(Clone)", 4f},
        {"Enemy6(Clone)", 5f},
        {"Enemy7(Clone)", 6f}
    };
    private Dictionary<float, int> enemyPosX = new Dictionary<float, int>
    {
        {-2.2f, 0},
        {-1.1f, 1},
        {0f, 2},
        {1.1f, 3},
        {2.2f, 4}
    };

    private float preReward = 0;

    void Start()
    {
        ConnectToServer();
        // InvokeRepeating("SendJsonRequestToPython", 0f, 0.1f);
        
    }

    void Update()
    {
        // ConnectToServer();
        // InvokeRepeating("SendJsonRequestToPython", 0f, 0.1f);
        SendJsonRequestToPython();
   
        
    }

    public float[] SetStateXY(int size, string tag)
    {
        size *= 2; // 
        // size 크기에 맞춰 배열 설정
        float[] floatList = new float[size];
        for (int i = 0; i < size; i++)
        {
            floatList[i] = 0.0f;
        }

        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(tag);
        int index = 0;
        foreach (GameObject obj in targetObjects)
        {
            Vector3 position = obj.transform.position;
            floatList[index] = position.x;
            index += 1;
            floatList[index] = position.y;
            index += 1;

            if (index == size) {
                break;
            }
            
        }
        return floatList;
    }


    public float[] SetStateEnemy(int size, string tag)
    {
        size *= 2; // 
        // size 크기에 맞춰 배열 설정
        float[] floatList = new float[size];
        for (int i = 0; i < size; i++)
        {
            floatList[i] = 0.0f;
        }

        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in targetObjects)
        {   
            Vector3 position = obj.transform.position;
            if (enemyPosX.ContainsKey(position.x)) {
                int index = enemyPosX[position.x];
                floatList[index] = position.y;
                floatList[index+1] = enemyLevel[obj.name];
            } else {
                Debug.LogError("x pos err" );
            }
        }
        return floatList;
    }

    // axis == 0 x축
    // axis == 1 y축
    public float[] SetState(int size, string tag, int axis)
    {
        // size 크기에 맞춰 배열 설정
        float[] floatList = new float[size];
        for (int i = 0; i < size; i++)
        {
            floatList[i] = 0.0f;
        }

        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(tag);
        int index = 0;
        foreach (GameObject obj in targetObjects)
        {
            Vector3 position = obj.transform.position;
            // axis == 0 x축
            // axis == 1 y축
            if (axis == 0) {
                floatList[index] = position.x;
            } else {
                floatList[index] = position.y;
            }
            index += 1;

            if (index == size) {
                break;
            }
            
        }

        return floatList;
    }



    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("localhost", 8080);
            stream = client.GetStream();
        }
        catch (Exception e)
        {
            Debug.LogError("Socket exception: " + e);
        }
    }

    void SendJsonRequestToPython()
    {
        float[] enemyPosList = SetStateEnemy(5, "Enemy"); // 15
        // Debug.LogError("enemyPosList exception: " + enemyPosList);
        float[] coinPosList = SetStateXY(5, "Coin"); // 10
        float[] playerPosList = SetState(1, "Player", 0); // 1
        float[] bossPosList = SetState(1, "Boss", 1); // 1
        float[] coin = new float[] {GameManager.instance.GetCoin()}; // 1
        float[] coinLevel = new float[] {GameManager.instance.GetCoinLevel()}; // 1
        float[] moveSpeed = new float[] {GameManager.instance.moveSpeed}; // 1

        float[] stateList = enemyPosList.Concat(coinPosList).Concat(playerPosList).Concat(bossPosList).Concat(coin).Concat(coinLevel).Concat(moveSpeed).ToArray();

   
        if (client == null || !client.Connected)
        {
            Debug.LogError("Not connected to the server");
            return;
        }

        try
        {
            // JSON 데이터 전송
            float currentConin = GameManager.instance.GetCoin();
            float r =  currentConin - preReward;
            preReward = currentConin;
            
            var dataToSend = new { state = stateList, reward = r, end = GameManager.instance.isGameOver};
            string jsonString = JsonConvert.SerializeObject(dataToSend);
            byte[] data = Encoding.ASCII.GetBytes(jsonString);
            stream.Write(data, 0, data.Length);

            // 응답 수신
            data = new byte[4];
            int bytes = stream.Read(data, 0, data.Length);
            int response = BitConverter.ToInt32(data, 0);

            // action 설정
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("Player");
            Vector3 moveTo = new Vector3(5f * Time.deltaTime, 0, 0);
            // Debug.Log("action = " + response);
            if (response == 0) {
                targetObjects[0].transform.position -= moveTo;
            } else if (response == 1) {
                targetObjects[0].transform.position += moveTo;
            }
            Debug.Log("Received from Python: " + response);
        }
        catch (Exception e)
        {
            Debug.LogError("Socket exception: " + e);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

     void CloseConnection()
    {
        try
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            Debug.Log("Connection closed");
        }
        catch (Exception e)
        {
            Debug.LogError("Exception when closing connection: " + e);
        }
    }

    
}
