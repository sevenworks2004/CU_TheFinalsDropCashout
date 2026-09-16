using CUCoreLib.Helpers;
using UnityEngine;
using CUCoreLib.Networking;
using System;
using KrokoshaCasualtiesMP;

public class Cashbox : MonoBehaviour
{
    private bool isHead = false;
    private bool isAnowers = false;
    private string[] AnowersDialog = new[]
    {
        "hahaha,cashbox me"
    };
    private Rigidbody2D rig;
    private DamagingCrate damg;
    private float collisionActivationDelay = 0.3f;
    private void Awake()
    {
        var sprRender = gameObject.GetComponent<SpriteRenderer>();
        AssetLoader.LoadFrameAnimationFromEmbeddedResources(
                "cashbox.frame",
                new[]
                {
                    "Assets.cashbox.Frame.0001.png",
                    "Assets.cashbox.Frame.0002.png",
                    "Assets.cashbox.Frame.0003.png",
                    "Assets.cashbox.Frame.0004.png",
                    "Assets.cashbox.Frame.0005.png",
                    "Assets.cashbox.Frame.0006.png",
                    "Assets.cashbox.Frame.0007.png",
                    "Assets.cashbox.Frame.0008.png",
                    "Assets.cashbox.Frame.0009.png",
                    "Assets.cashbox.Frame.0010.png",
                    "Assets.cashbox.Frame.0011.png",
                    "Assets.cashbox.Frame.0012.png",
                    "Assets.cashbox.Frame.0013.png",
                    "Assets.cashbox.Frame.0014.png",
                    "Assets.cashbox.Frame.0015.png",
                    "Assets.cashbox.Frame.0016.png",
                    "Assets.cashbox.Frame.0017.png",
                    "Assets.cashbox.Frame.0018.png",
                    "Assets.cashbox.Frame.0019.png",
                    "Assets.cashbox.Frame.0020.png",
                    "Assets.cashbox.Frame.0021.png",
                    "Assets.cashbox.Frame.0022.png",
                    "Assets.cashbox.Frame.0023.png",
                    "Assets.cashbox.Frame.0024.png",
                },
                32f,
                16,
                true
            );
        AssetLoader.TryApplyAnimation(sprRender, "cashbox.frame");

        gameObject.layer = LayerMask.NameToLayer("Ground");
        rig = gameObject.GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        damg = gameObject.GetComponent<DamagingCrate>() ?? gameObject.AddComponent<DamagingCrate>();
    }

    public void Update()
    {

        if (isHead && !isAnowers)
        {
            Body body = PlayerCamera.main?.body;
            #if DEBUG
            if (body == null)
            {
                ConsoleScript.instance.LogToConsole("Error Body");
                return;
            }
            #else
            if (body == null) return;
            #endif

            using (CCLBody.Use(body))
            {
                #if DEBUG
                ConsoleScript.instance.LogToConsole("cashbox isHead");
                #endif
                CUCoreUtils.talk(AnowersDialog[UnityEngine.Random.Range(0, AnowersDialog.Length)]);
                isAnowers = true;
            }
        }
        else if (isAnowers && !isHead){
            isAnowers = false;
        }
        if (CUCoreUtils.HasEquipped("cashbox(Clone)") || CUCoreUtils.HasEquipped("cashbox")){
            isHead = true;


        }
        else
        {
            isHead = false;
        }

        if (rig != null && damg != null)
        {
            if (isHead)
            {
                damg.enabled = false;
                gameObject.layer = 7;
                collisionActivationDelay = 0.3f;
            }
            else
            {
                if (collisionActivationDelay < 0.0f)
                {
                    damg.enabled = true;
                    gameObject.layer = 6;
                }
                else collisionActivationDelay -= Time.deltaTime;
            }
        }
        
    }
}