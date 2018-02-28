using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmpyrionMessageBroker
{
    public class APICall
    {
        public CmdId cmdId { get; private set; }

        public Type ArgType { get; private set; }

        public APIEvent ExpectedResponseEvent { get; private set; }


        public APICall(CmdId cmdId, Type argType, APIEvent expectedEvent) {
            this.cmdId = cmdId;
            this.ArgType = argType;
            this.ExpectedResponseEvent = expectedEvent;
        }

        public APICall(CmdId cmdId, Type argType, CmdId responseCmdId, Type responseType) : this(cmdId, argType, new APIEvent(responseCmdId, responseType)) { }

        public static KeyValuePair<CmdId, APICall> getKVP(CmdId requestCmdId, Type argType, CmdId responseCmdId, Type responseType)
        {
            var apiCall = new APICall(requestCmdId, argType, new APIEvent(responseCmdId, responseType));
            return new KeyValuePair<CmdId, APICall>(requestCmdId, apiCall);
        }
    }

    public class APIEvent
    {
        public CmdId responseCmdId { get; }
        public Type responseDataType { get; }

        public APIEvent(CmdId responseCmd, Type responseType = null)
        {
            this.responseCmdId = responseCmd;
            this.responseDataType = responseType;
        }

        public static KeyValuePair<CmdId, APIEvent> getKVP(CmdId eventCmdId, Type responseType)
        {
            var tmp = new APIEvent(eventCmdId, responseType);
            return new KeyValuePair<CmdId, APIEvent>(eventCmdId, tmp);
        }

    }

    public static class EmpyrionAPIContractManager
    {
        public static Dictionary<CmdId, APIEvent> UnsolicitedEvents = new List<KeyValuePair<CmdId, APIEvent>>()
        {
            APIEvent.getKVP(CmdId.Event_Playfield_Loaded, typeof(PlayfieldLoad)),
            APIEvent.getKVP(CmdId.Event_Playfield_Unloaded, typeof(PlayfieldLoad)),
            APIEvent.getKVP(CmdId.Event_Player_Connected, typeof(Id)),
            APIEvent.getKVP(CmdId.Event_Player_Disconnected, typeof(Id)),
            APIEvent.getKVP(CmdId.Event_Player_ChangedPlayfield, typeof(IdPlayfield)),
            APIEvent.getKVP(CmdId.Event_Player_DisconnectedWaiting, typeof(Id)),
            APIEvent.getKVP(CmdId.Event_Faction_Changed, typeof(FactionChangeInfo)),
            APIEvent.getKVP(CmdId.Event_Statistics, typeof(StatisticsType)), //validate this
            APIEvent.getKVP(CmdId.Event_ChatMessage, typeof(ChatInfo)),
            APIEvent.getKVP(CmdId.Event_TraderNPCItemSold, typeof(TraderNPCItemSoldInfo)),
            APIEvent.getKVP(CmdId.Event_ConsoleCommand, typeof(ConsoleCommandInfo)),
            APIEvent.getKVP(CmdId.Event_PdaStateChange, typeof(PdaStateInfo)),
            APIEvent.getKVP(CmdId.Event_GameEvent, typeof(GameEventData)), // what is this?

        }.ToDictionary(x => x.Key, x => x.Value);

        public static Dictionary<CmdId, APICall> APIContracts = new List<KeyValuePair<CmdId, APICall>>()
        {
            APICall.getKVP(CmdId.Request_Playfield_List, null, CmdId.Event_Playfield_List, typeof(PlayfieldList)),
            APICall.getKVP(CmdId.Request_Playfield_Stats, typeof(PString), CmdId.Event_Playfield_Stats, typeof(PlayfieldStats)),
            APICall.getKVP(CmdId.Request_Dedi_Stats, null, CmdId.Event_Dedi_Stats, typeof(DediStats)),
            APICall.getKVP(CmdId.Request_GlobalStructure_List, null, CmdId.Event_GlobalStructure_List, typeof(GlobalStructureList)),
            APICall.getKVP(CmdId.Request_GlobalStructure_Update, typeof(PString), CmdId.Event_Ok, null), // validate this
            APICall.getKVP(CmdId.Request_Structure_Touch, typeof(Id), CmdId.Event_Ok, null), // validate this
            APICall.getKVP(CmdId.Request_Structure_BlockStatistics, typeof(Id), CmdId.Event_Structure_BlockStatistics, typeof(IdStructureBlockInfo)),
            APICall.getKVP(CmdId.Request_Player_Info, typeof(Id), CmdId.Event_Player_Info, typeof(PlayerInfo)),
            APICall.getKVP(CmdId.Request_Player_List, null, CmdId.Event_Player_List, typeof(IdList)), 
            APICall.getKVP(CmdId.Request_Player_GetInventory, typeof(Id), CmdId.Event_Player_Inventory, typeof(Inventory)),
            APICall.getKVP(CmdId.Request_Player_SetInventory, typeof(Id), CmdId.Event_Player_Inventory, typeof(Inventory)), //validate this
            APICall.getKVP(CmdId.Request_Player_AddItem, typeof(IdItemStack), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Player_Credits, typeof(Id), CmdId.Event_Player_Credits, typeof(IdCredits)),
            APICall.getKVP(CmdId.Request_Player_SetCredits, typeof(IdCredits), CmdId.Event_Player_Credits, typeof(IdCredits)), //validate this
            APICall.getKVP(CmdId.Request_Player_AddCredits, typeof(IdCredits), CmdId.Event_Player_Credits, typeof(IdCredits)), //validate this
            APICall.getKVP(CmdId.Request_Blueprint_Finish, typeof(Id), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Blueprint_Resources, typeof(BlueprintResources), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Player_ChangePlayerfield, typeof(IdPlayfieldPositionRotation), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Player_ItemExchange, typeof(ItemExchangeInfo), CmdId.Event_Player_ItemExchange, typeof(ItemExchangeInfo)),
            APICall.getKVP(CmdId.Request_Player_SetPlayerInfo, typeof(PlayerInfoSet), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Entity_Teleport, typeof(IdPositionRotation), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Entity_ChangePlayfield, typeof(IdPlayfieldPositionRotation), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Entity_Destroy, typeof(Id), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Entity_PosAndRot, typeof(Id), CmdId.Event_Entity_PosAndRot, typeof(IdPositionRotation)),
            APICall.getKVP(CmdId.Request_Entity_Spawn, typeof(EntitySpawnInfo), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_Get_Factions, typeof(Id), CmdId.Event_Get_Factions, typeof(FactionInfoList)), // what is this?
            APICall.getKVP(CmdId.Request_NewEntityId, null, CmdId.Event_NewEntityId, typeof(Id)),
            APICall.getKVP(CmdId.Request_AlliancesAll, null, CmdId.Event_AlliancesAll, typeof(AlliancesTable)),
            APICall.getKVP(CmdId.Request_AlliancesFaction, typeof(AlliancesFaction), CmdId.Event_AlliancesFaction, typeof(AlliancesFaction)),
            APICall.getKVP(CmdId.Request_Load_Playfield, typeof(PlayfieldLoad), CmdId.Event_Ok, null), //validate this
            APICall.getKVP(CmdId.Request_ConsoleCommand, typeof(PString), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_GetBannedPlayers, null, CmdId.Event_BannedPlayers, typeof(IdList)),
            APICall.getKVP(CmdId.Request_InGameMessage_SinglePlayer, typeof(IdMsgPrio), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_InGameMessage_AllPlayers, typeof(IdMsgPrio), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_InGameMessage_Faction, typeof(IdMsgPrio), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_ShowDialog_SinglePlayer, typeof(IdMsgPrio), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_Player_GetAndRemoveInventory, typeof(Id), CmdId.Event_Player_GetAndRemoveInventory, typeof(Inventory)), // what is this?
            APICall.getKVP(CmdId.Request_Playfield_Entity_List, typeof(PString), CmdId.Event_Playfield_Entity_List, typeof(PlayfieldEntityList)),
            APICall.getKVP(CmdId.Request_Entity_Destroy2, typeof(IdPlayfield), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_Entity_Export, typeof(EntityExportInfo), CmdId.Event_Ok, null),
            APICall.getKVP(CmdId.Request_Entity_SetName, typeof(IdPlayfieldName), CmdId.Event_Ok, null),

        }.ToDictionary(x => x.Key, x => x.Value);
    }
}
