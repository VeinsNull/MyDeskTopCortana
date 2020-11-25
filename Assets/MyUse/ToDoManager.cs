using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System;
using System.Collections;
using TMPro;

public class ToDoManager : MonoBehaviour
{
    [SerializeField]
    private Transform toDoList;
    [SerializeField]
    private Transform clockPanel;

    public InputField todoListInput;//用户输入的任务
    public Transform content;//存放添加后的任务列表
    public GameObject ListItemPreFab;//用户任务列表预制体

    string jsonName = "todo.json";
    Thread connectThread;
    private List<ListObject> ListObjects = new List<ListObject>();

    bool buttonOk = false;
    bool downOk = false;

    string transmissionStatus="";

    bool whileFlag = false;

    void Start()
    {
        buttonOk = true;
        //程序一开始找到json文件目录，并赋值给filepath；
        if (Application.platform == RuntimePlatform.Android)
        {
            CoreManage.Instance.todoFilePath = Path.Combine(Application.persistentDataPath, jsonName);
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            CoreManage.Instance.todoFilePath = @"D:\todo.json";
        }
        loadJsonData();
    }

    void Update()
    {
        if (downOk)
        {
            downOk = false;
            //从服务器上下载数据
            loadJsonData();           
        }         
    }

    public void CreateNewItem()
    {
        string temp = todoListInput.text;
        Debug.Log(temp);
        CreateListItem(temp);
        todoListInput.text = "";
    }

    /// <summary>
    /// 创建实例化的按钮及更新信息
    /// 用户点击创建按钮时候，先将数据存到本地，按用户需求上传。
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="loadIndex"></param>
    /// <param name="loding"></param>
    private void CreateListItem(string temp, int loadIndex = 0, bool loding = false, listItemClass listclass=null)
    {
        //实例化清单
        GameObject item = Instantiate(ListItemPreFab, content);
        ListObject itemObject = item.GetComponent<ListObject>();
        int index = 0;
        if (loding != true)
        {
            index = ListObjects.Count;
        }
        itemObject.setObjectInfo(temp, index);
        
        //为清单上的信息赋值
        item.GetComponentInChildren<Text>().text = temp;
        if (listclass != null)
        {   //清除之前的老信息，添加新信息

            //查找名字符合的

            itemObject.sublistcalss.Clear();
            for (int i = 0; i < listclass.sub.Count; i++)
            {
                SubListClass subobj = new SubListClass(listclass.sub[i].objName,listclass.sub[i].index, listclass.sub[i].isok);
                itemObject.sublistcalss.Add(subobj);
            }            
        }
        ListObjects.Add(itemObject);

        //为button的点击事件添加监听
        itemObject.transform.Find("abandon").GetComponent<Button>().onClick.AddListener((delegate { CheckItem(itemObject); }));
        itemObject.transform.Find("ShowTree").GetComponent<Button>().onClick.AddListener((delegate { itemObject.ButtonClickTree(); }));
        itemObject.transform.Find("clock").GetComponent<Button>().onClick.AddListener((delegate { InClock(temp); }));
        if (loding != true)
        {
            saveJsonData();
        }
    }

     void InClock(string obj)
    {
        clockPanel.gameObject.SetActive(true);
        clockPanel.Find("InputTaskName").GetComponent<InputField>().text = obj;
        toDoList.gameObject.SetActive(false);
    }

    void CheckItem(ListObject item)
    {   
        if (item.subListObjects != null)
        {
            for (int i = item.subListObjects.Count; i > 0; i--)
            {
                item.subListObjects[i - 1].gameObject.GetComponent<SubListObject>().DelButtonClick();
            }
            item.sublistcalss.Clear();
        }
        ListObjects.Remove(item);
        Destroy(item.gameObject);
        Destroy(item.myWriteSon.gameObject);
        saveJsonData();
    }

    /// <summary>
    /// 从储存清单列表的列表中读取数据写入到json文件中
    /// </summary>
    public void saveJsonData()
    {
        string contents = "";
        List<SubListClass> templist=new List<SubListClass>();
        for (int i = 0; i < ListObjects.Count; i++)
        {
            if (ListObjects[i].sublistcalss.Count > 0)
            {
                for (int j = 0; j < ListObjects[i].sublistcalss.Count; j++)
                {
                    templist.Add(ListObjects[i].sublistcalss[j]);
                }
            }           
            listItemClass temp2 = new listItemClass(ListObjects[i].objName, ListObjects[i].index,templist);
            contents += JsonUtility.ToJson(temp2) + "\n";
            templist.Clear();
        }
        Debug.Log(contents);
        File.WriteAllText(CoreManage.Instance.todoFilePath, contents);
    }

    /// <summary>
    /// 从存储中加载json文件
    /// </summary>
    void loadJsonData()
    {
        if(content.childCount>0)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
        ListObjects.Clear();
        string dataAsJson = "";
        dataAsJson =CoreManage.Instance.ReadJsonFun("Todo");

        // 正确解析json文件
        string[] splitContents = dataAsJson.Split('\n');
        foreach (string content in splitContents)
        {
            if (content.Trim() != "")
            {
                listItemClass temp = JsonUtility.FromJson<listItemClass>(content.Trim());
                //先只创建父级任务，然后将子任务信息存到父级任务的listObject脚本中去。
                CreateListItem(temp.objName, temp.index, true,temp);
            }
        }
    }

    public void ButtonClick(string info)
    {
        
        if (buttonOk)
        {
            StartCoroutine(WaitTime());
            if (info == "下载")
            {
                connectThread = new Thread(new ThreadStart(CouldDown));
                connectThread.Start();
            }
            else
            {
                connectThread = new Thread(new ThreadStart(UpdateCould));
                connectThread.Start();
            }
        }
        else
        {
            Debug.Log("请隔1秒后再试");
        }   
    }

    IEnumerator WaitTime()
    {
        buttonOk = false;
        yield return new WaitForSeconds(1f);
        buttonOk = true;
        yield break;
    }

    void TransmissionStatus(Socket clientSocket)
    {
        while (whileFlag)
        {
            string recvStr = "";
            byte[] recvBytes = new byte[10240];
            int bytes;
            bytes = clientSocket.Receive(recvBytes, recvBytes.Length, 0);    //从服务器端接受返回信息 
            recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
            transmissionStatus = recvStr.Trim();
        }
    }

    void UpdateCould()
    {
        #region 连接服务器
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            clientSocket.Connect(new IPEndPoint(ip, 2333)); //配置服务器IP与端口  
            Debug.Log("连接服务器成功:准备上传");
        }
        catch
        {
            Debug.Log("连接服务器失败");
            return;
        }
        #endregion

        #region 本线程发送数据到服务器，再次线程中再开一个线程，专门接收状态信息。
        whileFlag = true;
        Thread upThread = new Thread(() => TransmissionStatus(clientSocket));
        upThread.Start();
        clientSocket.Send(Encoding.UTF8.GetBytes("UP"));

        while (true)
        {
            if(transmissionStatus=="con")//服务器发送信息，连接成功，我们给他发送todo，让他准备好接收todojson文件
            {
                clientSocket.Send(Encoding.UTF8.GetBytes("todo"));
                Debug.Log(transmissionStatus);
                transmissionStatus = "";
            }
            else if(transmissionStatus=="todoStart")//服务器接到消息准备接收
            {
                //todolist和clock数据可能随着时间推移过大，所以需要拆包分包发送
                string todoJson = CoreManage.Instance.ReadJsonFun("Todo");
                byte[] bufferData = CoreManage.Instance.BuildData(0x9, Encoding.UTF8.GetBytes(todoJson));
                int len = clientSocket.Send(Encoding.UTF8.GetBytes(todoJson));
                if (len == bufferData.Length)
                {
                    Debug.Log(todoJson);
                }
                Debug.Log(transmissionStatus);
                transmissionStatus = "";
            }
            else if(transmissionStatus=="todoCP")//todojson文件接收完毕，给他发送clock,让他准备好接收colockjson文件
            {
                clientSocket.Send(Encoding.UTF8.GetBytes("clock"));
                Debug.Log(transmissionStatus);
                transmissionStatus = "";
            }
            else if(transmissionStatus=="clockStart")//服务器接到消息准备接收
            {
                //todolist和clock数据可能随着时间推移过大，所以需要拆包分包发送
                string clockJson = CoreManage.Instance.ReadJsonFun("Clock");
                byte[] bufferData = CoreManage.Instance.BuildData(0x9, Encoding.UTF8.GetBytes(clockJson));
                int len = clientSocket.Send(Encoding.UTF8.GetBytes(clockJson));
                if (len == bufferData.Length)
                {
                    Debug.Log(clockJson);
                }
                transmissionStatus = "";
            }
            else if(transmissionStatus=="clockCP")//clockjson文件接收完毕，给他发送exit，退出客户端连接
            {
                whileFlag = false;
                clientSocket.Send(Encoding.UTF8.GetBytes("exit"));
                clientSocket.Close();
                transmissionStatus = "";
                break;                
            }
        }
        #endregion

        if (upThread!=null)
        {
            upThread.Interrupt();
            upThread.Abort();
        }
        //关掉线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
    }

    void CouldDown()
    {
        #region 连接服务器
        IPAddress ip = IPAddress.Parse("45.77.102.177");
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            clientSocket.Connect(new IPEndPoint(ip, 2333)); //配置服务器IP与端口  
            Debug.Log("连接服务器成功:准备下载");
        }
        catch
        {
            Debug.Log("连接服务器失败");
            return;
        }
        #endregion
        //封装到函数里由unity的button事件调用
        clientSocket.Send(Encoding.UTF8.GetBytes("下载"));//向服务器发送数据，需要发送中文则需要使用Encoding.UTF8.GetBytes()，否则会乱码
        //发送完下载命令后，准备接收服务端发过来的数据
        //！待解决问题：当传输数据过多时，数据丢失现象
        string recvStr = "";
        byte[] recvBytes = new byte[10240];
        int bytes;
        bytes = clientSocket.Receive(recvBytes, recvBytes.Length, 0);    //从服务器端接受返回信息 
        recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
        //格式化字符串，因为从服务端传过来的数据太乱了
        Debug.Log("从服务端获取的数据为：" + recvStr);
        string contents = CoreManage.Instance.Decodeing(recvStr);
        contents = contents.Replace(@"\\n", "\n");
        contents = contents.Replace(@"\\", "");
        contents = contents.Replace(@"\", "");
        contents = contents.Replace("\'\"", "");
        contents = contents.Replace("\"\'", "");
        Debug.Log("从服务端获取的数据解析后为：" + contents);
        File.WriteAllText(CoreManage.Instance.todoFilePath, contents);
        clientSocket.Close();
        downOk = true;

        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
    }
}

