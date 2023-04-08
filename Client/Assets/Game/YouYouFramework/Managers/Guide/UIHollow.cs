using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class UIHollow : UIFormBase
{
    private LinkedListNode<Transform> CurrGuide;
    private LinkedList<Transform> CurrGuides = new LinkedList<Transform>();
    private Transform itemParent;

    public GuideState GuideState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform item in transform)
        {
            item.gameObject.SetActive(false);
        }
    }
    public void SetUI(GuideState descGroup)
    {
        GuideState = descGroup;

        if (itemParent != null)
        {
            itemParent.gameObject.SetActive(false);
            itemParent = null;
        }

        CurrGuides.Clear();
        itemParent = transform.Find(GuideState.ToString());
        if (itemParent == null)
        {
            YouYou.GameEntry.LogError("itemParent==null, descGroup==" + GuideState);
        }
        itemParent.gameObject.SetActive(true);
        foreach (Transform item in itemParent)
        {
            item.gameObject.SetActive(false);
            CurrGuides.AddLast(item);
        }
        CurrGuide = CurrGuides.First;
        ShowGuide();

    }

    public void NextGroup()
    {
        CurrGuide.Value.gameObject.SetActive(false);
        CurrGuide = CurrGuide.Next;
        if (CurrGuide != null)
        {
            ShowGuide();
        }
    }

    private void ShowGuide()
    {
        GameEntry.Log(LogCategory.Hollow, "Enter=={0}=={1}=={2}", GameEntry.Guide.CurrentState, CurrGuide.Value.gameObject.name, GameEntry.Guide.GuideGroup.TaskGroup.CurrCount + 1);
        CurrGuide.Value.gameObject.SetActive(true);
    }

}
