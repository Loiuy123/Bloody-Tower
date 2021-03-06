﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointer : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    private Vector2Int mousePos = new Vector2Int();

    public List<Vector2Int> path;

    private Rect mapRect;

    private bool selected;

    private PlayerMovement playerMovement;

    public static MousePointer mousePointer;

    public int smallY, bigY;

    private bool _selected
    {
        get
        {
            return selected;
        }
        set
        {
            if (value != selected)
            {
                selected = value;
                DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
            }
        }
    }

    void Start()
    {
        mousePointer = this;
        playerMovement = GameManager.manager.player;
        UpdateMapRect();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.manager.playerStats.isDead) return;

        if(mapRect.Contains(Input.mousePosition))
        {
            mousePos = new Vector2Int(Mathf.FloorToInt((Input.mousePosition.x - pointA.position.x) / ((pointB.position.x - pointA.position.x) / DungeonGenerator.dungeonGenerator.mapWidth)), Mathf.FloorToInt((Input.mousePosition.y - (ResolutionManager.resolutionManager._Resolution == ResolutionManager.resolutionType.small ? bigY : smallY)) / ((pointB.position.y - pointA.position.y) / DungeonGenerator.dungeonGenerator.mapHeight))); //14 20

            if (!MapManager.IsValid)
            {
                return;
            }
            
            if( MapManager.map[mousePos.x, mousePos.y].isExplored)
            {
                Selector.Current.SelectedTile(mousePos.x, mousePos.y);

                if (MapManager.map[mousePos.x, mousePos.y].isWalkable || MapManager.map[mousePos.x, mousePos.y].type == "Door" || MapManager.map[mousePos.x, mousePos.y].enemy != null)
                {
                    _selected = true;
                    path = null;

                    path = AStar.CalculatePath(MapManager.playerPos, mousePos);

                    foreach (var pathTile in path)
                    {
                        MapManager.map[pathTile.x, pathTile.y].decoy = $"<color=#2B3659>\u205C</color>";
                    }

                    DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
                }
                else
                {
                    _selected = false;
                }
            }

            if (_selected && Input.GetMouseButtonDown(0))
            {
                StartCoroutine(WalkPath(path));
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (MapManager.map[mousePos.x, mousePos.y].isVisible && MapManager.map[mousePos.x, mousePos.y].isExplored && MapManager.map[mousePos.x, mousePos.y].enemy != null)
                {
                    GameManager.manager.UpdateMessages($"<color=red>{MapManager.map[mousePos.x, mousePos.y].enemy.GetComponent<RoamingNPC>().enemySO.enemyInfo}</color>");
                }
            }
        }    
    }

    public void UpdateMapRect()
    {
        mapRect = new Rect(pointA.transform.position.x, pointA.transform.position.y, pointB.transform.position.x - pointA.transform.position.x, pointB.transform.position.y - pointA.transform.position.y);
    }

    IEnumerator WalkPath(List<Vector2Int> _path)
    {
        for (int i = 0; i < _path.Count; i++)
        {
            playerMovement.Move(_path[i]);
            yield return new WaitForSeconds(.06f);
        }      
        yield return null;
    }
}
