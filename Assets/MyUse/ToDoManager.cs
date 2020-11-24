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

[Serializable]
public class listItemClass
{
    public string objName;
    public int index;
    public List<SubListClass> sub;
    public listItemClass(string name, int index,List<SubListClass> sudata)
    {
        this.objName = name;
        this.index = index;
        //list被json转化后的string字符串
        this.sub = sudata;
    }
}

[Serializable]
public class SubListClass
{
    public string objName;
    public int index;
    public bool isok;
    public SubListClass(string name, int index, bool isok)
    {
        this.objName = name;
        this.index = index;
        this.isok = isok;
    }
}



public class ToDoManager : MonoBehaviour
{
    [SerializeField]
    private Transform toDoList;
    [SerializeField]
    private Transform clockPanel;

    public InputField todoListInput;//用户输入的任务
    public Transform content;//存放添加后的任务列表
    public GameObject ListItemPreFab;//用户任务列表预制体

    string filePath;//存放同步的json文件
    string jsonName = "List.json";
    Thread connectThread;
    private List<ListObject> ListObjects = new List<ListObject>();

    bool buttonOk = false;
    bool downOk = false;




    void Start()
    {
        buttonOk = true;
        //程序一开始找到json文件目录，并赋值给filepath；
        if (Application.platform == RuntimePlatform.Android)
        {
            filePath = Path.Combine(Application.persistentDataPath, jsonName);
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            filePath = @"D:\List.json";
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
        File.WriteAllText(filePath, contents);
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
        dataAsJson = ReadJsonFun();

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

    void UpdateCould()
    {
        #region 连接服务器
        IPAddress ip = IPAddress.Parse("45.77.102.177");
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
        //读取本地json文件，将其转换为string类型
        string json = ReadJsonFun();
        Debug.Log(json);
        //将string类型的数据发送给服务器，让服务器以json文件保存
        clientSocket.Send(Encoding.UTF8.GetBytes(json));
        clientSocket.Close();
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
        string contents = Decodeing(recvStr);
        contents = contents.Replace(@"\\n", "\n");
        contents = contents.Replace(@"\\", "");
        contents = contents.Replace(@"\", "");
        contents = contents.Replace("\'\"", "");
        contents = contents.Replace("\"\'", "");
        Debug.Log("从服务端获取的数据解析后为：" + contents);
        File.WriteAllText(filePath, contents);
        clientSocket.Close();
        downOk = true;
        
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }      
    }

    string ReadJsonFun()
    {
        string dataAsJson = File.ReadAllText(filePath,Encoding.UTF8);
        if(dataAsJson==null)
        {
            return "";
        }
        return dataAsJson;      
    }
    string Decodeing(string s)
    {
        Regex reUnicode = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);
        return reUnicode.Replace(s, m =>
        {
            short c;
            if (short.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out c))
            {
                return "" + (char)c;
            }
            return m.Value;
        });
    }
}

