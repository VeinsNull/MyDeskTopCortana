using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MyClockManager : MonoBehaviour
{
    [SerializeField]
    private InputField fqTimeInfo;//用户输入的番茄钟时间
    [SerializeField]
    private Text fqtimeText;//显示时间
    [SerializeField]
    private Image fqImage;//用来显示时间进度的图片
    [SerializeField]
    private Button fqButton;//番茄钟的开始button
    private int fqButtonCount = 0;//番茄钟按钮按下计数 
    private int fqTime;
    private int fqTimee;

    [SerializeField]
    private Text jstimeText;//用来显示计时器时间
    [SerializeField]
    private Image JsImage;
    [SerializeField]
    private Sprite[] jsShowImage;//用来显示的图片
    [SerializeField]
    private Button jsButton1;//计时器开始的button
    [SerializeField]
    private Button jsButton2;
    private int jsButtonCount = 0;//计时器按钮按下计数 
    private int jsTime;//计时器时间
    private bool aniContro = false;
    float mi = 0;

    [SerializeField]
    private Button backButton;//返回按钮
    [SerializeField]
    private Transform toDoListCanvas;
    [SerializeField]
    private Transform clockPanelCanvas;

    public ClockList clockList;
    public List<SubClockList> subClockList;

    string filePath;//存放同步的json文件
    string jsonName = "ClockTime.json";

    public float LoadingImage { get; private set; }//用来分享进度，用来控制人物透明度


    void Start()
    {
        fqTimeInfo.text = "";
        fqtimeText.text = "";
        jstimeText.text = "";
        fqButton.onClick.AddListener(FQButtonDown);
        jsButton1.onClick.AddListener(JSButtonOneDown);
        jsButton2.onClick.AddListener(JSButtonTwoDown);
        backButton.onClick.AddListener(SetBack);
        clockList = new ClockList();
        subClockList = new List<SubClockList>();


        //程序一开始找到json文件目录，并赋值给filepath；
        if (Application.platform == RuntimePlatform.Android)
        {
            CoreManage.Instance.clockFilePath = Path.Combine(Application.persistentDataPath, jsonName);
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            CoreManage.Instance.clockFilePath = @"D:\ClockTime.json";
        }
        loadJsonData();
    }

    void OnEnable()
    {
#if UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
    }

    void OnDisable()
    {
#if UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
#endif
    }

    void FixedUpdate()
    {
        if (aniContro == true)
        {
            mi += 1;
            if (mi < 2)
            {
                JsImage.sprite = jsShowImage[0];
            }
            if (mi > 2)
            {
                JsImage.sprite = jsShowImage[1];
                mi = 0;
            }
        }
    }


    public void SetBack()
    {
        toDoListCanvas.gameObject.SetActive(true);
        clockPanelCanvas.gameObject.SetActive(false);
    }

    private void JSButtonTwoDown()
    {
        //用户按下结束键，停止计时，时间归零
        clockList.totalTimer += (jsTime/60);
        SubClockList temp = new SubClockList();
        temp.subName = clockPanelCanvas.Find("InputTaskName").GetComponent<InputField>().text;
        temp.subTimer = (jsTime/60);
        subClockList.Add(temp);
        clockList.sub = subClockList;
        saveJsonData();

        StopCoroutine("JSTimeCoro");
        jsTime = 0;
        jstimeText.text = "";
        jsButton1.GetComponentInChildren<Text>().text = "开始";
        JsImage.sprite = jsShowImage[0];
        jsButtonCount = 0;
        aniContro = false;
    }

    private void JSButtonOneDown()
    {
        jsButtonCount += 1;
        if (jsButtonCount == 1)//开始
        {
            jsButton1.GetComponentInChildren<Text>().text = "暂停";
            StartCoroutine("JSTimeCoro");
            aniContro = true;
        }
        else if (jsButtonCount == 2)//暂停
        {
            jsButton1.GetComponentInChildren<Text>().text = "开始";
            StopCoroutine("JSTimeCoro");
            jsButtonCount = 0;
            aniContro = false;
        }
    }

    IEnumerator JSTimeCoro()
    {
        while (true)
        {
            TimeSpan ts = new TimeSpan(0, 0, jsTime);
            //文本显示时间
            jstimeText.text = string.Format("{0}:{1}:{2}", ts.Hours, ts.Minutes, ts.Seconds);
            yield return new WaitForSeconds(1f);
            jsTime += 1;
        }
    }


    //当用户按下番茄钟的开始按钮后
    private void FQButtonDown()
    {
        fqButtonCount += 1;
        if (fqButtonCount == 1)
        {
            fqImage.fillAmount = 0;
            fqButton.GetComponentInChildren<Text>().text = "停止";
            fqTime = Convert.ToInt32(fqTimeInfo.text) * 60;
            StartCoroutine("FQTimeCoro");
            fqTimeInfo.text = "";
        }
        else if (fqButtonCount == 2)
        {
            //如果用户按下了停止按键，一切恢复初始值
            FQRecover();
            StopCoroutine("FQTimeCoro");
            fqButtonCount = 0;          
        }
    }

    IEnumerator FQTimeCoro()
    {
        int copyTime = fqTime;
        fqTimee = fqTime;
        while (fqTimee > 0)
        {
            TimeSpan ts = new TimeSpan(0, 0, fqTimee);
            //文本显示时间
            fqtimeText.text = string.Format("{0}:{1}:{2}", ts.Hours, ts.Minutes, ts.Seconds);
            //计算时间加载进度
            float tempTime = (float)Math.Round((float)(copyTime - fqTimee) / copyTime, 4);
            fqImage.fillAmount = tempTime;
            LoadingImage = tempTime;
            yield return new WaitForSeconds(1f);
            fqTimee -= 1;
        }
        GetComponent<MyProgramTray>().SetTipp();
        FQRecover();
        yield break;
    }

    void FQRecover()
    {
        clockList.totalTimer += ((fqTime-fqTimee) / 60);
        SubClockList temp = new SubClockList();
        temp.subName = clockPanelCanvas.Find("InputTaskName").GetComponent<InputField>().text;
        temp.subTimer = ((fqTime - fqTimee) / 60);
        subClockList.Add(temp);
        clockList.sub = subClockList;
        saveJsonData();

        fqButton.GetComponentInChildren<Text>().text = "开始";
        fqTime = 0;
        fqTimee = 0;
        fqImage.fillAmount = 1;
        fqtimeText.text = "";
        fqTimeInfo.text = "";
        LoadingImage = 1;
    }


    /// <summary>
    /// 从列表中读取数据写入到json文件中
    /// </summary>
    private void saveJsonData()
    {
        string contents = "";
        contents = JsonUtility.ToJson(clockList) + "\n";
        Debug.Log(contents);
        File.WriteAllText(CoreManage.Instance.clockFilePath, contents);
    }

    /// <summary>
    /// 从存储中加载json文件
    /// </summary>
    public void loadJsonData()
    {
        string dataAsJson = "";
        dataAsJson = CoreManage.Instance.ReadJsonFun("Clock");

        // 正确解析json文件
        string[] splitContents = dataAsJson.Split('\n');
        foreach (string content in splitContents)
        {
            if (content.Trim() != "")
            {
                ClockList temp = JsonUtility.FromJson<ClockList>(content.Trim());
                clockList = temp;
                for (int i = 0; i < clockList.sub.Count; i++)
                {
                    subClockList[i] = clockList.sub[i];
                }
            }
        }
    }
}
