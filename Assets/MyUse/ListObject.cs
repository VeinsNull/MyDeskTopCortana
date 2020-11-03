using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListObject : MonoBehaviour
{
    public string objName;
    public int index;
    public GameObject writeSonInfo;
    public GameObject showSonInfo;

    private int countSon=0;//添加子菜单数量


    private List<SubListClass> subListClasses;

    private Text itemText;

    private void Start()
    {
        itemText = GetComponentInChildren<Text>();
        itemText.text = objName;
    }


    public void setObjectInfo(string name, int index)
    {
        this.objName = name;
        this.index = index;
    }


    public void ButtonClickAddTreeInfoOK()
    {
        //用户输入完成信息后
        SubListClass subList=new SubListClass();
        subList.objName = writeSonInfo.GetComponentInChildren<InputField>().text;
        subList.index = countSon;
        subList.isok = false;
        subListClasses.Add(subList);
        countSon += 1;
        Instantiate(showSonInfo);
        showSonInfo.GetComponentInChildren<Text>().text = subList.objName;
        showSonInfo.GetComponentInChildren<Toggle>().isOn = subList.isok;
    }


    public void ButtonClickAddTree()
    {
        //用户按下添加按钮
        ButtonClickTreeShow();
        //首先出现第一个perfab，用户选择输入任务，输入任务并确定后，在第一个perfab位置生成togger按钮

    }



    public void ButtonClickDeletTree()
    {
        //用户按下删除按钮
    }

    public void ButtonClickTree()
    {
        //检测用户按下次数，第一次按出现树形菜单，第二次隐藏

    }

    private void ButtonClickTreeShow()
    {

    }

    private void ButtonClickTreeHide()
    {

    }

}
