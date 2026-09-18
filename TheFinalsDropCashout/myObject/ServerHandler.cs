

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CUCoreLib.Helpers;
using CUCoreLib.Networking;
using CUCoreLib.Registries;
using HarmonyLib;
using KrokoshaCasualtiesMP;
using KrokoshaCasualtiesUtils;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements.Collections;

public class ServerHandler
{
    // private const int ClientToServerMsg = (int)NetmsgId.CLIENT_ServerCustomCommand;
    // private const int ServerToClientMsg = (int)NetmsgId.SERVER_ClientCustomCommand;
    private const string IdCommandCashout = "cashout_status";
    private const string IdCommandSuspendCashoutOpen = "suspendCashout_open";
    private const string IdCommandSuspendCashoutCollision = "suspendCashout_collision";

    private static Thread loopThread;
    private static bool isRunningLoop = false;

    public static void initServerHandler()
    {
        var handler = new ServerHandler();
        handler.RegisterHandlers();

        ConsoleCommandRegistry.Register(
            "aaa",
            "aaa",
            (action) =>
            {
                // ServerMain
            }
        );

        if (!isRunningLoop)
        {
            isRunningLoop = true;
            loopThread = new Thread(handler.ServerBroadcastLoop);
            loopThread.Start();
        }
    }
    private void RegisterHandlers()
    {
        var serverDictField = AccessTools.Field(typeof(ServerMain), "ServerClientCustomCommandsDict");
        if (serverDictField?.GetValue(null) is IDictionary serverDict)
        {
            serverDict[IdCommandSuspendCashoutOpen] = new Action<NetPlayer, string, string[]>(ServerRequestHandlerSuspendCashout);
            serverDict[IdCommandSuspendCashoutCollision] = new Action<NetPlayer,string,string[]>(ServerRequestHandlerSuspendCashoutCollision);
        }

        var clientDictField = AccessTools.Field(typeof(ClientMain), "ClientCustomCommandsDict");
        if (clientDictField?.GetValue(null) is IDictionary clientDict)
        {
            clientDict[IdCommandCashout] = new Action<string, NetDataReader>(ClientResponseHandler);
        }
    }



    private void ServerBroadcastLoop()
    {
        while (isRunningLoop)
        {
            if (KrokoshaScavMultiplayer.network_system_is_running && KrokoshaScavMultiplayer.is_server)
            {
                if (Util.IsWorldGenerated())
                try
                {
                    foreach (var obj in NetObjectRegistry.NetIdToSyncInfoDict)
                    {
                        NetDataWriter writer = Net.CreateWriter(NetmsgId.CLIENT_ServerCustomCommand);
                        writer.Put(IdCommandCashout);
                        if (obj.Value.go.TryGetComponent<Cashout>(out Cashout cashout))
                            {
                            var data = new DataStatusCashout {
                                id = obj.Key.id,
                                timerCashout = cashout.timerCashout,
                                isFinised = cashout.isFinised,
                                isInsertCashbox = cashout.isInsertCashbox,
                                isCashoutActiveExplod = cashout.isCashoutActiveExplod,
                                timerActiveExplod = cashout.timerActiveExplod
                            };
                            var dataJect = JObject.FromObject(data);
                            writer.Put(dataJect.ToString());
                            foreach (var netBody in NetBody.all_instances)
                            {
                                if (NetPlayer.LOCAL_PLAYER.body.name == netBody.body.name) continue;
                                Net.Server_SendTo(DeliveryMethod.ReliableOrdered, in writer,netBody.netId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ServerHandler Loop Error] {ex.Message}");
                }
                Thread.Sleep(800);
            }
        }
    }

    private void ClientResponseHandler(string arg1, NetDataReader reader)
    {
        string data = reader.GetString();
        DataStatusCashout status = JObject.Parse(data).ToObject<DataStatusCashout>();
        foreach (var obj in NetObjectRegistry.NetIdToSyncInfoDict)
        {
            try
            {
                if (!obj.Value.go.TryGetComponent<Cashout>(out Cashout cashout)) continue;
                if (status.id != obj.Key.id) continue;
                cashout.timerCashout = status.timerCashout;
                cashout.timerActiveExplod = status.timerActiveExplod;
                cashout.isInsertCashbox = status.isInsertCashbox;
                cashout.isFinised = status.isFinised;
                cashout.isCashoutActiveExplod = status.isCashoutActiveExplod;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void ServerRequestHandlerSuspendCashout(NetPlayer player, string arg2, string[] arg3)
    {
        Console.WriteLine($"Open Suspend Cashout id {arg3[1]}");
        if (!ushort.TryParse(arg3[1],out ushort id)) throw new TypeAccessException("id string to ushort Error");

        if (NetObjectRegistry.NetIdToSyncInfoDict.TryGetValue(new knetid {id = id},out SyncInfo v))
        {
            if (!v.go.TryGetComponent<SuspendedCashBox>(out SuspendedCashBox suspendedCashBox))
            {
                Console.WriteLine("Error Not 'SuspendedCashBox' companet");
                return;
            }
            suspendedCashBox.openCashbox();
            return;
        }
        Console.WriteLine("Error id find");
    }

    internal static bool SuspendCashoutOpenClient(SuspendedCashBox suspendedCashBox)
    {
        foreach (var obj in NetObjectRegistry.NetIdToSyncInfoDict){
            if (obj.Value.go.TryGetComponent<SuspendedCashBox>(out SuspendedCashBox suspendedCashBox1))
            {
                if (suspendedCashBox == suspendedCashBox1)
                {
                    NetDataWriter writer = Net.CreateWriter(NetmsgId.SERVER_ClientCustomCommand);
                    writer.Put($"{IdCommandSuspendCashoutOpen} {obj.Key.id}");
                    Console.WriteLine($"Send server Open id: {obj.Key.id}");
                    Net.Client_Send(DeliveryMethod.ReliableOrdered,writer);
                    return true;
                }
            }
        }
        Console.WriteLine("Error object 'suspendedCashBox' Not Found");
        return false;
    }
    private void ServerRequestHandlerSuspendCashoutCollision(NetPlayer player, string arg2, string[] arg3)
    {
        var id = int.Parse(arg3[1]);
        var isHead = bool.Parse(arg3[2]);
        Console.WriteLine($"Player: {player.body.name} Cashbox id : {id} isHead : {isHead}");
        var obj = NetObjectRegistry.NetIdToSyncInfoDict.Get(new knetid{id = (ushort)id});
        if (obj.go.TryGetComponent<Cashbox>(out Cashbox cashbox))
        {
            cashbox.isHand = isHead;
            cashbox.plr = player;
            if (!isHead)
            {
                cashbox.plr = null;
            }
            return;
        }
        Console.WriteLine("Error Not found");
        return;
    }
    public static void isHeadItemCashbox(Cashbox cashbox,NetPlayer plr)
    {
        foreach (var obj in NetObjectRegistry.NetIdToSyncInfoDict)
        {
            if (!obj.Value.go.TryGetComponent<Cashbox>(out Cashbox cashbox1))continue;

            if (cashbox1 == cashbox)
            {
                NetDataWriter writer = Net.CreateWriter(NetmsgId.SERVER_ClientCustomCommand);
                writer.Put($"{IdCommandSuspendCashoutCollision} {obj.Key.id} {cashbox.isHand}");
                Net.Client_Send(DeliveryMethod.ReliableSequenced,writer);
                return;
            }
        }
        Console.WriteLine("Error Object Cashout NotFound");
    }
}
class DataStatusCashout
{
    public int id;
    public float timerCashout;
    public bool isInsertCashbox;
    public bool isFinised;
    public bool isCashoutActiveExplod;

    public float timerActiveExplod;
}