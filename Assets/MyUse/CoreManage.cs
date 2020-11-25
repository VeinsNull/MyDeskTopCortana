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


#region 定义消息头部

/// <summary>
/// Demo消息头定义
/// </summary>
public struct SocketHead
{
    //起始位，表示字节的开始
    public byte StartFlag;
    //校验位，检验数据是否正确
    public byte CheckNum;
    //协议位，表示需要执行什么功能
    public byte Cmd;
    //消息体数据长度
    public int Length;
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



    #region 数据长度与字节数组的转换
    /// <summary>
    /// 将int类型以数组方式添加到目标数组
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    public static void IntoBytes(byte[] data, int offset, int value)
    {
        data[offset++] = (byte)(value);
        data[offset++] = (byte)(value >> 8);
        data[offset++] = (byte)(value >> 16);
        data[offset] = (byte)(value >> 24);
    }

    /// <summary>
    /// bytes数据长度转成int类型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static int ToInt32(byte[] data, int offset)
    {
        return (int)(data[offset++] | data[offset++] << 8 | data[offset++] << 16 | data[offset] << 24);
    }
    #endregion

    /// <summary>
    /// 构建数据包头
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public  byte[] BuildData(byte cmd, byte[] data)
    {
        byte[] buffer = new byte[7 + data.Length];
        byte startFlag = 0xF;
        //起始位
        buffer[0] = startFlag;
        //指令位
        buffer[1] = cmd;
        //校验位
        buffer[2] = (byte)(cmd + startFlag);
        IntoBytes(buffer, 3, data.Length);
        Array.Copy(data, 0, buffer, 7, data.Length);
        return buffer;
    }


    /// <summary>
    /// 解析数据包头
    /// </summary>
    /// <param name="data"></param>
    /// <param name="socketHead"></param>
    /// <returns></returns>
    public  bool ParseHead(byte[] data, out SocketHead socketHead)
    {
        if (data.Length >= 7)
        {
            socketHead = new SocketHead
            {
                StartFlag = data[0],
                Cmd = data[1],
                CheckNum = data[2]
            };
            //验证数据是否正确
            if (socketHead.CheckNum == socketHead.StartFlag + socketHead.Cmd)
            {
                socketHead.Length = ToInt32(data, 3);
                return true;
            }
            return false;
        }
        socketHead = new SocketHead();
        return false;
    }
}
