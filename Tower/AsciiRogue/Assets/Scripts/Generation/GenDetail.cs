﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenDetail
{

    public enum EntityType
    {
        Enemy,
        Item,
        Chest,
        Altar,
        Anvil,
        StairsUp,
        StairsDown,
        Door,
        KeyDoor,
        BloodDoor,

    }
    public int Priority
    {
        get
        {
            switch (Type)
            {
                case DetailType.Entity:
                    return 3;
                case DetailType.Decoration:
                    return 2;
                case DetailType.Background:
                    return 0;
                case DetailType.Wall:
                    return 1;
                case DetailType.Floor:
                default:
                    return -1;
            }
        }
    }
    public enum DetailType
    {
        Entity, // interactable, moveable, non-static things
        Decoration, // unpassable tiles
        Background, // or Floor, not sure what is better -> walkable
        Wall,
        Floor // default 
    }

    public EntityType Entity;
    public DetailType Type;

    public char Char = '¶';
    public string Name = "Invalid";







}
