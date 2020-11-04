using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListObject : MonoBehaviour
{
    public string objName;
    public int index;
    public List<SubListClass> subListClasses;
    public GameObject writeSonInfo;
    public GameObject showSonInfo;

    private int countSon=0;//添加子菜单数量

    int ccount=0;

    List<Transform> sublist;//实例化生成的子级菜单
    

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
        //更新一下子级菜单列表
        ButtonClickTreeShow();
    }


    public void ButtonClickAddTree()
    {
        //用户按下添加按钮
        ButtonClickTreeShow();
    }



    public void ButtonClickDeletTree()
    {
        //用户按下删除按钮,找到list记录删除掉，

        //并销毁按钮的父物体

        //然后更新子级菜单列表
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

    private void ButtonClickTreeShow()
    {
        //调节content列表

        //生成子菜单
        GameObject temp1 = Instantiate(Resources.Load<GameObject>("writeSonInfo"), this.transform.parent);
        temp1.transform.GetComponent<Button>().onClick.AddListener(ButtonClickAddTree);
        sublist.Add(temp1.transform);
        for (int i = 0; i < subListClasses.Count; i++)
        {
            GameObject temp2 = Instantiate(Resources.Load<GameObject>("ShowSonInfo"),this.transform.parent);
            temp2.transform.GetComponentInChildren<Text>().text = subListClasses[i].objName;
            temp2.transform.GetComponentInChildren<Toggle>().isOn = subListClasses[i].isok;
            temp2.transform.GetComponent<Button>().onClick.AddListener(ButtonClickDeletTree);
            sublist.Add(temp2.transform);
        }
    }

    private void ButtonClickTreeHide()
    {
        for (int i = sublist.Count; i >0; i--)
        {
            Destroy(sublist[i].gameObject);
            sublist.Remove(sublist[i].transform);
        }
        sublist.Clear();
    }

}
