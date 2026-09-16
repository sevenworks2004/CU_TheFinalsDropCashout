using System;
using CUCoreLib;
using CUCoreLib.Helpers;
using KrokoshaCasualtiesMP;
using KrokoshaCasualtiesUtils;
using UnityEngine;

class SuspendedCashBox: MonoBehaviour
{
    private UsableObject usable;
    private float pushCashbox = 15f;
    // private AudioSource audioOpenEffect;
    private float minDistationSound = 1;
    private float maxDistationSound = 5;
    private bool isOpen = false;
    private static string[] dialog = new[]
    {
        "What was that?"
    };
    private Rigidbody2D rigSuspCashbox;
    private AudioClip clip;

    private void Awake()
    {
        usable = gameObject.AddComponent<UsableObject>();
        usable.toggleString = "Open";
        usable.didLangString = true;

        rigSuspCashbox = gameObject.GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rigSuspCashbox.mass = 20f;

        clip = AssetLoader.LoadEmbeddedAudio(
            "Assets.suspebedCashbox.sound.Opend.wav"
        );
        // audioOpenEffect = gameObject.AddComponent<AudioSource>(); 
        // audioOpenEffect.clip = clip;
        // audioOpenEffect.spatialBlend = 1f;
        // audioOpenEffect.minDistance = minDistationSound;
        // audioOpenEffect.maxDistance = maxDistationSound;
        // audioOpenEffect.loop = false;
    }

    public void OnUse()
    {
        if (KrokoshaScavMultiplayer.is_client && KrokoshaScavMultiplayer.network_system_is_running)
        {
            ServerHandler.SuspendCashoutOpenClient(this);
        }
        openCashbox();
    }

    public void openCashbox()
    {
        if (isOpen) return;
        isOpen = true;
        if (clip != null) CUCoreUtils.playSoundAt(clip,transform.position);

        if (rigSuspCashbox != null) rigSuspCashbox.AddForce(Vector2.up * 1,ForceMode2D.Impulse);
        var col = gameObject.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rigSuspCashbox != null) Destroy(rigSuspCashbox);

        var cashboxObj = CustomInstantiate.InstantiateReturn(
            "cashbox",
            transform.position + new Vector3(0,1f),
            Quaternion.identityQuaternion
        );
        // cashboxObj.AddComponent<FreshItemDrop>();
        var rigCashbox = cashboxObj.GetComponent<Rigidbody2D>();

        if (rigCashbox != null)
        {
            rigCashbox.AddForce(
                Vector2.up * pushCashbox,
                ForceMode2D.Impulse
            );
        }
        else
        {
            #if DEBUG
            Console.WriteLine("Error Rigidboy2d ");
            #endif
        }
        if (gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer)){
            Destroy(renderer);
        }

        // Body body = PlayerCamera.main.body;
        // if (body != null)
        // {
        //     using (CCLBody.Use(body))
        //     {
        //         CUCoreUtils.talk(
        //             dialog[UnityEngine.Random.Range(0,dialog.Length)]
        //         );
        //     }
        // }
        Destroy(gameObject);
    }

}