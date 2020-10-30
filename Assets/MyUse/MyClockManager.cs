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
    private int fqButtonCount=0;//番茄钟按钮按下计数 
    private int fqTime;

    public float LoadingImage { get; private set; }//用来分享进度，用来控制人物透明度

    void Start()
    {
        fqTimeInfo.text = "";
        fqButton.onClick.AddListener(FQButtonDown);
    }

    void Update()
    {
        
    }

    //当用户按下番茄钟的开始按钮后
    private void FQButtonDown()
    {
        fqButtonCount += 1;
        if(fqButtonCount==1)
        {
            fqImage.fillAmount = 0;
            fqButton.GetComponentInChildren<Text>().text = "停止";
            fqTime = Convert.ToInt32(fqTimeInfo.text) * 60;
            StartCoroutine("FQTimeCoro");
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
            fqtimeText.text = string.Format("{0}:{1}", ts.Minutes, ts.Seconds);
            //计算时间加载进度
            float tempTime = (float)Math.Round((float)(copyTime - fqTime) / copyTime, 2);
            Debug.Log(tempTime);
            fqImage.fillAmount = tempTime;
            Debug.Log(fqImage.fillAmount);
            LoadingImage = tempTime;
            fqTime -= 1;
            yield return new WaitForSeconds(1f);
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
