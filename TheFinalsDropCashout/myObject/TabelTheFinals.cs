

using System;
using System.Collections.Generic;
using CUCoreLib;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using ModThefinalsDropCashout;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabelTheFinals : MonoBehaviour
{
    public static int intelect = 0;
    private NoteItem noteItem;
    private bool isRead = false;
    public static List<NoteItem> listHistory = new List<NoteItem>
    {
        new NoteItem
        {
            text = "I think it's time to say goodbye to the old sport and welcome THE FINALS, the biggest game show of the future! Become famous, style on your enemies and become the next big star! Can you reach THE FINALS?",
            
        },
        new NoteItem
        {
            text = "WHO. NEEDS. WATER? Try OSPUZE Pro Juice! A zero sugar energy drink, filled to the brim with the legal limit of performance-peaking stimulants, you won't ever need to sleep with this one! So POP. POUR. PERFORM."
        }
    };
    private int index;

    public void Awake()
    {
        AssetLoader.LoadFrameAnimationFromEmbeddedResources(
                "tabel.frame",
                new[]
                {
                    "Assets.tabel.Frame.0001.png",
                    "Assets.tabel.Frame.0002.png",
                    "Assets.tabel.Frame.0003.png",
                    "Assets.tabel.Frame.0004.png",
                    "Assets.tabel.Frame.0005.png",
                    "Assets.tabel.Frame.0006.png",
                    "Assets.tabel.Frame.0007.png",
                    "Assets.tabel.Frame.0008.png",
                    "Assets.tabel.Frame.0009.png",
                },
                23f,
                16,
                true
            );

        var renderSp = gameObject.GetComponent<SpriteRenderer>();
        if (renderSp != null)
        {
            AssetLoader.TryApplyAnimation(renderSp,"tabel.frame");
        }

        if (isAllReadHistory())
        {
            List<NoteItem> a = getListHistoryNotRead();
            index = UnityEngine.Random.Range(0,a.Count);
        }
        else
        {
            index = UnityEngine.Random.Range(0,listHistory.Count);
        }
        noteItem = listHistory[index];
    }

    private List<NoteItem> getListHistoryNotRead()
    {
        var ll = new List<NoteItem>();
        foreach(var noteItem in listHistory)
        {
            if (!noteItem.isRead)
            {
                ll.Add(noteItem);
            }
        }
        return listHistory;
    }

    private static bool isAllReadHistory()
    {
        foreach(var noteItem in listHistory)
        {
            if (!noteItem.isRead)
            {
                return false;
            }
        }
        return true;
    }
    public void Use()
    {
        if (!listHistory[index].isRead)
        {
            listHistory[index].isRead = true;
            intelect += 1;
        }
        PlayerCamera.main.SetTimeScale(PlayerCamera.SpeedType.Slowmo,switchSound:false);
        ScrollableText.CreateText(noteItem.text,false,null,null);
    }
}

[Serializable]
public class NoteItem
{
    public string text;
    public bool isRead = false;
}