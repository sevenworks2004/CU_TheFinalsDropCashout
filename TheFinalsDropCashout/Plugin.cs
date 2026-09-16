using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.ContentReload;
using CUCoreLib.Registries;
using CUCoreLib.Helpers;
using CUCoreLib.Data;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using System.Diagnostics;
using System.Threading;
using CUCoreLib;
using CUCoreLib.Networking;

namespace ModThefinalsDropCashout
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.TheFinals.drop";
        public const string ModName = "The Finals Drop Cashout";
        public const string ModVersion = "0.1.0";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        public static string bundleID = "bundle.game";

        void Awake()
        {
            
            Logger = base.Logger;
            Instance = this;
            // ContentReloadManager.EnableHotReload(ModGUID);
            itemCashboxInit();
            objectCashoutInit();
            objectSuspendedCashboxInit();
            itemTabelTheFinalInit();
            ServerHandler.initServerHandler();
            
            _harmony.PatchAll(typeof(Plugin).Assembly);
            #if DEBUG
            ConsoleCommandRegistry.Register(
                "getObjects",
                "Get all objects in range",
                Action =>
                {
                Body body = PlayerCamera.main?.body;
                if (body == null)
                {
                    ConsoleScript.instance.LogToConsole("Error Body");
                    return;
                }
                if (!int.TryParse(Action[1], out int maxDistationSound) )
                {
                    ConsoleScript.instance.LogToConsole("Error maxDistationSound");
                    return;
                }
                Collider2D[] colliders = Physics2D.OverlapCircleAll(
                        body.transform.position,
                        maxDistationSound
                    );
                    foreach (var col in colliders)
                    {
                        ConsoleScript.instance.LogToConsole(
                            $"Object: {col.gameObject.name} | Position: {col.gameObject.transform.position}"
                        );
                        Console.WriteLine(
                            $"Object: {col.gameObject.name} | Position: {col.gameObject.transform.position}"
                        );
                    }  
                }
            );
            ConsoleCommandRegistry.Register(
                "test",
                "test",
                Action =>
                {
                    if (MultiplayerApi.IsClient && MultiplayerApi.IsRunning)
                    {
                        Console.WriteLine("Error spawn test");
                        return;
                    }
                    var body = PlayerCamera.main?.body;

                    if (body == null)
                    {
                        ConsoleScript.instance.LogToConsole("Error Body");
                        return;
                    }
                    var ob = CustomInstantiate.InstantiateReturn(
                        "cashbox",
                        body.transform.position,
                        Quaternion.identity,
                        1f
                    );
                    if (ob == null)
                    {
                        ConsoleScript.instance.LogToConsole("Error Ob");
                        return;
                    }
                    Item it = ob?.GetComponent<Item>();
                    if (it == null)
                    {
                        ConsoleScript.instance.LogToConsole("Error it");
                        return;
                    }
                    body.PickUpItem(it,0,true);


                    BuildingEntityRegistry.Spawn(
                        "cashout",
                        body.transform.position += Vector3.up,
                        Quaternion.identity
                    );

                    BuildingEntityRegistry.Spawn(
                        "suspebedCashbox",
                        body.transform.position,
                        Quaternion.identity
                    );
                }
            );
            #endif
        }
        private void objectSuspendedCashboxInit()
        {
            Sprite suspebedCashboxSprite = AssetLoader.LoadEmbeddedSprite(
                "Assets.suspebedCashbox.suspebedCashbox.png",
                32f
            );

            BuildingEntityRegistry.Register(
                "suspebedCashbox",
                new CustomBuildingEntityDefinition
                {
                    Name = "suspebedCashbox",
                    Description = "suspebedCashbox",
                    Sprite = suspebedCashboxSprite,
                    Health = 500,
                    AddRigidbody2D = true,
                    Metallic = true,
                    HitSoundReferenceId = "metal",
                    Placement = BuildingPlacementType.Floor,
                    RigidbodyBodyType = RigidbodyType2D.Dynamic,
                    SurfaceOffset = 2f,
                    RequireGround = true,
                    LayerEnum = BuildingLayer.Ground,
                    RigidbodyGravityScale = 2f,
                    DamagePlayerOnImpact = true,
                    Components = new[]
                    {
                        typeof(SuspendedCashBox)
                    },
                    ItemsDropOnDestroy = [
                        BuildingEntityRegistry.AddDrop("scrapmetal",3),
                    ],
                    GenerationStyle = BuildingGenerationStyle.Standard,
                    SpawnMinPerChunk = 0.01f,
                    SpawnMaxPerChunk = 0.03f,
                    SpawnLayers = BuildingEntityRegistry.AllLayersExcept()
                }
            );
        }

        void itemCashboxInit()
        {
            Sprite cashboxSprite = AssetLoader.LoadEmbeddedSprite(
                "Assets.cashbox.cashbox.png",
                64f
            );
            var customItem = new CustomItemInfo
            {
                fullName = "cashbox",
                description = "cashbox description",
                category = "tools",
                weight = 3.0f,
                tags = "tag cashbox",
                onlyHoldInHands = true,
                ignoreDepression = true,

            };
            customItem.AddSpawnComponent<Cashbox>();
            
            ItemRegistry.Register(
                "cashbox",
                customItem,
                cashboxSprite
            );
        }
        void objectCashoutInit()
        {
            Sprite cashoutSprite = AssetLoader.LoadEmbeddedSprite(
                "Assets.cashout.cashout.png",
                16f
            );
            BuildingEntityRegistry.Register(
                "cashout",
                new CustomBuildingEntityDefinition
                {
                    Name = "Cashout",
                    Description = "Cashout terminal",
                    Sprite = cashoutSprite,
                    Health = 20000,
                    AddRigidbody2D = true,
                    Metallic = true,
                    HitSoundReferenceId = "metal",
                    Placement = BuildingPlacementType.Floor,
                    RigidbodyBodyType = RigidbodyType2D.Dynamic,
                    SurfaceOffset = 0.5f,
                    SpawnInGround = true,
                    RequireGround = false,
                    LayerEnum = BuildingLayer.Ground,
                    RigidbodyGravityScale = 2f,
                    DamagePlayerOnImpact = true,
                    Components = new []
                    {
                        typeof(Cashout)
                    },
                    ItemsDropOnDestroy = [
                        BuildingEntityRegistry.AddDrop("scrapmetal",3),
                    ],
                    GenerationStyle = BuildingGenerationStyle.DropPod,
                    SpawnMinPerChunk = 0.01f,
                    SpawnMaxPerChunk = 0.02f,
                    SpawnLayers = BuildingEntityRegistry.AllLayersExcept(),
                    
                }
            );
        }

        void itemTabelTheFinalInit()
        {
            // if (AssetLoader.RegisterBundleFromPluginFolder(this,bundleID,"bundles/game") )
            // {
            //     Console.WriteLine("Load bundle OK");
            // }
            // else
            // {
            //     Console.WriteLine("Error Bundle load");
            // }
            Sprite knowledgeTheFinals_sprite = AssetLoader.LoadEmbeddedSprite(
                "Assets.tabel.tabel.png",
                50f
            );
            var customItem = new CustomItemInfo
            {
                fullName = "tabletTheFinals",
                description = "tablet",
                category = "tools",
                weight = 3.0f,
                usable = true,
                tags = "tag knowledgeTheFinals",
                ignoreDepression = true,
                useAction = (body, item) =>
                {
                    var table = item.GetComponent<TabelTheFinals>();
                    if (table != null ) table.Use();
                }
            };
            customItem.AddSpawnComponent<TabelTheFinals>();

            ItemRegistry.Register(
                "tabletTheFinals",
                customItem,
                knowledgeTheFinals_sprite
            );
        }
    }
}
