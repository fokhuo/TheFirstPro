using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Xiaoxiaole : MonoBehaviour
{
    private GameObject m_Item;
    private Transform m_Transform;
    public int columnCounts = 8;
    public int rowCounts = 10;
    private List<ItemProperty> m_allItems;
    private List<ItemProperty> m_ItemTypes;
    private bool isStartRecord;
    private Dictionary<int,int> m_animationRecord;

    private class ItemProperty
    {
        public int ItemType;
        public int idx;
        public GameObject itemObj;
        public List<(int, int)> axisRecord = new List<(int, int)>();
    }

    // private List<ItemProperty> m_ItemPro = new List<ItemProperty>();
    // Start is called before the first frame update
    void Start()
    {
        m_Item = transform.Find("ItemsRoot/Image").gameObject;
        m_Transform = transform.Find("ItemsRoot");
        m_ItemTypes = new List<ItemProperty>();
        m_animationRecord = new Dictionary<int, int>();
        m_allItems = new List<ItemProperty>();
        InitItems();
    }

    private void InitItems()//初始化子项
    {
        var itemsCounts = 0;
        for (int rowCount = 0; rowCount < rowCounts; rowCount++)
        {
            for (int columnCount = 0; columnCount < columnCounts; columnCount++)
            {
                var obj = Instantiate(m_Item, m_Transform);
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(70 + 120 * columnCount, -70 - 120 * rowCount);
                obj.SetActive(true);
                var itemPro = new ItemProperty()
                {
                    idx = itemsCounts,
                    itemObj = obj,
                    ItemType = Random.Range(1,4),
                    axisRecord = new List<(int, int)>(){(columnCount,rowCount)}
                };
                itemsCounts++;
                m_allItems.Add(itemPro);
                var listener = obj.AddComponent<EventListener>();
                listener.onPointDown = () =>
                {
                    Debug.Log("按下按钮" + isStartRecord);
                    isStartRecord = true;
                    m_animationRecord.Clear();
                    m_ItemTypes.Add(itemPro);
                };
                listener.onPointUp = JudgementLine;
                // m_ItemPro.Add(itemPro);
                SetItemsPicture(itemPro.ItemType, itemPro.itemObj.GetComponent<Image>());
                listener.onPointEnter = () =>
                {
                    if (!isStartRecord) return;
                    Debug.Log("进入按钮");
                    {
                        for (int i = 0; i < m_ItemTypes.Count; i++)
                        {
                            if (m_ItemTypes[i].idx == itemPro.idx)
                            {
                                if (itemPro.idx == m_ItemTypes[0].idx && m_ItemTypes.Count >= 4)
                                    return;
                                m_ItemTypes.RemoveAt(i + 1);
                                return;
                            }
                        }
                        m_ItemTypes.Add(itemPro);
                    }
                };
            }
        }
    }

    private void JudgementLine() //判断是否满足连线
    {
        var stand = m_ItemTypes[0].ItemType;
        if (m_ItemTypes.Count < 3)
        {
            isStartRecord = false;
            m_ItemTypes.Clear();
            return;
        }
        foreach (var item in m_ItemTypes)
        {
            if (item.ItemType != stand)//不满足连线规则
            {
                Debug.Log("press a wrong value!");
                isStartRecord = false;
                m_ItemTypes.Clear();
                return;
            }
        }
        var itemsColumn = m_ItemTypes.Select(a => a.axisRecord[0].Item1).ToList();
        foreach (var value in itemsColumn)
        {
            if (m_animationRecord.ContainsKey(value))
                m_animationRecord[value] += 1;
            else
                m_animationRecord.Add(value,1);
        }

        foreach (var item in m_animationRecord)
        {
            Debug.Log("除去的items" + item.Key + " " + item.Value);
        }
        
        foreach (var item in m_ItemTypes)
        {
            RemoveItems(item);
        }
        isStartRecord = false;
        m_ItemTypes.Clear();
    }
    
    private void SetItemsPicture(int type,Image img)//给图标加上图片资源
    {
        switch (type)
        {
           case 1:
               var sprite1 = Resources.Load<Sprite>("jelly_arrow_TEX_RGB");
               img.sprite = sprite1;
               break;
           case 2:
               var sprite2 = Resources.Load<Sprite>("jelly_tiles_TEX_RGB");
               img.sprite = sprite2;
               break;
           case 3:
               var sprite3 = Resources.Load<Sprite>("jelly_coin_01_TEX");
               img.sprite = sprite3;
               break;
           default:
               break;
        }
    }

    private void GenerateNewItems()
    {
        
    }

    private List<ItemProperty> GainItems(int column,int row)//挑选删除项上面的items
    {
        // Debug.Log("99999999此时的行和列" + column + "  " + row);
        return m_allItems.Where(t => t.axisRecord[0].Item2 < row && t.axisRecord[0].Item1 == column).ToList();
    }

    private void RemoveItems(ItemProperty item) //移除的物体下落
    {
        item.itemObj.SetActive(false);
        m_allItems.RemoveAll(which => which.idx == item.idx);
        var items = GainItems(item.axisRecord[0].Item1, item.axisRecord[0].Item2);
   
        foreach (var o in items)
        {
            // o.itemObj.GetComponent<RectTransform>().anchoredPosition =  new Vector2(
            //     o.itemObj.GetComponent<RectTransform>().anchoredPosition.x,
            //     o.itemObj.GetComponent<RectTransform>().anchoredPosition.y - 120);
            foreach (var b in m_animationRecord)
            {
                if (o.axisRecord[0].Item1 == b.Key)
                {
                    StartCoroutine(PlayFallAnim(o.itemObj.GetComponent<RectTransform>().anchoredPosition,
                        new Vector2(
                            o.itemObj.GetComponent<RectTransform>().anchoredPosition.x,
                            o.itemObj.GetComponent<RectTransform>().anchoredPosition.y - 120*b.Value),o.itemObj));
                }
            }
        }
    }

    private static IEnumerator PlayFallAnim(Vector2 start ,Vector2 end,GameObject item)//下落动画
    {
        var process = 0.0f;
        const float duration = 0.3f;
        while (process <= 1)
        {
            process += Time.deltaTime/duration;
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(start, end, process);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
