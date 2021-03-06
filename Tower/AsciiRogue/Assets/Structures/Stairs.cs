﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stairs : Structure
{
    public int dungeonLevelId; //id of the floor where it will teloport you
    public Vector2Int spawnPosition;

    public override void Use()
    {
        MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].hasPlayer = false;
        MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].letter = "";

        if (!MapManager.Floors[dungeonLevelId].EnteredBefore && MapManager.Floors[dungeonLevelId].randomEvent != null)
        {
            MapManager.Floors[dungeonLevelId].EnteredBefore = true;
            RandomFloorEventsManager.instance.SendMessage(MapManager.Floors[dungeonLevelId].randomEvent.eventVoidName);
            //MapManager.Floors[dungeonLevelId].randomEvent.;
        }
        DungeonGenerator.dungeonGenerator.MovePlayerToFloor(dungeonLevelId);
        // MapManager.MoveToFloor(dungeonLevelId);
    }

    public override void WalkIntoTrigger()
    {
        GameManager.manager.UpdateMessages("Press <color=yellow>'space'</color> to walk stairs.");
    }
}
