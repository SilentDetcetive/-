using UnityEngine;

public class MapPageManager : MonoBehaviour
{
    [Header("地图面板引用")]
    public GameObject map1;
    public GameObject map2;
    public GameObject map3;
    public GameObject map4;
    public GameObject map5;
    public GameObject map6;
    public GameObject map7;

    void Start()
    {
        // 游戏启动时，强制确保只显示第一张图
        GoToMap1();
    }

    // 跳转到图 1
    public void GoToMap1()
    {
        map1.SetActive(true);
        map2.SetActive(false);
        map3.SetActive(false);
        map4.SetActive(false);
        map5.SetActive(false);
        map6.SetActive(false);
        map7.SetActive(false);
    }

    // 跳转到图 2
    public void GoToMap2()
    {
        map1.SetActive(false);
        map2.SetActive(true);
        map3.SetActive(false);
        map4.SetActive(false);
        map5.SetActive(false);
        map6.SetActive(false);
        map7.SetActive(false);
    }

    // 跳转到图 3
    public void GoToMap3()
    {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(true);
        map4.SetActive(false);
        map5.SetActive(false);
        map6.SetActive(false);
        map7.SetActive(false);
    }

    public void GoToMap4()
    {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(false);
        map4.SetActive(true);
        map5.SetActive(false);
        map6.SetActive(false);
        map7.SetActive(false);
    }

    public void GoToMap5()
    {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(false);
        map4.SetActive(false);
        map5.SetActive(true);
        map6.SetActive(false);
        map7.SetActive(false);
    }

    public void GoToMap6()
    {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(false);
        map4.SetActive(false);
        map5.SetActive(false);
        map6.SetActive(true);
        map7.SetActive(false);
    }

    public void GoToMap7()
    {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(false);
        map4.SetActive(false);
        map5.SetActive(false);
        map6.SetActive(false);
        map7.SetActive(true);
    }
}