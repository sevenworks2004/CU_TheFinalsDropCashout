using System;
using System.Collections.Generic;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using CUCoreLib.Networking;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Collections;
using Newtonsoft.Json;
using KrokoshaCasualtiesMP;
using LiteNetLib;
using LiteNetLib.Utils;
using HarmonyLib;


public class Cashout : MonoBehaviour
{
    private UsableObject usable;
    private AudioSource audioSourceAlaram;
    private AudioSource audioSourceMusic;
    private AudioSource audioSourceInsert;
    private AudioSource audioSourceFinished;
    private float volume = 1f;
    private BuildingEntity objBuildingEntity;
    private BoxCollider2D customCol;
    private BoxCollider2D orgCol;
    public bool isInsertCashbox = false;
    private bool isPlayInsert = true;
    private bool isPlayMusic = false;

    #if DEBUG
        public float timerCashout = 30;
        private float printTimer = 2;
    #else 
        public float timerCashout = 60;
    #endif

    public bool isFinised = false;
    private Rigidbody2D rigBody;
    private static string[] listGoodHappiness = new[]
    {
        // "Go, go, go!",
        // "Let's go!",
        // "Get pumped!",
        // "Let's roll!",
        // "Bring it on!",
        // "We got this!",
        // "Fired up!",
        // "Jackpot! Let's vanish!",
        "Hold the line, we're rich!",
        "Oh. Nice.",
        "This is good.",
        "I like this.",
        "Okay, I like it.",
        "Well, that's nice.",
        "That's pretty good.",
        "I really like this.",
        "Oh, this is nice!",
        "Wow, I like this!",
        "This feels good.",
        "Now that's better!",
        "This is getting good!",
        "I like where this is going!",
        "Oh, this is going well!",
        "Wow, this is great!",
        "Okay, this is awesome!",
        "I love this!",
        "Wow! This is amazing!",
        "This is exactly what I wanted!",
        "Now we're talking!",
        "Oh, yeah! I like this!",
        "Wow... this feels great!",
        "Everything is looking good!",
        "This is wonderful!",
        "Wow! This is perfect!",
    };

    private static string[] listAlmostGoodHappiness = new[]
    {
        "Huh... not bad.",
        "Okay... that's actually good.",
        "Wait... is this working?",
        "Well... that's better.",
        "Oh. I didn't expect that.",
        "Okay, I'm starting to like this.",
        "Wait... this is actually going well.",
        "Wow... I haven't seen this before.",
        "Hold on... are we actually winning?",
        "Okay... I'm impressed.",
        "I can work with this.",
        "Maybe today isn't so bad.",
        "Okay... now that's something.",
        "Wow. Things are finally looking up.",
        "No way... is this really happening?",
        "Wow... that's new.",
        "Not terrible.",
        "Well... that's something.",
        "I guess that's good.",
        "Huh. Didn't expect that.",
        "Okay... interesting.",
        "Wait... are things getting better?",
        "Well, that's a pleasant surprise.",
        "Hold on... this is actually good.",
        "Okay... I'm starting to feel better.",
    };

    private float minDistationSound = 15f;
    private float maxDistationSound = 35f;
    private float happineAdd = 30f;
    private float happineLimit = 50;

    private int countEnemy = 3;
    private float radiusMinEnemySpawn = 5f; // Минимальная дистанция от кашаута
    private float raduisEnemuSpawn = 20f;   // Максимальная дистанция
    private string nameEnemy = "overgrowntick";

    #if DEBUG
    private double randomChanceExplide = 0.030;
    #else
    private double randomChanceExplide = 0.015;
    #endif
    private double timerRandomCachceExplide = 3;
    private System.Random random = new System.Random();
    public bool isCashoutActiveExplod;
    public float timerActiveExplod = 3;
    private string usableString = "Insert Cashbox";


    [Header("Item Drop Settings")]
    private int maxSpawnItems = 5;
    private int miniSpawnItems = 2;
    
    // Настройки ценности и шансов
    private int minValueFilter = 11;
    private int maxValueFilter = 50;
    
    // Граница разделения на обычные и редкие (например, всё что дороже 25 — редкое, от 0 до 25 — обычное)
    private int rareThresholdValue = 25;
    
    [Range(0f, 1f)]
    private float rareItemChance = 0.2f; // 20% шанс выпадения редкого предмета вместо обычного
    
    private static string channelId = "Cashout.channe;";
    public int indexCashout;
    private void Awake()
    {
        AudioClip clipMusic = AssetLoader.LoadEmbeddedAudio("Assets.cashout.sound.CashoutMusic.wav");
        AudioClip clipAlaram = AssetLoader.LoadEmbeddedAudio("Assets.cashout.sound.Alaram.wav");
        AudioClip clipInsert = AssetLoader.LoadEmbeddedAudio("Assets.cashout.sound.insert.wav");
        AudioClip clipFinished = AssetLoader.LoadEmbeddedAudio("Assets.cashout.sound.Finished.wav");
        
        AssetLoader.LoadFrameAnimationFromEmbeddedResources(
            "cashout.frame.insert",
            new []
            {
                "Assets/cashout/Frame/insert/0001.png",
                "Assets/cashout/Frame/insert/0002.png",
                "Assets/cashout/Frame/insert/0003.png",
                "Assets/cashout/Frame/insert/0004.png",
                "Assets/cashout/Frame/insert/0005.png",
                "Assets/cashout/Frame/insert/0006.png",
                "Assets/cashout/Frame/insert/0007.png",
                "Assets/cashout/Frame/insert/0008.png",
                "Assets/cashout/Frame/insert/0009.png",
            },
            16f,
            12,
            false
        );

        AssetLoader.LoadFrameAnimationFromEmbeddedResources(
            "cashout.frame.disappears",
            new []
            {
                "Assets/cashout/Frame/disappears/0001.png",
                "Assets/cashout/Frame/disappears/0002.png",
                "Assets/cashout/Frame/disappears/0003.png",
                "Assets/cashout/Frame/disappears/0004.png",
                "Assets/cashout/Frame/disappears/0005.png",
                "Assets/cashout/Frame/disappears/0006.png",
                "Assets/cashout/Frame/disappears/0007.png",
                "Assets/cashout/Frame/disappears/0008.png",
                "Assets/cashout/Frame/disappears/0009.png",
            },
            16f,
            12,
            false
        );
        
        audioSourceMusic = gameObject.AddComponent<AudioSource>();
        audioSourceMusic.clip = clipMusic;
        audioSourceMusic.loop = true;
        audioSourceMusic.volume = volume;
        audioSourceMusic.minDistance = minDistationSound;
        audioSourceMusic.maxDistance = maxDistationSound;
        audioSourceMusic.spatialBlend = 1f;
        audioSourceMusic.pitch = 0.7f;
        StartCoroutine(GlitchSound.MicroLoop(audioSourceMusic));

        audioSourceAlaram = gameObject.AddComponent<AudioSource>();
        audioSourceAlaram.clip = clipAlaram;
        audioSourceAlaram.volume = volume;
        audioSourceAlaram.loop = false;
        audioSourceAlaram.minDistance = minDistationSound;
        audioSourceAlaram.maxDistance = maxDistationSound;
        audioSourceAlaram.spatialBlend = 1f;

        audioSourceInsert = gameObject.AddComponent<AudioSource>();
        audioSourceInsert.clip = clipInsert;
        audioSourceInsert.volume = volume;
        audioSourceInsert.loop = false;
        audioSourceInsert.minDistance = minDistationSound;
        audioSourceInsert.maxDistance = maxDistationSound;
        audioSourceInsert.spatialBlend = 1f;
        audioSourceInsert.pitch = 0.9f;

        audioSourceFinished = gameObject.AddComponent<AudioSource>();
        audioSourceFinished.clip = clipFinished;
        audioSourceFinished.volume = volume;
        audioSourceFinished.loop = false;
        audioSourceFinished.minDistance = minDistationSound;
        audioSourceFinished.maxDistance = maxDistationSound;
        audioSourceFinished.spatialBlend = 1f;
        audioSourceFinished.pitch = 0.7f;
        
        objBuildingEntity = gameObject.GetComponent<BuildingEntity>();
        
        rigBody = gameObject.GetComponent<Rigidbody2D>();
        rigBody.mass = 120f;

        orgCol = gameObject.GetComponent<BoxCollider2D>();

        customCol = gameObject.AddComponent<BoxCollider2D>();
        customCol.isTrigger = true;
        
        customCol.size = gameObject.GetComponent<BoxCollider2D>().size;

        // usable = gameObject.AddComponent<UsableObject>();
        // usable.toggleString = usableString;
        // usable.didLangString = true;
 
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (KrokoshaScavMultiplayer.is_client) return;
        if (collider.TryGetComponent<Cashbox>(out Cashbox cashbox))
        {
            if (isInsertCashbox) return;
            #if DEBUG
            Console.WriteLine($"Detectd {cashbox.name}");
            ConsoleScript.instance.LogToConsole($"Detectd {cashbox.name}");
            #endif
            Destroy(cashbox.gameObject);
            InsertAnimationAndLogical();
        }
    }


    public void OnUse()
    {
        // Body body = PlayerCamera.main?.body;
        // if (body)
        // {
        //     if (CUCoreUtils.tryGetHeldItem(out Item held) ){
        //         if ((CUCoreUtils.HasEquipped("cashbox(Clone)") || CUCoreUtils.HasEquipped("cashbox")) && (held.name.Contains("cashbox(Clone)") || held.name.Contains("cashbox")))
        //         {
        //             Destroy(held.gameObject);
        //             InsertAnimationAndLogical();
        //         }
        //     }
        // }
    }
    private bool isFinised_ = false;
    private bool IsExplodeActive_ = false;
    public void Update()
    {
        // Client data
        if (KrokoshaScavMultiplayer.is_client && KrokoshaScavMultiplayer.network_system_is_running){
            if (isInsertCashbox && !isPlayMusic)
            {
                if (isPlayInsert)
                {
                    audioSourceInsert.Play();
                    audioSourceMusic.Play();
                    audioSourceMusic.volume = 0.7f;
                    isPlayInsert = false;
                    animationInser();
                    AttentionSays();
                }
                if (!audioSourceInsert.isPlaying)
                {
                    audioSourceMusic.volume = volume;
                    isPlayMusic = true;
                    #if DEBUG
                    Console.WriteLine("Spawn Enemy Moster Disable for Debug");
                    #else
                    spawnEnemyMonster();
                    #endif
                }
            }
            if (isCashoutActiveExplod && !IsExplodeActive_){
                IsExplodeActive_ = true;
                BadFeeling();
            }
            if (isInsertCashbox && !isFinised)
            {
                #if DEBUG
                printTimer -= Time.deltaTime;
                if (printTimer < 0)
                {
                    Console.WriteLine($"[Client] Left timer cashout: {(int)timerCashout }");
                    printTimer = 3;
                }
                #endif
            }
            if (isFinised && !isFinised_){
                isFinised_ = true;
                audioSourceMusic.Stop();
                rigBody.bodyType = RigidbodyType2D.Static;
                orgCol.enabled = false;
                animationDisappears();
                audioSourceFinished.Play();
            }
            return;
        }


        // server Data
        if (isInsertCashbox && !isPlayMusic)
        {
            if (isPlayInsert)
            {
                audioSourceInsert.Play();
                audioSourceMusic.Play();
                audioSourceMusic.volume = 0.7f;
                isPlayInsert = false;
                animationInser();
                AttentionSays();
            }
            if (!audioSourceInsert.isPlaying )
            {
                audioSourceMusic.volume = volume;
                isPlayMusic = true;
                #if DEBUG
                Console.WriteLine("Spawn Enemy Moster Disable for Debug");
                #else
                spawnEnemyMonster();
                #endif
            }
        }
        
        if (isInsertCashbox && !isFinised)
        {
            if (!isCashoutActiveExplod) timerCashout -= Time.deltaTime;
            #if DEBUG
            printTimer -= Time.deltaTime;
            if (printTimer < 0)
            {
                Console.WriteLine($"[Server] Left timer cashout: {(int)timerCashout }");
                printTimer = 3;
            }

            #endif
            timerRandomCachceExplide -= Time.deltaTime;
            if ( timerRandomCachceExplide <= 0 && timerCashout > timerActiveExplod && !isCashoutActiveExplod)
            {
                var ranEx = random.NextDouble();
                if (ranEx < randomChanceExplide)
                {
                    isCashoutActiveExplod = true;
                    #if DEBUG
                    Console.WriteLine("Cashout Explosion Activate");
                    #endif
                    BadFeeling();
                }
                else
                {
                    timerRandomCachceExplide = 3;
                }
            }
            if (isCashoutActiveExplod)
            {
                timerActiveExplod -= Time.deltaTime;
                #if DEBUG
                Console.WriteLine($"Explosion: {timerActiveExplod} ");
                #endif
                if (timerActiveExplod <= 0)
                {
                    WorldGeneration.CreateExplosion(new ExplosionParams{
                        position = transform.position,
                    });
                    Destroy(gameObject);
                }
            }

            if (timerCashout <= 0)
            {
                audioSourceMusic.Stop();
                isFinised = true;
                rigBody.bodyType = RigidbodyType2D.Static;
                orgCol.enabled = false;
                animationDisappears();
                UpHappinessAndSay();
                audioSourceFinished.Play();
                RandomBugFlayUpCashout();
                SpawnRandomItemObject();
            }
        }
        
        
        if (isFinised){
            audioSourceInsert.Stop();
            audioSourceMusic.Stop();
            if (!audioSourceFinished.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
        }

    }

    private void BadFeeling()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            maxDistationSound
        );
        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.TryGetComponent<Body>(out Body body)) continue;
            if (!body.alive) continue;

            MoodleRegistry.AddMoodle(
                1,
                AssetLoader.LoadEmbeddedSprite("Assets.ImpendingDoom_Moodle.png",128),
                "ImpendingDoom_Moodle",
                null,
                false,
                false,
                false,
                null,
                timerActiveExplod += 0.3f
            );
            return;
        
        }

    }

    private void RandomBugFlayUpCashout()
    {
        if (! (new System.Random().NextDouble() < 0.0015)) return;
        if ((transform.rotation.z > 0.5f || transform.rotation.z < -0.5f) && (rigBody.velocity.y < -0.1f || rigBody.velocity.y < 0.1f ))
        {
            transform.position += new Vector3(0,4.5f);
        }
    }

    private void spawnEnemyMonster()
    {
        #if DEBUG
        Console.WriteLine("Spawn Enemy");
        #endif
        for (var i = 0; i < countEnemy; i++)
        {
            Vector3 currentPosition = transform.position;
            
            // Генерируем случайную точку в кольце между radiusMinEnemySpawn и raduisEnemuSpawn
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            float randomDistance = UnityEngine.Random.Range(radiusMinEnemySpawn, raduisEnemuSpawn);
            Vector3 randomPoint = currentPosition + new Vector3(randomDirection.x * randomDistance, randomDirection.y * randomDistance, 0f);

            var cols = Physics.OverlapSphere(currentPosition, raduisEnemuSpawn);

            if (cols.Length > 0)
            {
                #if DEBUG
                foreach(var col in cols )
                {
                    Console.WriteLine($"Spawn Fail postition : {randomPoint} > {col.gameObject.name}");
                }
                #endif

                i -= 1;
                continue;
            }
            #if DEBUG
            Console.WriteLine($"Spawn Positions : {randomPoint}");
            #endif

            CustomInstantiate.InstantiateReturn(
                nameEnemy,
                randomPoint,
                Quaternion.identity,
                1f
            );
        }
    }

    private void animationInser()
    {
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        #if DEBUG
        if (AssetLoader.TryApplyAnimation(spriteRenderer, "cashout.frame.insert"))
        {
            Console.WriteLine("Play Animation: 'cashout.frame.insert'");
        }
        else
        {
            Console.WriteLine("Error Play Animation 'cashout.frame.insert'");
        }
        #else
        AssetLoader.TryApplyAnimation(spriteRenderer, "cashout.frame.insert");
        #endif
    }

    private void AttentionSays()
    {
        if (MultiplayerApi.IsClient && KrokoshaScavMultiplayer.network_system_is_running) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            maxDistationSound
        );
        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.TryGetComponent<Body>(out Body body)) continue;
            if (!body.alive) continue;
            if (!KrokoshaScavMultiplayer.network_system_is_running){
                using (CCLBody.Use(body))
                {
                    body.talker.Talk("...!");
                }
                continue;
            }
            foreach (var netBody in NetBody.all_instances){
                if (netBody.body.name == body.name){
                    netBody.body.talker.Talk("..?");
                    Console.WriteLine($"Talker user : {netBody.body.name}");
                }
            }
        }
    }

    private void animationDisappears()
    {
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        #if DEBUG
        if (AssetLoader.TryApplyAnimation(spriteRenderer, "cashout.frame.disappears"))
        {
            Console.WriteLine("Play Animation: 'cashout.frame.disappears'");
        }
        else
        {
            Console.WriteLine("Error Play Animation 'cashout.frame.disappears'");
        }
        #else
        AssetLoader.TryApplyAnimation(spriteRenderer, "cashout.frame.disappears");
        #endif

    }

    private void UpHappinessAndSay()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            maxDistationSound
        );
        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject || col.gameObject.name.Equals(nameEnemy)) continue;
            if (col.TryGetComponent<Body>(out Body body))
            {
                using (CCLBody.Use(body))
                {
                    if (!body.alive) continue;
                    if (body.totalBleedSpeed > 0.3) continue;
                    
                    if (body.happiness < happineLimit)
                    {
                        body.happiness += Mathf.Max(body.happiness + happineAdd,happineAdd);
                    }

                    if ( (body.happiness > -5) && (body.averagePain > 5f)) CUCoreUtils.talk(listGoodHappiness[UnityEngine.Random.Range(0,listGoodHappiness.Length)]);
                    else  CUCoreUtils.talk(listAlmostGoodHappiness[UnityEngine.Random.Range(0,listAlmostGoodHappiness.Length)]);
                }
                
            }
        }
    } 

    private void InsertAnimationAndLogical(){
        isInsertCashbox = true;
        customCol.enabled = false;
        // usable.enabled = false;
    }

        private void SpawnRandomItemObject()
    {
        var listCommonItems = new List<string>();
        var listRareItems = new List<string>();

        foreach (var i in Item.GlobalItems)
        {
            // Фильтруем предметы по общему диапазону ценности (от 0 до 50)
            if (i.Value.value < minValueFilter || i.Value.value > maxValueFilter) continue;

            // Распределяем на обычные и редкие
            if (i.Value.value >= rareThresholdValue)
            {
                listRareItems.Add(i.Key);
            }
            else
            {
                listCommonItems.Add(i.Key);
            }
        }

        if (listCommonItems.Count == 0 && listRareItems.Count == 0)
        {
            #if DEBUG
            ConsoleScript.instance.LogToConsole("Error: No items found in the specified value range!");
            #endif
            return;
        }
        var count = UnityEngine.Random.Range(miniSpawnItems,maxSpawnItems);
        for (var i = 0; i < count; i++)
        {
            string itemSpawn = null;

            // Определяем, спавнить редкий или обычный предмет на основе шанса
            bool rollRare = UnityEngine.Random.Range(0f, 1f) < rareItemChance;

            if (rollRare && listRareItems.Count > 0)
            {
                itemSpawn = listRareItems[UnityEngine.Random.Range(0, listRareItems.Count)];
            }
            else if (listCommonItems.Count > 0)
            {
                itemSpawn = listCommonItems[UnityEngine.Random.Range(0, listCommonItems.Count)];
            }
            else if (listRareItems.Count > 0)
            {
                // Если обычных нет, но есть редкие — берем редкий
                itemSpawn = listRareItems[UnityEngine.Random.Range(0, listRareItems.Count)];
            }

            if (!string.IsNullOrEmpty(itemSpawn))
            {
                var objItem = CustomInstantiate.InstantiateReturn(
                    itemSpawn,
                    transform.position + new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)),
                    Quaternion.identity
                );
                if (objItem != null)
                {
                    objItem.AddComponent<FreshItemDrop>();
                }
            }
        }
    }
}

public class GlitchSound
{
    public static IEnumerator MicroLoop(AudioSource source)
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 3f));

            int loopSamples = Mathf.RoundToInt(source.clip.frequency * 0.08f);

            // Сколько раз повторить кусок
            int repeats = UnityEngine.Random.Range(6,15);

            // Запоминаем текущую позицию
            int startSample = source.timeSamples - loopSamples;

            if (startSample < 0)
                startSample = 0;

            for (int i = 0; i < repeats; i++)
            {
                source.timeSamples = startSample;

                // Даём музыке проиграть маленький кусок
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}