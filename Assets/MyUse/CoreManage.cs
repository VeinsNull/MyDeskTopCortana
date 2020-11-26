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


#region 番茄钟json格式
[Serializable]
public class ClockList
{
    public int totalTimer;
    public List<SubClockList> sub;
}

[Serializable]
public class SubClockList
{
    public string subName;
    public int subTimer;
}
#endregion

#region 任务清单json格式
[Serializable]
public class listItemClass
{
    public string objName;
    public int index;
    public List<SubListClass> sub;
    public listItemClass(string name, int index, List<SubListClass> sudata)
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
#endregion



public class CoreManage
{

    static CoreManage instance;

    public static CoreManage Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CoreManage();
            }
            return instance;
        }
    }

    public string todoFilePath;
    public string clockFilePath;

    public string Decodeing(string s)
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


    /// <summary>
    /// 读取本地todoJson文件，将其转换为string类型
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string ReadJsonFun(string str)
    {
        string dataAsJson="";
        if (str == "Todo")
        {
           dataAsJson = File.ReadAllText(todoFilePath, Encoding.UTF8);
            if (dataAsJson != null)
            {
                return dataAsJson;
            }
        }
        else if(str =="Clock")
        {
            dataAsJson = File.ReadAllText(clockFilePath, Encoding.UTF8);
            if (dataAsJson != null)
            {
                return dataAsJson;
            }
        }
        return dataAsJson;
    }
}
