using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PhotonCommanderHelper
{

    private static PhotonProviderManager ppm;

    public static bool IsServerHere { get; }
    
    public static void InitWith(PhotonProviderManager photonProviderManager)
    {
        //need to init with some manager or component, that give photon functions
    }

    #region photon connecting functions

    public static void Connect()
    {
        //ppm.innerObject.Connect();
    }

    public static List<RoomProvider> GetAllRooms()
    {
        return null;
        //get all rooms from photon provider 
    }

    public static RoomProvider CreateRoomAndJoin()
    {
        //also make server localy.
        return null;
        //create room with ppm and join to it
    }

    public static void JoinToRoom(RoomProvider room)
    {
        //join to room with ppm
    }
    
    #endregion

    #region RPC commands

    public static void SendCommandToServer(ServerRequest requestType, DataContainer data)
    {
        
    }

    public static void SubscribeToCommandFromServer(ServerResponce responceType, Action<DataContainer> callback)
    {
        
    }
    
    #endregion
    
}

public enum ServerResponce
{
    startDataLoading,
    gameStart,
    dataGameTickUpdate
}

public enum ServerRequest
{
    join,
    readyToLoading,
    dataLoaded,
    readyToStart
}


public class DataContainer
{
    
}

public class PhotonProviderManager
{
}

public class RoomProvider
{
    //photon room
}

[Serializable]
public class PlayerData
{
}
