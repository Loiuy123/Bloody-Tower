﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class CleanerTemple
{
    public static RangeInt TempleWidth = new RangeInt(56, 1);
    public static RangeInt TempleHeight = new RangeInt(22, 1);

    public static RangeInt RandomWidth = new RangeInt(5, 15);
    public static RangeInt RandomHeight = new RangeInt(5, 15);

    public static event System.Action Done;


    public static TMPro.TMP_Text Logging;
    public static TMPro.TMP_Text Overlay;

    public static string LastOutput;

    public enum RoomTypes
    {
        FourPillars,
        PillarHallway,
        AltarRoom,
        Empty,
        Outer, // if something else should be put in here
        RandomPillars,
        PillarSpam
    }


    public static string GetSimpleTemple()
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        GenData temple = new GenData(RNG.Range(TempleWidth.start, TempleWidth.end), RNG.Range(TempleHeight.start, TempleHeight.end));

        List<GenRoom> AutoFillRoom = new List<GenRoom>();

        // list of all rooms that are not allowed to be used for door placement 
        List<GenRoom> NoAutoDoor = new List<GenRoom>();
        List<GenRoom> SecretRooms = new List<GenRoom>();

        GenTile Chest = GenTile.GetEmpty();
        Chest.Details.Add(new GenDetail() { Char = '=', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Chest });

        GenTile pillar = GenTile.GetEmpty();
        //pillar.Details.Add(new GenDetail() { Char = '\u01C1', Type = GenDetail.DetailType.Decoration });
        pillar.Details.Add(new GenDetail() { Char = '\u01C1', Type = GenDetail.DetailType.Decoration });

        GenTile[,] pillars = new GenTile[,]
        {
            {GenTile.Copy(pillar),GenTile.Copy(pillar) },
            {GenTile.Copy(pillar),GenTile.Copy(pillar) }
        };

        GenTile Door = GenTile.GetEmpty();
        Door.Details.Add(new GenDetail() { Char = '+', Type = GenDetail.DetailType.Door, Entity = GenDetail.EntityType.Door });


        GenRoom outer = GenRoom.Sized(temple.Width, temple.Height);
        outer.FillFloor('+');
        outer.SpacePriority = -2;
        temple.PlaceRoom(0, 0, outer);
        temple.EdgeWalls('#', outer);


        int w = RNG.Range(9, 12);
        int h = RNG.Range(10, 16);

        // EntryHall
        temple.TryGrowRect(1, outer.Outer.GetCenter().y, w, h, out GenRect EntrySize, false);
        GenRoom EntryHall = GenRoom.At(EntrySize.MinX, EntrySize.MinY, EntrySize.WidthT, EntrySize.HeightT);
        EntryHall.FillFloor('.');
        temple.PlaceRoom(EntrySize.MinX, EntrySize.MinY, EntryHall);
        temple.EdgeWalls('#');
        EntryHall.GetAtWorldspaceG(
            EntrySize.MinX + 1, EntrySize.GetCenter().y)
            .Details
            .Add(new GenDetail() { Char = '>', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.StairsDown });


        int posX = EntrySize.MinX + 2;
        int posy = EntrySize.MinY + 2;
        GenTile[,] sym = GenUtil.GetSymetry(pillars.GetCopy(), ref posX, ref posy, EntryHall, GenUtil.Axis.Horizontal | GenUtil.Axis.Vertical);

        EntryHall.PlaceDetailsAt(posX, posy, sym);
        temple.FixOverlap();


        // hall to big thing
        int whall = RNG.Range(10, 22);
        int hhall = RNG.Range(5, 7);
        int space = RNG.Range(2, 4);

        temple.TryGrowRect(EntrySize.MaxX + 1, EntrySize.GetCenter().y, whall, hhall, out GenRect HallSize);
        GenRoom PillarHall = GenRoom.Sized(HallSize.WidthT, HallSize.HeightT);
        PillarHall.FillFloor('.');
        PillarHall.SpacePriority = 3;
        temple.PlaceRoom(HallSize.MinX, HallSize.MinY, PillarHall);
        temple.EdgeWalls('#', PillarHall);

        NoAutoDoor.Add(PillarHall);

        // place doors to the entry
        if (hhall == 5)
        {
            // a single door in the middle
            PillarHall.AddDetails(HallSize.MinX, HallSize.MinY + 2, GenTile.Copy(Door));
            PillarHall.AddDetails(HallSize.MaxX, HallSize.MinY + 2, GenTile.Copy(Door));
        }
        else
        {
            // place symetric doors
            PillarHall.AddDetails(HallSize.MinX, HallSize.MinY + 2, GenTile.Copy(Door));
            PillarHall.AddDetails(HallSize.MinX, HallSize.MinY + 3, GenTile.Copy(Door));

            PillarHall.AddDetails(HallSize.MaxX, HallSize.MinY + 2, GenTile.Copy(Door));
            PillarHall.AddDetails(HallSize.MaxX, HallSize.MinY + 3, GenTile.Copy(Door));
        }

        int currBar = HallSize.MinX + space;
        GenTile[,] singlePillar = new GenTile[,] { { GenTile.Copy(pillar) } };
        while (temple.IsInsideRoom(currBar, HallSize.MinY + 1, PillarHall))
        {


            int fx = currBar;
            int fy = HallSize.MinY + 1;
            GenTile[,] feature = GenUtil.GetSymetry(singlePillar, ref fx, ref fy, PillarHall, GenUtil.Axis.Vertical);
            PillarHall.PlaceDetailsAt(fx, fy, feature);
            currBar += space;
        }
        temple.FixOverlap();


        // holy water or something

        int waterHeight = RNG.Range(2, 4);
        int waterWidth = RNG.Range(2, 4);
        int waterPillarWidth = RNG.Range(2, 3);

        int waterRoomHeight = waterHeight + 4 + waterPillarWidth * 2;
        int waterRoomWidth = waterWidth + 6 + waterPillarWidth * 2;

        temple.TryGrowRect(HallSize.MaxX + 1, HallSize.GetCenter().y, waterRoomWidth, waterRoomHeight, out GenRect WaterSize, false, GenUtil.Direction4.Top);

        GenRoom waterRoom = GenRoom.Sized(WaterSize.WidthT, WaterSize.HeightT);
        waterRoom.FillFloor();
        waterRoom.SpacePriority = 2;
        temple.PlaceRoom(WaterSize.MinX, WaterSize.MinY, waterRoom);
        temple.EdgeWalls('#', waterRoom);


        int BackDoorWater = RNG.Range(1, waterRoom.Height / 2);
        waterRoom.AddDetails(WaterSize.MaxX, WaterSize.MinY + BackDoorWater, GenTile.Copy(Door));
        waterRoom.AddDetails(WaterSize.MaxX, WaterSize.MaxY - BackDoorWater, GenTile.Copy(Door));


        GenTile waterSingle = GenTile.GetEmpty();
        waterSingle.Details.Add(new GenDetail() { Char = '~', Type = GenDetail.DetailType.Background });
        GenTile[,] water = GenUtil.Fill(waterWidth, waterHeight, waterSingle);

        waterRoom.PlaceDetailsAt(WaterSize.MinX + 3 + waterPillarWidth, WaterSize.MinY + 2 + waterPillarWidth, water);

        int waterPX = WaterSize.MinX + 3;
        int waterPY = WaterSize.MinY + 2;

        GenTile[,] waterPillarPlace = GenUtil.GetSymetry(pillars.GetCopy(), ref waterPX, ref waterPY, waterRoom, GenUtil.Axis.Horizontal | GenUtil.Axis.Vertical);
        waterRoom.PlaceDetailsAt(waterPX, waterPY, waterPillarPlace);


        temple.FixOverlap();


        // pillar spam
        int spamWidth = RNG.Range(10, 20);
        int spamHeight = WaterSize.HeightT + RNG.Range(8, 15);

        temple.TryGrowRect(WaterSize.MaxX + 1, WaterSize.GetCenter().y, spamWidth, spamHeight, out GenRect SpamSize, true, GenUtil.Direction4.Top);
        GenRoom Spam = GenRoom.Sized(SpamSize.WidthT, SpamSize.HeightT);
        Spam.FillFloor();
        Spam.SpacePriority = 1;
        temple.PlaceRoom(SpamSize.MinX, SpamSize.MinY, Spam);
        temple.EdgeWalls('#', Spam);

        int spamPX = SpamSize.MinX + 2;
        int spamPY = SpamSize.MinY + 2;

        for (int x = spamPX; temple.IsInsideRoom(x, spamPY, Spam); x += 2)
        {
            for (int y = spamPY; temple.IsInsideRoom(x, y, Spam); y += 2)
            {
                GenTile p = GenTile.Copy(pillar);
                Spam.AddDetails(x, y, p);
            }
        }
        

        temple.FixOverlap();


        //temple.Rooms.Remove(outer); // we dont have boundries
        for (int x = outer.Inner.MinX; x < outer.Outer.MaxX; x++)
        {
            for (int y = outer.Inner.MinY; y < outer.Outer.MaxY; y++)
            {
                outer.RemoveTileAtG(x, y);
            }
        }


        //GenRoom Mark = GenRoom.Sized(0, 0);
        //Mark.FillFloor('X');
        //Mark.SpacePriority = 100;
        Debug.Log("Pre defined done: " + timer.ElapsedMilliseconds);

        // lets go ham with randomly sized rooms
        int spawnAttemptsRemaining = 1000;

        while (spawnAttemptsRemaining-- > 0)// lol the arrow
        {
            int rWidth = RNG.Range(RandomWidth.start, RandomWidth.end);
            int rHeight = RNG.Range(RandomHeight.start, RandomHeight.end);

            int rX = RNG.Range(2, temple.Width - 2);
            int rY = RNG.Range(2, temple.Height - 2);
            GenRect rHopeSize = new GenRect(rX, rX + rWidth + 1, rY, rY + rWidth - 1);


            if (temple.IsInsideRoom(rX, rY) || temple.GetTile(rX, rY) != null)
            {
                continue;
            }

            temple.TryGrowRect(rX, rY, rWidth, rHeight, out GenRect rSize, true);

            GenRoom add = GenRoom.Sized(rSize.WidthT, rSize.HeightT);
            add.FillFloor();
            add.SpacePriority = 01;

            temple.PlaceRoom(rSize.MinX, rSize.MinY, add);

            AutoFillRoom.Add(add);

            temple.EdgeWalls('#', add);

            temple.FixOverlap();

        }

        Debug.Log("random rooms done: " + timer.ElapsedMilliseconds);
        // now fill the rooms with things

        var adjList = temple.GetAdjacentRoomMap();
        // remove any Room that is too small and have no connections
        Debug.Log("adjList created: " + timer.ElapsedMilliseconds);

        List<GenRoom> tooSmall = new List<GenRoom>();
        foreach (var room in temple.Rooms)
        {
            if (room.Width >= 5 && room.Height == 3)
            {
                tooSmall.Add(room);
                continue;
            }
            if (room.Height >= 5 && room.Width == 3)
            {
                tooSmall.Add(room);
                continue;
            }
            if (room.Height <= 3 && room.Width <= 3)
            {
                tooSmall.Add(room);
                continue;
            }
            if (adjList[room].Count == 0)
            {
                tooSmall.Add(room);
                continue;
            }
        }
        foreach (var room in tooSmall)
        {
            temple.Rooms.Remove(room);
        }

        Debug.Log("Remove small Room Time: " + timer.ElapsedMilliseconds);

        List<GenRoom> PotentialSecret = new List<GenRoom>();

        foreach (var room in temple.Rooms)
        {
            if (room.Width + room.Height <= 12)
            {
                PotentialSecret.Add(room);
            }
        }

        Debug.Log("potential " + PotentialSecret.Count + "Secret rooms");
        Debug.Log("Potential Secret Room Time: " + timer.ElapsedMilliseconds);

        // 1 room --> 0,1,2,3
        // 2 room --> 4,5
        int SecretCount = Mathf.Min(Mathf.FloorToInt(Mathf.Sqrt(RNG.Range(1, 6))), PotentialSecret.Count); // this goes to 5 

        Debug.Log(SecretCount + " Secret rooms chosen");
        foreach (var secret in PotentialSecret.GetRandom(SecretCount))
        {
            // we get random door
            // add a chest
            // remove it from door spawn
            GenPositionTile entry = temple.GetDoorableTiles(secret.GetEdge().ToList()).GetRandom();
            secret.AddDetail(entry.PositionG.x, entry.PositionG.y,
                new GenDetail() { Char = 'h', Entity = GenDetail.EntityType.Door, Type = GenDetail.DetailType.Door });

            GenPositionTile myChest = secret
                .GetAllTiles()
                .Where(t => temple.IsInsideRoom(t.PositionG.x, t.PositionG.y, secret) && temple.IsCornerGR(t.PositionG.x, t.PositionG.y, secret))
                .ToList()
                .GetRandom();
            secret.AddDetails(myChest.PositionG.x, myChest.PositionG.y, GenTile.Copy(Chest));

            AutoFillRoom.Remove(secret);
            SecretRooms.Add(secret);
            NoAutoDoor.Add(secret);
        }
        Debug.Log("secret rooms: " + timer.ElapsedMilliseconds);

        // go through all other rooms and determin what they are 

        foreach (GenRoom room in AutoFillRoom)
        {


            // pillar hallway
            if (room.Height <= 7 && room.Height >= 5 && room.Width > 6)
            {
                // potential horizontal hallway
                if (RNG.Next() < 0.4f)
                {
                    // hallway confirmed
                    // left to right

                    GenTile[,] p = new GenTile[,]
                    {
                        {GenTile.Copy(pillar) }
                    };
                    int spacing = RNG.Range(2, 5);

                    int tmpX = room.Outer.MinX + spacing;
                    int tmpY = room.Outer.MinY + 1;


                    p = GenUtil.GetSymetry(p, ref tmpX, ref tmpY, room, GenUtil.Axis.Vertical);

                    while (temple.IsInsideRoom(tmpX, tmpY, room))
                    {
                        room.PlaceDetailsAt(tmpX, tmpY, p);

                        tmpX = tmpX + spacing;
                    }
                    int enemyCount = RNG.Range(0, 4);
                    for (int i = 0; i < enemyCount; i++)
                    {
                        SpawnEnemy(temple, room);
                    }
                    int itemCount = RNG.Range(-1, 3);
                    for (int i = 0; i < itemCount; i++)
                    {
                        SpawnItem(temple, room, true);
                    }
                    int chestCount = RNG.Range(-2, 2);
                    for (int i = 0; i < chestCount; i++)
                    {
                        SpawnChest(temple, room, true);
                    }
                    continue;
                }
            }
            if (room.Width <= 7 && room.Width >= 5 && room.Height > 6)
            {
                // potential horizontal hallway
                if (RNG.Next() < 0.4f)
                {
                    // hallway confirmed
                    // left to right

                    GenTile[,] p = new GenTile[,]
                    {
                        {GenTile.Copy(pillar) }
                    };
                    int spacing = RNG.Range(2, 5);

                    int tmpX = room.Outer.MinX + 1;
                    int tmpY = room.Outer.MinY + spacing;


                    p = GenUtil.GetSymetry(p, ref tmpX, ref tmpY, room, GenUtil.Axis.Horizontal);

                    while (temple.IsInsideRoom(tmpX, tmpY, room))
                    {
                        room.PlaceDetailsAt(tmpX, tmpY, p);

                        tmpY = tmpY + spacing;
                    }
                    int enemyCount = RNG.Range(0, 4);
                    for (int i = 0; i < enemyCount; i++)
                    {
                        SpawnEnemy(temple, room);
                    }
                    int itemCount = RNG.Range(-1, 3);
                    for (int i = 0; i < itemCount; i++)
                    {
                        SpawnItem(temple, room, true);
                    }
                    int chestCount = RNG.Range(-2, 2);
                    for (int i = 0; i < chestCount; i++)
                    {
                        SpawnChest(temple, room, true);
                    }
                    continue;
                }
            }

            if (room.Height >= 8 && room.Width >= 8)
            {
                // can either be pillar spam or room in room

                if (RNG.Next() < 0.6f)
                {
                    if (RNG.Next() < 0.7f && room.Width % 2 == 1 && room.Height % 2 == 1)
                    {
                        // pillar spam

                        for (int x = 2; x < room.Width - 2; x += 2)
                        {
                            for (int y = 2; y < room.Height - 2; y += 2)
                            {
                                room.AddDetails(room.PosX + x, room.PosY + y, GenTile.Copy(pillar));
                            }
                        }
                        int enemyCount = RNG.Range(0, 5);
                        for (int i = 0; i < enemyCount; i++)
                        {
                            SpawnEnemy(temple, room);
                        }
                        int itemCount = RNG.Range(-1, 3);
                        for (int i = 0; i < itemCount; i++)
                        {
                            SpawnItem(temple, room, true);
                        }
                        int chestCount = RNG.Range(-3, 3);
                        for (int i = 0; i < chestCount; i++)
                        {
                            SpawnChest(temple, room, true);
                        }
                    }
                    else
                    {
                        // room in room

                        // find where to put the inner room
                        temple.TryGrowRect(room.Inner.GetCenter().x, room.Inner.GetCenter().y, 100, 100, out GenRect InnerSize);


                        if (InnerSize.WidthT >= 4 && InnerSize.HeightT >= 4)
                        {
                            if (InnerSize.WidthT >= 10 || InnerSize.HeightT >= 10)
                            {
                                if (Mathf.Abs(InnerSize.WidthT - InnerSize.HeightT) > 3)
                                {
                                    // we want to divide
                                    if (InnerSize.WidthT > InnerSize.HeightT)
                                    {

                                        // divide left and right
                                        int singleWidth = InnerSize.WidthT / 2 - 2;
                                        // left
                                        GenRect LeftRoom = new GenRect(InnerSize.MinX, InnerSize.MinX + singleWidth, InnerSize.MinY, InnerSize.MaxY);
                                        // right
                                        GenRect RightRoom = new GenRect(InnerSize.MaxX - singleWidth, InnerSize.MaxX, InnerSize.MinY, InnerSize.MaxY);

                                        GenRoom left = GenRoom.Sized(LeftRoom.WidthT, LeftRoom.HeightT);
                                        left.FillFloor();
                                        left.SpacePriority = 4;
                                        GenRoom right = GenRoom.Sized(RightRoom.WidthT, RightRoom.HeightT);
                                        right.FillFloor();
                                        right.SpacePriority = 4;
                                        temple.PlaceRoom(LeftRoom.MinX, LeftRoom.MinY, left);
                                        temple.PlaceRoom(RightRoom.MinX, RightRoom.MinY, right);

                                        NoAutoDoor.Add(left);
                                        NoAutoDoor.Add(right);

                                        temple.EdgeWalls('#', left);
                                        temple.EdgeWalls('#', right);

                                        temple.FixOverlap();


                                        var leftDoor = temple.GetDoorableTiles(left.GetAllTiles()).GetRandom(1);
                                        var rightDoor = temple.GetDoorableTiles(right.GetAllTiles()).GetRandom(1);

                                        for (int i = 0; i < leftDoor.Count; i++)
                                        {
                                            left.AddDetails(leftDoor[i].PositionG.x, leftDoor[i].PositionG.y, GenTile.Copy(Door));
                                        }
                                        for (int i = 0; i < rightDoor.Count; i++)
                                        {
                                            right.AddDetails(rightDoor[i].PositionG.x, rightDoor[i].PositionG.y, GenTile.Copy(Door));
                                        }
                                        SpawnItem(temple, left);
                                        SpawnItem(temple, right);
                                        if (RNG.Next() < 0.4f)
                                        {
                                            SpawnItem(temple, right);
                                        }
                                        if (RNG.Next() < 0.4f)
                                        {
                                            SpawnItem(temple, left);
                                        }
                                        if (RNG.Next() < 0.2f)
                                        {
                                            SpawnChest(temple, left, true);
                                        }
                                        if (RNG.Next() < 0.2f)
                                        {
                                            SpawnChest(temple, left, true);
                                        }
                                    }
                                    else
                                    {
                                        // divide top bot
                                        Debug.Log("currently not implemented, sorry");
                                    }
                                }
                            }
                            else
                            {
                                // one single room
                                if (InnerSize.WidthT > 5)
                                {
                                    InnerSize = InnerSize.Transform(-1, 0, -1, 0);
                                }
                                if (InnerSize.HeightT > 5)
                                {
                                    InnerSize = InnerSize.Transform(0, -1, 0, -1);
                                }

                                Debug.Log("HERE");

                                GenRoom single = GenRoom.Sized(InnerSize.WidthT, InnerSize.HeightT);
                                single.SpacePriority = 4;
                                single.FillFloor();
                                NoAutoDoor.Add(single);

                                temple.PlaceRoom(InnerSize.MinX, InnerSize.MinY, single);
                                temple.EdgeWalls('#', single);
                                temple.FixOverlap();


                                // double doors
                                var doorables = single.GetAllTiles();// single.GetEdge().ToList();
                                var theDoors = temple.GetDoorableTiles(doorables).GetRandom(2);

                                for (int i = 0; i < theDoors.Count; i++)
                                {
                                    single.AddDetails(theDoors[i].PositionG.x, theDoors[i].PositionG.y, GenTile.Copy(Door));
                                }

                                SpawnItem(temple, single);

                                if (RNG.Next() < 0.2f)
                                {
                                    SpawnChest(temple, single, true);
                                }
                                if (RNG.Next() < 0.2f)
                                {
                                    SpawnChest(temple, single, true);
                                }
                            }
                        }

                        SpawnEnemy(temple, room);
                        if (RNG.Next() < 0.5f)
                        {
                            SpawnEnemy(temple, room);
                        }
                        if (RNG.Next() < 0.2f)
                        {
                            SpawnEnemy(temple, room);
                        }

                    }
                    continue;
                }
            }

            // a room with some other sort of detail





            // something random
            //room.FillFloor('~');

            SpawnEnemy(temple, room);
            SpawnEnemy(temple, room);

            if (RNG.Next() < 0.5f)
            {
                SpawnEnemy(temple, room);
            }
            if (RNG.Next() < 0.2f)
            {
                SpawnEnemy(temple, room);
            }
            SpawnItem(temple, room);
            if (RNG.Next() < 0.3f)
            {
                SpawnItem(temple, room);
            }
            if (RNG.Next() < 0.3f)
            {
                SpawnItem(temple, room);
            }
            if (RNG.Next() < 0.4f)
            {
                SpawnChest(temple, room);
            }
            if (RNG.Next() < 0.1f)
            {
                SpawnChest(temple, room);
            }
        }
        Debug.Log("autofill done: " + timer.ElapsedMilliseconds);

        List<GenRoom> RequireDoor = new List<GenRoom>(temple.Rooms);

        foreach (var doo in NoAutoDoor)
        {
            RequireDoor.Remove(doo);
        }
        
        GenRoom start = EntryHall;

        

        void RandomDoorTo(GenRoom a, GenRoom b)
        {
            List<GenPositionTile> tiles = temple.GetDoorableTiles(temple.GetConnectingTiles(a, b));
            GenPositionTile location = tiles.GetRandom();
            a.AddDetails(location.PositionG.x, location.PositionG.y, GenTile.Copy(Door));
        }

        

        List<GenRoom> DoorIteration = new List<GenRoom>(RequireDoor);
        Dictionary<GenRoom, List<GenRoom>> adj = temple.GetAdjacentRoomMap();

        foreach (var from in adj)
        {
            foreach (var to in NoAutoDoor)
            {
                adj[from.Key].Remove(to);
            }
        }

        Debug.Log("change adj: " + timer.ElapsedMilliseconds);

        Queue<GenRoom> Doorqueue = new Queue<GenRoom>(RequireDoor);

        Dictionary<GenRoom, List<GenRoom>> mapDoored = temple.GetDooredMap();

        Debug.Log("queue new: " + timer.ElapsedMilliseconds);

        while (Doorqueue.Count > 0)
        {
            GenRoom room = Doorqueue.Dequeue();

            if (adj[room].Count > 0)
            {
                GenRoom to= adj[room].GetRandom();
                RandomDoorTo(room,to );
                if (!mapDoored.ContainsKey(room))
                {
                    mapDoored[room] = new List<GenRoom>();
                }
                if (!mapDoored.ContainsKey(to))
                {
                    mapDoored[to] = new List<GenRoom>();
                }
                if (!mapDoored[room].Contains(to))
                {
                    mapDoored[room].Add(to);
                    mapDoored[to].Add(room);
                }
                if (timer.ElapsedMilliseconds>10000)
                {
                    break;
                }
            }

            if (temple.IsReachable(start, room,mapDoored))
            {
                // reachable so we dont have to do a thing
            }
            else
            {
                Doorqueue.Enqueue(room);
            }
            if (timer.ElapsedMilliseconds > 3500)
            {
                break;
            }
        }

        foreach (var room in adj[Spam].GetRandom(2))
        {
            RandomDoorTo(Spam, room);
        }
        Debug.Log("Doors time: " + timer.ElapsedMilliseconds);

        if (RNG.Next() < 0.2f)
        {
            Spam.AddDetail(
            SpamSize.MaxX - 1,
            SpamSize.GetCenter().y,
            new GenDetail()
            { Char = '<', Type = GenDetail.DetailType.Stairs, Entity = GenDetail.EntityType.StairsUp });
        }
        else
        {
            var room = RequireDoor.Where(x=>temple.IsReachable(EntryHall,x,mapDoored)).ToList().GetRandom();
            var spawn = room
                .GetAllTiles()
                .Where(t => temple.IsInsideRoom(t.PositionG.x, t.PositionG.y, room))
                .ToList();
            var only = spawn.GetRandom();
            GenDetail item = new GenDetail() { Char = '<', Type = GenDetail.DetailType.Stairs, Entity = GenDetail.EntityType.StairsUp };
            room.AddDetail(only.PositionG.x, only.PositionG.y, item);
        }

        Debug.Log("Stairs done: " + timer.ElapsedMilliseconds);


        Debug.Log("comepletely Done: " + timer.ElapsedMilliseconds);
        Debug.Log("Done");


        LastOutput = GenUtil.Print(temple, false);
        Done?.Invoke();


        return LastOutput;
    }
    public static void SpawnChest(GenData data, GenRoom room, bool corner = false)
    {
        if (corner)
        {
            var spawn = room
                .GetAllTiles()
                .Where(t => data.IsCornerG(t.PositionG.x, t.PositionG.y, GenDetail.DetailType.Wall, GenDetail.DetailType.Decoration))
                .ToList().GetRandom();
            GenDetail chest = new GenDetail() { Char = '=', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Chest };
            room.AddDetail(spawn.PositionG.x, spawn.PositionG.y, chest);
        }
        else
        {
            var spawn = room
                .GetAllTiles()
                .Where(t => data.IsInsideRoom(t.PositionG.x, t.PositionG.y, room))
                .ToList()
                .GetRandom();
            GenDetail chest = new GenDetail() { Char = '=', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Chest };
            room.AddDetail(spawn.PositionG.x, spawn.PositionG.y, chest);
        }
    }
    public static void SpawnItem(GenData data, GenRoom room, bool corner = false)
    {
        if (corner)
        {
            var spawn = room
                .GetAllTiles()
                .Where(t => data.IsCornerG(t.PositionG.x, t.PositionG.y, GenDetail.DetailType.Wall, GenDetail.DetailType.Decoration))
                .ToList().GetRandom();
            GenDetail item = new GenDetail() { Char = '0', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Item };
            room.AddDetail(spawn.PositionG.x, spawn.PositionG.y, item);
        }
        else
        {
            var spawn = room
                .GetAllTiles()
                .Where(t => data.IsInsideRoom(t.PositionG.x, t.PositionG.y, room))
                .ToList();
            var only = spawn.GetRandom();
            GenDetail item = new GenDetail() { Char = '0', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Item };
            room.AddDetail(only.PositionG.x, only.PositionG.y, item);
        }
    }
    public static void SpawnEnemy(GenData data, GenRoom room, bool corner = false)
    {
        if (corner)
        {
            var spawn = room
                .GetAllTiles()
                .Where(t => data.IsCornerG(t.PositionG.x, t.PositionG.y, GenDetail.DetailType.Wall, GenDetail.DetailType.Decoration))
                .ToList().GetRandom();
            GenDetail enemy = new GenDetail() { Char = 'g', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Enemy };
            room.AddDetail(spawn.PositionG.x, spawn.PositionG.y, enemy);
        }
        else
        {
            var spawn = room
                .GetAllTiles()
                .Where(t => data.IsInsideRoom(t.PositionG.x, t.PositionG.y, room))
                .ToList()
                .GetRandom();
            GenDetail enemy = new GenDetail() { Char = 'g', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Enemy };
            room.AddDetail(spawn.PositionG.x, spawn.PositionG.y, enemy);
        }
    }




    public static void Log(GenData data)
    {
        if (Logging != null)
        {
            Logging.text = GenUtil.Print(data, false);
        }
    }
    public static void OverlayShow(char[,] data)
    {
        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < data.GetLength(1); y++)
        {
            for (int x = 0; x < data.GetLength(0); x++)
            {
                sb.Append(data[x, y]);
            }
        }
        OverlayShow(sb.ToString());
    }
    public static void OverlayShow(string data)
    {
        Overlay.text = data;
    }
    public static void OverlayHide()
    {
        Overlay.text = "";
    }

    static GenDetail dPillar = new GenDetail() { Char = '\u01C1', Type = GenDetail.DetailType.Decoration };
    static GenDetail dStatue = new GenDetail() { Char = '&', Type = GenDetail.DetailType.Decoration };
    static GenDetail dChest = new GenDetail() { Char = '=', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Chest };
    static GenDetail dItem = new GenDetail() { Char = '0', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Item };
    static GenDetail dEnemy = new GenDetail() { Char = 'g', Type = GenDetail.DetailType.Entity, Entity = GenDetail.EntityType.Enemy };

    static GenDetail dWater = new GenDetail() { Char = '~', Type = GenDetail.DetailType.Background };
    static GenDetail dEmpty = new GenDetail() { Char = ' ', Type = GenDetail.DetailType.Nothing };


    public static List<GenTile[,]> RoomDecorations = new List<GenTile[,]>()
    {
        new GenTile[,]
        {
            {
                new GenTile(){Details = new List<GenDetail>(){ } },

            }
        }
    };



}
