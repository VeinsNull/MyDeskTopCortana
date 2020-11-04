using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListObject : MonoBehaviour
{
    public string objName;
    public int index;

    public GameObject writeSonInfo;
    public GameObject showSonInfo;
    public int countSon=0;//添加子菜单数量

    public List<SubListObject> subListObjects;

    public GameObject myWriteSon;
    int ccount=0;//用来记录用户按下按键次数  


    private void Start()
    {
        this.GetComponentInChildren<Text>().text = objName;
    }


    public void setObjectInfo(string name, int index)
    {
        this.objName = name;
        this.index = index;
    }


    public void ButtonClickAddTreeInfoOK()
    {
        //用户输入完成信息后
        GameObject tempobj = Instantiate(showSonInfo);
        tempobj.GetComponentInChildren<SubListObject>().setSubObjectInfo(writeSonInfo.GetComponentInChildren<InputField>().text,
            countSon, false, this.gameObject);
        subListObjects.Add(tempobj.GetComponent<SubListObject>());
        countSon += 1;
        //更新一下子级菜单列表
        ButtonClickTreeShow();
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

        myWriteSon = null;
        //生成子菜单
        myWriteSon = Instantiate(Resources.Load<GameObject>("writeSonInfo"), this.transform.parent);
        myWriteSon.transform.GetComponentInChildren<Button>().onClick.AddListener(ButtonClickAddTreeInfoOK);

        for (int i = 0; i < subListObjects.Count; i++)
        {
            GameObject temp2 = Instantiate(Resources.Load<GameObject>("ShowSonInfo"),this.transform.parent);
            temp2.GetComponent<SubListObject>().setSubObjectInfo(
               subListObjects[i].objName, subListObjects[i].index, subListObjects[i].isok, this.gameObject);
        }
    }

    private void ButtonClickTreeHide()
    {
        for (int i = subListObjects.Count; i >0; i--)
        {
            Destroy(subListObjects[i].gameObject);
        }
    }

}
