using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ListObject : MonoBehaviour
{
    public string objName;
    public int index;

    public GameObject writeSonInfo;
    public GameObject showSonInfo;
    public int countSon=0;//添加子菜单数量

    public List<SubListClass> sublistcalss;
    public List<SubListObject> subListObjects;

    public GameObject myWriteSon;
    int ccount=0;//用来记录用户按下按键次数  


    private void Start()
    {
        this.GetComponentInChildren<Text>().text = objName;
        myWriteSon = Instantiate(writeSonInfo, this.transform.parent);
        myWriteSon.transform.GetComponentInChildren<Button>().onClick.AddListener(ButtonClickAddTreeInfoOK);
        myWriteSon.SetActive(false);
    }


    public void setObjectInfo(string name, int index)
    {
        this.objName = name;
        this.index = index;
    }


    public void ButtonClickAddTreeInfoOK()
    {
        //用户输入完成信息后
        CreateSubListItem(myWriteSon.GetComponentInChildren<InputField>().text,
             false, countSon);
        myWriteSon.GetComponentInChildren<InputField>().text = "";
        countSon += 1;
    }

    private void CreateSubListItem(string temp, bool isok,int loadIndex = 0,bool Show=false)
    {
        GameObject subtemp = Instantiate(showSonInfo, this.transform.parent);
        SubListObject subtempObj = subtemp.GetComponent<SubListObject>();      
        if(Show==true)
        {
            SubListClass subtempClas = new SubListClass(temp, loadIndex, isok);
            sublistcalss.Add(subtempClas);
        }  
        int index = sublistcalss.Count;
        subtempObj.setSubObjectInfo(temp, loadIndex, isok,this.gameObject);
        subListObjects.Add(subtempObj);      
    }

    public void ButtonClickTree()
    {
        //检测用户按下次数，第一次按出现树形菜单，第二次隐藏
        ccount += 1;
        if(ccount==1)
        {
            ButtonClickTreeShow();
        }
        else if(ccount==2)
        {
            ButtonClickTreeHide();
            ccount = 0;
        }
    }

    public void ButtonClickTreeShow()
    {
        //调节content列表


        //生成子菜单
        myWriteSon.SetActive(true);

        for (int i = 0; i < sublistcalss.Count; i++)
        {
            //CreateSubListItem()
        }
    }

    private void ButtonClickTreeHide()
    {
        myWriteSon.SetActive(false);
        for (int i = 0; i < subListObjects.Count; i++)
        {
            subListObjects[i].gameObject.SetActive(false);
        }
    }

}
