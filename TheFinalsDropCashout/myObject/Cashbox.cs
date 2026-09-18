using CUCoreLib.Helpers;
using UnityEngine;
using CUCoreLib.Networking;
using System;
using KrokoshaCasualtiesMP;

public class Cashbox : MonoBehaviour
{
    public bool isHand = false;
    private bool isAnowers = false;
    private string[] AnowersDialog = new[]
    {
        "hahaha,cashbox me"
    };
    private Rigidbody2D rig;
    private DamagingCrate damg;
    private Item item;
    private float collisionActivationDelay = 0.3f;
    bool isChange = false;
    public NetPlayer plr;
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
        item = gameObject.GetComponent<Item>();
        
    }
    public void Update()
    {
        Body body = PlayerCamera.main?.body;
        if (KrokoshaScavMultiplayer.is_client && KrokoshaScavMultiplayer.network_system_is_running)
        {
            NetPlayer player = NetPlayer.LOCAL_PLAYER;

            if (body != null && body.HoldingItem(item) && body.alive)
            {
                isHand = true;
            }
            else
            {
                isHand = false;
            }
            if (isHand && !isChange)
            {
                isChange = true;
                ServerHandler.isHeadItemCashbox(this, NetPlayer.LOCAL_PLAYER);
            }
            else if (!isHand && isChange)
            {
                isChange = false;
                ServerHandler.isHeadItemCashbox(this, NetPlayer.LOCAL_PLAYER);
            }
            if (rig != null && damg != null)
            {
                if (isHand)
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
                    else
                    {
                        collisionActivationDelay -= Time.deltaTime;
                    }
                }
            }
            return;
        }

        if ((body != null && body.alive && body.HoldingItem(item)) || plr != null)
        {
            isHand = true;
        }
        else
        {
            isHand = false;
        }

        if (rig != null && damg != null)
        {
            if (isHand)
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
                else
                {
                    collisionActivationDelay -= Time.deltaTime;
                }
            }
        }
    }
}