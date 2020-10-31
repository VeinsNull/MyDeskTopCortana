using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
        while (fqTime > 0)
        {
            TimeSpan ts = new TimeSpan(0, 0, fqTime);
            //文本显示时间
            fqtimeText.text = string.Format("{0}:{1}:{2}",ts.Hours, ts.Minutes, ts.Seconds);
            //计算时间加载进度
            float tempTime = (float)Math.Round((float)(copyTime - fqTime) / copyTime, 4);
            fqImage.fillAmount = tempTime;
            LoadingImage = tempTime;
            yield return new WaitForSeconds(1f);
            fqTime -= 1;
        }
        FQRecover();
        yield break;
    }

    void FQRecover()
    {
        fqButton.GetComponentInChildren<Text>().text = "开始";
        fqTime = 0;
        fqImage.fillAmount = 1;
        fqtimeText.text = "";
        fqTimeInfo.text = "";
        LoadingImage = 1;
    }
}
