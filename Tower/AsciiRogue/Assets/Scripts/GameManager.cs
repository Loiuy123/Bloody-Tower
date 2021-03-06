﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Enemy detect player messages")]
    public List<string> monsterDetectPlayer = new List<string>();

    DungeonGenerator dungeonGenerator;
    public static GameManager manager;
    public EnemySpawner enemySpawner;
    public ItemSpawner itemSpawner;
    public PlayerMovement player;
    public PlayerStats playerStats;

    public GameObject enemyPrefab;

    public bool isPlayerTurn;
    public int turns; //turn counter

    public event Action playerTurnTasks; //Effects that will be executed at players turn

    [HideInInspector] public Vector2Int poisonDuration; //x = min, y = ma
    [HideInInspector] public Vector2Int fireResistanceDuration;
    [HideInInspector] public Vector2Int poisonResistanceDuration;
    [HideInInspector] public Vector2Int regenerationDuration;

    //Messages
    public Text messages;
    private string messagesText;
    private Queue m_Messages;
    public Text mapName;

    //Inventory
    public Text inventory;
    private string inventoryText;
    [HideInInspector] public Queue<string> m_Inventory;
    public GameObject invBorder; //inventory ui border  

    public GameObject mapText;
    public GameObject weaponArtIndicatorText;
    public Text mainUItext;

    [Header("Item UI stats")]
    public Text itemName;
    public Text itemType;
    public Text itemRare;
    public Text itemEffects;
    public RectTransform selector;
    public int selectedItem;
    private bool inventoryOpen = false;

    [Header("Decisions")]
    public Text decisions;
    public GameObject GOdec;
    private Item _selectedItem;
    private bool equipState;

    [Header("Pathfinding")]
    public List<GameObject> enemies = new List<GameObject>();

    [HideInInspector] public Tasks tasks;

    private bool cheatMenu = false;
    public string cheatString;

    [HideInInspector] public bool isReadingBook;
    public bool waiting;
    public SpellbookSO readingBook;
    public IEnumerator waitingCoroutine;
    public bool openGrimoire;
    private string grimoireText;
    [HideInInspector] public Queue<string> m_grimoire;
    public bool decidingSpell;


    // Skill things
    [Header("Skills and stuff")]
    public bool SkillsOpen;
    public bool SkillCasting;
    public SkillScriptableObject LastSkill;
    [NonSerialized]public List<SkillScriptableObject> ExistingSkills;
    [NonSerialized]public List<SkillScriptableObject> LearnedSkills;
    [NonSerialized]public List<SkillScriptableObject> VisibleSkills;
    public int SelectedSkillIndex;
    public int TopVisibleSkillIndex; // the top most skill in the window
    public string SkillText;
    public List<SkillScriptableObject> AutoLearnSkills;

    public GameObject deadScreenAnimation;

    // Throw Item
    public bool IsThrowingItem;


    [HideInInspector] public FOVNEW fv;
    
    [HideInInspector] public Item itemToAnvil;
    [HideInInspector] public bool anvilMenuOpened;
    [HideInInspector] public Anvil anvil;

    public bool MenuOpen => anvilMenuOpened || openGrimoire || inventoryOpen || SkillsOpen ||IsThrowingItem;

    void Awake()
    {
        RNG.Seed = UnityEngine.Random.Range(1, 100000);
        RNG.Reset();

        if (!Controls.IsInitialized)
        {
            Debug.Log("whoops");
        }
        fv = GetComponent<FOVNEW>();

        manager = this;

        Popup.Init(GOdec, decisions);

        m_Messages = new Queue();
        m_Inventory = new Queue<string>();
        //m_grimoire = new Queue<string>();

        tasks = GetComponent<Tasks>();

        dungeonGenerator = GetComponent<DungeonGenerator>();        
    }

    void Start()
    {
        if (PrefabProvider.Rooms.Loaded== false)
        {

        }
        RunManager.StartNewRun();
        GenerateThings();

        //Clear "Messages" Box
        m_Messages.Clear();
        messages.text = "";

        FoV.Initialize();
        fv.Initialize(fv.CanLightPass, fv.SetToVisible, fv.Distance);

        invBorder.SetActive(false);
        VisibleSkills = new List<SkillScriptableObject>();
        ExistingSkills = new List<SkillScriptableObject>();
        foreach (var skill in AutoLearnSkills)
        {
            ExistingSkills.Add(skill);
        }
        if (LearnedSkills != null)
        {
            LearnedSkills.Clear();
        }
        else
        {
            LearnedSkills = new List<SkillScriptableObject>();
        }
    }
    public async Task GenerateThings()
    {
        messages.gameObject.SetActive(false);
        dungeonGenerator.InitializeDungeon();
        await Task.Delay(1);
        await dungeonGenerator.GenerateDungeon(MapManager.AppendNewFloor(), 0);
        MapManager.CurrentFloor.Valid = true;
        dungeonGenerator.MovePlayerToFloor(0);
        messages.gameObject.SetActive(true);

        if (!MapManager.CurrentFloor.GO.activeSelf)
        {
            Debug.Log("Something broke in generation");
        }

        m_Messages.Clear();
        messagesText = "";
        messages.text = messagesText;
        
        

        dungeonGenerator.DrawMap(MapManager.map);
        FirstTurn();
        await Task.Delay(1);
        for (int i = 1; i <= 20; i++)
        {
            await dungeonGenerator.GenerateDungeon(MapManager.AppendNewFloor(), i);
            await Task.Delay(1);
        }  
    }

    [Obsolete]
    public void Update()
    {
        if (waiting) return;

        if (isPlayerTurn)
        {
            CheckSkillLearning();
            if (Controls.GetKeyDown(Controls.Inputs.TechnicOpen) && !inventoryOpen && !openGrimoire && !IsThrowingItem)
            {
                if (!SkillsOpen)
                {
                    UpdateItemStats();
                    InitSkillWindow();
                    UpdateSkillText();
                    UpdateSkillStats();            
                }
                else
                {
                    CloseEQ();
                }
            }

            if (Controls.GetKeyDown(Controls.Inputs.InventoryOpen) && !choosingWeapon && !openGrimoire && !SkillsOpen && !IsThrowingItem)
            {
                if (!inventoryOpen)
                {
                    OpenEQ();
                    if (playerStats.itemsInEqGO.Count < 1) UpdateItemStats();
                }
                else
                {
                    CloseEQ();
                }               
            }
            else if (Controls.GetKeyDown(Controls.Inputs.CancelButton))
            {
                CloseEQ();
            }

            if (inventoryOpen && !Popup.Showing)
            {
                if (Controls.GetKeyDown(Controls.Inputs.Use) && !choosingWeapon)
                {
                    decisions.text = "";

                    try
                    {
                        _selectedItem = playerStats.itemsInEqGO[selectedItem];
                    }
                    catch { return; }
                    _selectedItem = playerStats.itemsInEqGO[selectedItem];

                    if (anvilMenuOpened)
                    {
                        if(_selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Armor || _selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Weapon)
                        {                            
                            equipState = playerStats.itemsInEqGO[selectedItem].isEquipped;
                                                       
                            Popup.NewDecission(true, ("Anvil", "Upgrade", gameObject));
                            Popup.Show();

                            DecisionTurn();
                        }                       
                    }
                    else
                    {
                        equipState = playerStats.itemsInEqGO[selectedItem].isEquipped;

                        //GOdec.SetActive(true);

                        Popup.NewDecission();
                        Popup.AddDecissionOption("Drop", "Drop", gameObject);
                        //decisions.text = "1. Drop";
                        //AddAnotherOption("Drop");

                        if (_selectedItem.iso.I_whereToPutIt != ItemScriptableObject.whereToPutIt.none)
                        {
                            if (equipState == true)
                            {
                                Popup.AddDecissionOption("Unequip", "UEquip", gameObject);
                                //decisions.text += "\n" + "2. Unequip";
                                //decisionsCount++;
                                //AddAnotherOption("UEquip");
                            }
                            else
                            {
                                Popup.AddDecissionOption("Equip", "UEquip", gameObject);
                                //decisions.text += "\n" + "2. Equip";
                                //decisionsCount++;
                                //AddAnotherOption("UEquip");
                            }
                        }
                        if (_selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Scroll || _selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Spellbook || _selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Gem || _selectedItem.iso is Bell bell)
                        {
                            Popup.AddDecissionOption("Use", "Use", gameObject);
                            //decisionsCount++;
                            //decisions.text += "\n" + decisionsCount + ". " + "Use";
                            //AddAnotherOption("Use");
                        }
                        else if (_selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Potion)
                        {
                            Popup.AddDecissionOption("Drink", "Use", gameObject);
                            //decisionsCount++;
                            //decisions.text += "\n" + decisionsCount + ". " + "Drink";
                            //AddAnotherOption("Use");
                        }
                        else if(_selectedItem.iso.I_itemType == ItemScriptableObject.itemType.Readable)
                        {
                            Popup.AddDecissionOption("Read", "Use", gameObject);
                            //decisionsCount++;
                            //decisions.text += "\n" + decisionsCount + ". " + "Read";
                            //AddAnotherOption("Use");
                        }
                        if (!equipState && ItemThrowHelper.CanBeThrown(_selectedItem))
                        {
                            Popup.AddDecissionOption("Throw", "ThrowItem", gameObject);
                            //decisionsCount++;
                            //decisions.text += "\n" + decisionsCount + ". Throw";
                            //AddAnotherOption("ThrowItem");
                        }

                        //decisionMade = false;
                        //deciding = true;

                        //DecisionTurn();
                        Popup.Show();
                    }                                     
                }
                /*else if(Controls.GetKeyDown(Controls.Inputs.Use) && choosingWeapon && playerStats.itemsInEqGO[selectedItem].iso is WeaponsSO) //ADD GEM TO THE SOCKET
                {
                    choosingWeapon = false;
                    choosenWeapon = playerStats.itemsInEqGO[selectedItem];
                    choosenWeapon.AddGem(gemToConnect);
                    //try { if (choosenWeapon.isEquipped) gemToConnect.Use(playerStats, playerStats.itemsInEqGO[selectedItem]); } catch { }
                    FinishPlayersTurn();
                }*/

                if (Controls.GetKeyDown(Controls.Inputs.InventoryDown))
                {
                    if (selectedItem < playerStats.itemsInEqGO.Count - 1)
                    {
                        selectedItem++;
                        selector.anchoredPosition -= new Vector2(0, 26);
                        UpdateItemStats(playerStats.itemsInEqGO[selectedItem]);
                    }
                    else
                    {
                        selectedItem = 0;
                        selector.anchoredPosition = new Vector3(selector.anchoredPosition.x, -284, 0);
                        UpdateItemStats(playerStats.itemsInEqGO[selectedItem]);
                    }
                }
                else if (Controls.GetKeyDown(Controls.Inputs.InventoryUp))
                {
                    if (selectedItem > 0)
                    {
                        selectedItem--;
                        selector.anchoredPosition += new Vector2(0, 26);
                        UpdateItemStats(playerStats.itemsInEqGO[selectedItem]);
                    }
                    else
                    {
                        selectedItem = playerStats.itemsInEqGO.Count - 1;
                        for (int i = 0; i < selectedItem; i++)
                        {
                            selector.anchoredPosition -= new Vector2(0, 26);
                        }
                        UpdateItemStats(playerStats.itemsInEqGO[selectedItem]);
                    }
                }               
            }

            /*if (openGrimoire)
            {
                if (Controls.GetKeyDown(Controls.Inputs.Use) && playerStats.rememberedSpells.Count > 0)
                {
                    GOdec.SetActive(true);
                    decisions.text = "1. Cast";
                    decisionMade = false;
                    decidingSpell = true;

                    DecisionTurn();
                }

                if (Controls.GetKeyDown(Controls.Inputs.GrimoirDown))
                {
                    if (selectedItem < playerStats.rememberedSpells.Count - 1)
                    {
                        selectedItem++;
                        selector.anchoredPosition -= new Vector2(0, 26);
                        UpdateSpellStats(playerStats.rememberedSpells[selectedItem]);
                    }
                    else
                    {
                        selectedItem = 0;
                        selector.anchoredPosition = new Vector3(selector.anchoredPosition.x, -284, 0);
                        UpdateSpellStats(playerStats.rememberedSpells[selectedItem]);
                    }
                }
                else if (Controls.GetKeyDown(Controls.Inputs.GrimoirUp))
                {
                    if (selectedItem > 0)
                    {
                        selectedItem--;
                        selector.anchoredPosition += new Vector2(0, 26);
                        UpdateSpellStats(playerStats.rememberedSpells[selectedItem]);
                    }
                    else
                    {
                        selectedItem = playerStats.rememberedSpells.Count - 1;
                        for (int i = 0; i < selectedItem; i++)
                        {
                            selector.anchoredPosition -= new Vector2(0, 26);
                        }
                        UpdateSpellStats(playerStats.rememberedSpells[selectedItem]);
                    }
                }
            }*/

            if (SkillsOpen)
            {
                if (!SkillCasting)
                {
                    if (Controls.GetKeyDown(Controls.Inputs.TechnicDown))
                    {
                        // move down one
                        SelectedSkillIndex++;
                    }
                    if (Controls.GetKeyDown(Controls.Inputs.TechnicUp))
                    {
                        SelectedSkillIndex--;
                    }
                    SelectedSkillIndex = Mathf.Clamp(SelectedSkillIndex, 0, VisibleSkills.Count - 1);
                    TopVisibleSkillIndex = Mathf.Clamp(TopVisibleSkillIndex, SelectedSkillIndex - 15, SelectedSkillIndex);

                    UpdateSkillText();
                    UpdateSkillStats();
                    if (Controls.GetKeyDown(Controls.Inputs.Use))
                    {
                        if (VisibleSkills[SelectedSkillIndex].IsCastable(playerStats))
                        {
                            // we will now cast the spell
                            if (VisibleSkills[SelectedSkillIndex].IsInstant)
                            {
                                // we dont have to block anything
                                VisibleSkills[SelectedSkillIndex].Prepare(playerStats);
                            }
                            else
                            {
                                SkillCasting = true;
                                VisibleSkills[SelectedSkillIndex].Prepare(playerStats);
                                invBorder.SetActive(false);
                                mapText.SetActive(true);
                                mainUItext.enabled = true;
                                Debug.Log("Trigger technic");
                            }
                            LastSkill = VisibleSkills[SelectedSkillIndex];

                        }
                    }
                }
                else
                {
                    // what to do during prep phase
                    if (!LastSkill.AllowTargetingMove()||
                        (
                        Mathf.Max(
                            Mathf.Abs(Targeting.Position.x-PlayerMovement.playerMovement.position.x),
                            Mathf.Abs(Targeting.Position.y-PlayerMovement.playerMovement.position.y))>LastSkill.Range&&LastSkill.Range!=-1))
                    {
                        Debug.Log("revert targeting");
                        Targeting.Revert();
                    }

                    if (LastSkill.IsValidTarget())
                    {
                        MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = "<color=yellow>*</color>";
                    }
                    else
                    {
                        MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = $"<color=#6b6b6b>*</color>";
                    }
                    LastSkill.DrawPreperation();

                    Selector.Current.SelectedTile(Targeting.Position.x, Targeting.Position.y);
                    
                    if (Controls.GetKeyDown(Controls.Inputs.Use))
                    {
                        if (LastSkill.IsValidTarget())
                        {
                            playerStats.__blood -= LastSkill.BloodCost;
                            LastSkill.Activate(playerStats);
                            CloseEQ();                            
                            dungeonGenerator.DrawMap(MapManager.map);
                        }
                    }
                    if (Controls.GetKeyDown(Controls.Inputs.CancelButton))
                    {
                        SkillCasting = false;
                        CloseEQ();
                        dungeonGenerator.DrawMap(MapManager.map);
                    }
                    MapManager.NeedRepaint = true;
                }
            }

            if (IsThrowingItem)
            {
                if (!ItemThrowHelper.AllowTargetingMove() ||
                        (
                        Mathf.Max(
                            Mathf.Abs(Targeting.Position.x - PlayerMovement.playerMovement.position.x),
                            Mathf.Abs(Targeting.Position.y - PlayerMovement.playerMovement.position.y)) > ItemThrowHelper.MaxRange && ItemThrowHelper.MaxRange != -1))
                {
                    Debug.Log("revert targeting");
                    Targeting.Revert();
                }

                if (ItemThrowHelper.IsValidTarget())
                {
                    MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = "<color=yellow>*</color>";
                }
                else
                {
                    MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = $"<color=#6b6b6b>*</color>";
                }
                
                Selector.Current.SelectedTile(Targeting.Position.x, Targeting.Position.y);

                if (Controls.GetKeyDown(Controls.Inputs.Use))
                {
                    if (ItemThrowHelper.IsValidTarget())
                    {
                        ItemThrowHelper.Activate(playerStats);
                        CloseEQ();
                        dungeonGenerator.DrawMap(MapManager.map);
                        dungeonGenerator.DrawMap(MapManager.map);
                        IsThrowingItem = false;
                    }
                }
                if (Controls.GetKeyDown(Controls.Inputs.CancelButton))
                {
                    IsThrowingItem = false;
                    CloseEQ();
                    dungeonGenerator.DrawMap(MapManager.map);
                }
                MapManager.NeedRepaint = true;
            }
        }

        /*if(decidingSpell)
        {
            if (Controls.GetKeyDown(Controls.Inputs.InventoryChoice1))
            {
                if(playerStats.rememberedSpells[selectedItem] is SpellbookSO book)
                {
                    book.CastSpell(playerStats);
                }
                decisionMade = true;
            }
        }*/

        if (Popup.Showing)
        {
            if (Popup.CheckDecission())
            {
                // the user clicked something
                
                // FinishPlayersTurn();
            }
            else
            {
                // the user didnt click anything
            }
        }


        /*
        if (deciding && !choosingWeapon)
        {
            if (Controls.GetKeyDown(Controls.Inputs.InventoryChoice1)) //drop item
            {
                if (itemOption1 !="")
                {
                    try { SendMessage(itemOption1); } catch { }
                }                
            }
            else if (Controls.GetKeyDown(Controls.Inputs.InventoryChoice2))
            {
                if (itemOption2 != "")
                {
                    try { SendMessage(itemOption2); } catch { }
                }                
            }
            else if (Controls.GetKeyDown(Controls.Inputs.InventoryChoice3))
            {
                if (itemOption3 != "")
                {
                    try { SendMessage(itemOption3); } catch { }
                }                
            }

            if (Controls.GetKeyDown(Controls.Inputs.CancelButton) || Controls.GetKeyDown(Controls.Inputs.InventoryOpen))
            {
                decisionMade = true;
                FinishPlayersTurn();
            }
        }
        */

        /*
        if (decisionMade)   
        {
            deciding = false;
            decidingSpell = false;
            GOdec.SetActive(false);            
        }
        */

        if (playerStats.isDead)
        {
            string _new = playerStats.deadText.Replace("12345", System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString());
            dungeonGenerator.screen.text = _new;
            

            dungeonGenerator.deathScreen.text = "<color=white>" + RunManager.GetResultScreen() + "</color>";

            if (Controls.GetKeyDown(Controls.Inputs.Use))
            {
                playerStats.ResultStatus = -1;
                // we have shown them the result screen and therefor load the main menu
                Application.LoadLevel(0);
            }
        }   
        
        if(Controls.GetKeyDown(Controls.Inputs.CheatOpen))
        {
            cheatMenu = true;
        }
        
        if(cheatMenu)
        {
            if(Controls.GetKeyDown(Controls.Inputs.CheatActivate))
            {
                cheatMenu = false;

                if(cheatString.Substring(0,2) == "tp")
                {
                    string[] floor = cheatString.Split(new string[] { "/" }, StringSplitOptions.None);
                    int _floor = int.Parse(floor[1]);
                    MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].hasPlayer = false;
                    MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].letter = "";

                    MapManager.ChangeFloor(_floor);
                }
                else if (cheatString.Substring(0, 2) == "st")
                {
                    if(MapManager.CurrentFloorIndex != 0)
                    {
                        UpdateMessages(MapManager.GetFloor(MapManager.CurrentFloorIndex-1).StairsDown.x + " " + MapManager.GetFloor(MapManager.CurrentFloorIndex - 1).StairsDown.y);                
                    }

                    UpdateMessages(MapManager.GetFloor(MapManager.CurrentFloorIndex ).StairsUp.x + " " + MapManager.GetFloor(MapManager.CurrentFloorIndex).StairsUp.y);

                    dungeonGenerator.DrawMap(MapManager.map);
                }
                else if (cheatString.Substring(0, 2) == "fv")
                {
                    playerStats.FullVision();
                }   
                else if(cheatString.Substring(0, 5) == "spawn")
                {
                    string[] enemy = cheatString.Split(new string[] {"/"}, StringSplitOptions.None);
                    enemySpawner.SpawnAt(MapManager.CurrentFloor, MapManager.playerPos.x, MapManager.playerPos.y - 1, enemySpawner.allEnemies.Where(obj => obj.name == enemy[1]).SingleOrDefault());
                }
                else if(cheatString.Substring(0, 5) == "level")
                {
                    string[] level = cheatString.Split(new string[] {"/"}, StringSplitOptions.None);
                    int _level = int.Parse(level[1]);
                    for(int i = 0; i < _level; i++)
                    {
                        playerStats.UpdateLevel(playerStats.__experienceNeededToLvlUp);
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (MapManager.NeedRepaint)
        {
            MapManager.NeedRepaint = false;
            dungeonGenerator.DrawMap(MapManager.map);
        }
    }


    public IEnumerator DestroyTorchEffect(Vector2Int _pos)
    {
        Vector2Int pos = _pos;

        int x = 0;

        for (int i = 1; i < 60; i++)
        {
            x = pos.x;
            for (int i1 = pos.y + i; i1 >= pos.y; i1--)
            {
                try { MapManager.map[x, i1].decoy = "<color=red>*</color>"; }
                catch { }

                try { MapManager.map[x, pos.y + (pos.y - i1)].decoy = "<color=red>*</color>"; }
                catch { }

                try { MapManager.map[x, i1].decoy = "<color=red>*</color>"; }
                catch { }

                try { MapManager.map[x, pos.y + (pos.y - i1)].decoy = "<color=red>*</color>"; }
                catch { }
                x++;
            }

            x = pos.x;

            for (int i1 = pos.y + i; i1 >= pos.y; i1--)
            {
                try { MapManager.map[x, i1].decoy = "<color=red>*</color>"; }
                catch { }

                try { MapManager.map[x, pos.y + (pos.y - i1)].decoy = "<color=red>*</color>"; }
                catch { }

                try { MapManager.map[x, i1].decoy = "<color=red>*</color>"; }
                catch { }

                try { MapManager.map[x, pos.y + (pos.y - i1)].decoy = "<color=red>*</color>"; }
                catch { }
                x--;
            }
            yield return new WaitForSeconds(0.001f);
            DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
        }

        yield return null;
    }

    private void Drop()
    {
        Vector2Int posToDrop = new Vector2Int(1000, 1000);

        if (_selectedItem.cursed && _selectedItem.equippedPreviously)
        {
            UpdateMessages("You can't drop this item because it is <color=red>cursed</color>!");
            FinishPlayersTurn();
            return;
        }

        if (_selectedItem.isEquipped)
        {
            _selectedItem.iso.onUnequip(playerStats);
            _selectedItem.OnUnequip(playerStats);
        }

        if (CanItemBeDroppedHere(MapManager.playerPos)) posToDrop = MapManager.playerPos;
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x - 1, MapManager.playerPos.y))) posToDrop = new Vector2Int(MapManager.playerPos.x - 1, MapManager.playerPos.y);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x + 1, MapManager.playerPos.y))) posToDrop = new Vector2Int(MapManager.playerPos.x + 1, MapManager.playerPos.y);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x, MapManager.playerPos.y - 1))) posToDrop = new Vector2Int(MapManager.playerPos.x, MapManager.playerPos.y - 1);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x, MapManager.playerPos.y + 1))) posToDrop = new Vector2Int(MapManager.playerPos.x, MapManager.playerPos.y + 1);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x - 1, MapManager.playerPos.y - 1))) posToDrop = new Vector2Int(MapManager.playerPos.x - 1, MapManager.playerPos.y - 1);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x + 1, MapManager.playerPos.y + 1))) posToDrop = new Vector2Int(MapManager.playerPos.x + 1, MapManager.playerPos.y + 1);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x + 1, MapManager.playerPos.y - 1))) posToDrop = new Vector2Int(MapManager.playerPos.x + 1, MapManager.playerPos.y - 1);
        else if (CanItemBeDroppedHere(new Vector2Int(MapManager.playerPos.x - 1, MapManager.playerPos.y + 1))) posToDrop = new Vector2Int(MapManager.playerPos.x - 1, MapManager.playerPos.y + 1);
        else
        {
            UpdateMessages("There is no space around you.");
            FinishPlayersTurn();
            return;
        }

        if (posToDrop != new Vector2Int(1000, 1000))
        {
            if (GetComponent<RevivingMonster>().TestRevive(MapManager.playerPos, _selectedItem))
            {
                UpdateInventoryText();
                FinishPlayersTurn();
                ApplyChangesInInventory(_selectedItem.iso);
                return;
            }
            else
            {
                if (ColorUtility.TryParseHtmlString(_selectedItem.iso.I_color, out Color color))
                {
                    MapManager.map[posToDrop.x, posToDrop.y].exploredColor = color;
                }
                MapManager.map[posToDrop.x, posToDrop.y].baseChar = _selectedItem.iso.I_symbol;

                if (_selectedItem.iso is WeaponsSO bloodSword && _selectedItem.iso.I_name == "Bloodsword") tasks.everyTurnTasks -= bloodSword.BloodswordDecreaseDamage;
                // TODO: there might be gameobject beein left behind here
                GameObject item = Instantiate(_selectedItem.gameObject, transform.position, Quaternion.identity);
                item.GetComponent<Item>().iso = _selectedItem.iso;
                item.transform.SetParent(MapManager.CurrentFloor.GO.transform);
                item.GetComponent<Item>().isEquipped = false;
                MapManager.map[posToDrop.x, posToDrop.y].item = item.gameObject;
            }
        }

        if (_selectedItem.iso.I_whereToPutIt != ItemScriptableObject.whereToPutIt.none && posToDrop != new Vector2Int(1000, 1000)) //drop equipped item
        {
            if (_selectedItem.isEquipped)
            {
                _selectedItem.isEquipped = false;
                switch (_selectedItem.iso.I_whereToPutIt)
                {
                    case ItemScriptableObject.whereToPutIt.head:
                        playerStats._head = null;
                        playerStats.Head.text = "Helm:";
                        break;
                    case ItemScriptableObject.whereToPutIt.body:
                        playerStats._body = null;
                        playerStats.Body.text = "Chest:";
                        break;
                    case ItemScriptableObject.whereToPutIt.hand:
                        if (_selectedItem._handSwitch == Item.hand.left)
                        {
                            playerStats._Lhand = null;
                            playerStats.LHand.text = "Left Hand:";
                            /*if (_selectedItem.iso is WeaponsSO weapon)
                            {
                                _selectedItem.UnequipWithGems();
                            }*/
                        }
                        else if (_selectedItem._handSwitch == Item.hand.right)
                        {
                            playerStats._Rhand = null;
                            playerStats.RHand.text = "Right Hand:";
                            /*if (_selectedItem.iso is WeaponsSO weapon)
                            {
                                _selectedItem.UnequipWithGems();
                            }*/
                        }
                        break;
                    case ItemScriptableObject.whereToPutIt.legs:
                        playerStats._legs = null;
                        playerStats.Legs.text = "Legs:";
                        break;
                    case ItemScriptableObject.whereToPutIt.ring:
                        if (playerStats._ring.iso is RingSO ring)
                        {
                            ring.Dequip(playerStats);
                        }
                        playerStats._ring = null;
                        playerStats.Ring.text = "Ring:";
                        break;
                }
            }

            _selectedItem.isEquipped = false;

            if (_selectedItem.identified)
            {
                UpdateMessages($"You dropped <color={_selectedItem.iso.I_color}>{_selectedItem.iso.I_name}</color>.");
            }
            else
            {
                UpdateMessages($"You dropped <color={_selectedItem.iso.I_color}>{_selectedItem.iso.I_unInName}</color>.");
            }
            UpdateInventoryText();
            ApplyChangesInInventory(_selectedItem.iso);
        }
        else
        {
            if (posToDrop != new Vector2Int(1000, 1000))
            {
                if (_selectedItem.identified)
                {
                    UpdateMessages($"You dropped <color={_selectedItem.iso.I_color}>{_selectedItem.iso.I_name}</color>.");
                }
                else
                {
                    UpdateMessages($"You dropped <color={_selectedItem.iso.I_color}>{_selectedItem.iso.I_unInName}</color>.");
                }
                UpdateInventoryText();
                ApplyChangesInInventory(_selectedItem.iso);
            }
        }

        FinishPlayersTurn();
    }

    private void UEquip() //Unequip and Equip
    {
        if (_selectedItem.iso.I_whereToPutIt != ItemScriptableObject.whereToPutIt.none) //unequip or equip
        {
            if (equipState) //unqeuip
            {
                if (_selectedItem.cursed)
                {
                    UpdateMessages("You can't unequip this item becuase it's <color=red>cursed</color>.");
                }
                else
                {
                    switch (_selectedItem.iso.I_whereToPutIt)
                    {
                        case ItemScriptableObject.whereToPutIt.head:
                            playerStats._head = null;
                            UpdateEquipment(playerStats.Head, "<color=#ffffff>Helm: </color>");

                            if (_selectedItem.iso is ArmorSO armor)
                            {
                                armor.Use(playerStats, _selectedItem);
                            }
                            _selectedItem.isEquipped = false;
                            break;
                        case ItemScriptableObject.whereToPutIt.body:
                            playerStats._body = null;
                            UpdateEquipment(playerStats.Body, "<color=#ffffff>Chest: </color>");

                            if (_selectedItem.iso is ArmorSO armor1)
                            {
                                armor1.Use(playerStats, _selectedItem);
                            }
                            _selectedItem.isEquipped = false;
                            break;
                        case ItemScriptableObject.whereToPutIt.hand:
                            if (_selectedItem._handSwitch == Item.hand.left)
                            {
                                playerStats._Lhand = null;
                                playerStats.LHand.text = "Left Hand:";
                                if (_selectedItem.isEquipped) _selectedItem.isEquipped = false;
                                /*if (_selectedItem.iso is WeaponsSO weapon)
                                {
                                    _selectedItem.UnequipWithGems();
                                }*/
                            }
                            else if (_selectedItem._handSwitch == Item.hand.right)
                            {
                                playerStats._Rhand = null;
                                playerStats.RHand.text = "Right Hand:";
                                if (_selectedItem.isEquipped) _selectedItem.isEquipped = false;
                                /*if (_selectedItem.iso is WeaponsSO weapon)
                                {
                                    _selectedItem.UnequipWithGems();
                                }*/
                            }
                            break;
                        case ItemScriptableObject.whereToPutIt.ring:

                            playerStats._ring = null;
                            UpdateEquipment(playerStats.Ring, "<color=#ffffff>Ring: </color>");
                            _selectedItem.isEquipped = false;
                            break;
                        case ItemScriptableObject.whereToPutIt.legs:
                            playerStats._legs = null;
                            UpdateEquipment(playerStats.Legs, "<color=#ffffff>Legs: </color>");

                            if (_selectedItem.iso is ArmorSO armor2)
                            {
                                armor2.Use(playerStats, _selectedItem);
                            }
                            _selectedItem.isEquipped = false;
                            break;
                    }
                    UpdateInventoryText();
                    _selectedItem.iso.onUnequip(playerStats);
                    _selectedItem.OnUnequip(playerStats);
                }
            }
            else //equip
            {
                if (!_selectedItem.identified)
                {
                    playerStats.itemsInEqGO[selectedItem].identified = true; //make item identifyied
                    UpdateItemStats(playerStats.itemsInEqGO[selectedItem]); //show full statistics
                    UpdateInventoryQueue(null);
                }

                if (_selectedItem.cursed) UpdateMessages($"You wince as your grip involuntarily tightens around your {_selectedItem.iso.I_name}.");

                _selectedItem.equippedPreviously = true;

                switch (_selectedItem.iso.I_whereToPutIt)
                {
                    case ItemScriptableObject.whereToPutIt.head:
                        if (playerStats._head)
                        {
                            UpdateMessages("You are already wearing something here. (<color=red>Unequip it first</color>)");
                        }
                        else
                        {
                            playerStats._head = _selectedItem;
                            UpdateEquipment(playerStats.Head, "<color=#00FFFF>Helm: </color>" + "\n" + " " + playerStats._head.iso.I_name);

                            if (_selectedItem.iso is ArmorSO armor)
                            {
                                armor.Use(playerStats, _selectedItem);
                            }
                            _selectedItem.isEquipped = true;
                            _selectedItem.iso.onEquip(playerStats);
                            _selectedItem.OnEquip(playerStats);
                        }

                        break;
                    case ItemScriptableObject.whereToPutIt.body:
                        if (playerStats._body)
                        {
                            UpdateMessages("You are already wearing something here. (<color=red>Unequip it first</color>)");
                        }
                        else
                        {
                            playerStats._body = _selectedItem;
                            UpdateEquipment(playerStats.Body, "<color=#00FFFF>Chest: </color>" + "\n" + " " + playerStats._body.iso.I_name);

                            if (_selectedItem.iso is ArmorSO armor1)
                            {
                                armor1.Use(playerStats, _selectedItem);
                            }
                            _selectedItem.isEquipped = true;
                            _selectedItem.iso.onEquip(playerStats);
                            _selectedItem.OnEquip(playerStats);
                        }

                        break;
                    case ItemScriptableObject.whereToPutIt.hand:
                        if (playerStats._Lhand == null)
                        {
                            playerStats._Lhand = _selectedItem;
                            UpdateEquipment(playerStats.LHand, "<color=#00FFFF>Left Hand: </color>" + "\n" + " " + playerStats._Lhand.iso.I_name);
                            _selectedItem.isEquipped = true;
                            _selectedItem._handSwitch = Item.hand.left;

                            if (_selectedItem.iso is WeaponsSO weapon)
                            {
                                //_selectedItem.EquipWithGems(_selectedItem);
                                _selectedItem.iso.onEquip(playerStats);
                                _selectedItem.OnEquip(playerStats);
                            }
                        }
                        else if (playerStats._Rhand == null)
                        {
                            playerStats._Rhand = _selectedItem;
                            UpdateEquipment(playerStats.RHand, "<color=#00FFFF>Right Hand: </color>" + "\n" + " " + playerStats._Rhand.iso.I_name);
                            _selectedItem.isEquipped = true;
                            _selectedItem._handSwitch = Item.hand.right;
                            if (_selectedItem.iso is WeaponsSO weapon)
                            {
                                //_selectedItem.EquipWithGems(_selectedItem);
                                _selectedItem.iso.onEquip(playerStats);
                                _selectedItem.OnEquip(playerStats);
                            }
                        }
                        else
                        {
                            UpdateMessages("You have no free hand.");
                        }
                        break;
                    case ItemScriptableObject.whereToPutIt.ring:
                        if (playerStats._ring)
                        {
                            UpdateMessages("You are already wearing something here. (<color=red>Unequip it first</color>)");
                        }
                        else
                        {
                            playerStats._ring = _selectedItem;
                            UpdateEquipment(playerStats.Ring, "<color=#00FFFF>Ring: </color>" + "\n" + " " + playerStats._ring.iso.I_name);

                            if (_selectedItem.iso is RingSO ring)
                            {
                                ring.Use(playerStats, _selectedItem);
                            }
                            ApplyChangesInInventory(null);
                            _selectedItem.isEquipped = true;
                            _selectedItem.iso.onEquip(playerStats);
                            _selectedItem.OnEquip(playerStats);
                        }

                        break;
                    case ItemScriptableObject.whereToPutIt.legs:
                        if (playerStats._legs)
                        {
                            UpdateMessages("You are already wearing something here. (<color=red>Unequip it first</color>)");
                        }
                        else
                        {

                            playerStats._legs = _selectedItem;
                            UpdateEquipment(playerStats.Legs, "<color=#00FFFF>Legs: </color>" + "\n" + " " + playerStats._legs.iso.I_name);
                            _selectedItem.iso.onEquip(playerStats);
                            _selectedItem.OnEquip(playerStats);

                            if (_selectedItem.iso is ArmorSO armor2)
                            {
                                armor2.Use(playerStats, _selectedItem);
                            }
                            _selectedItem.isEquipped = true;
                        }

                        break;
                }
                UpdateInventoryText();
            }
            FinishPlayersTurn();
        }
        else
        {
            if (_selectedItem.iso is Gem gem)
            {
                gemToConnect = _selectedItem.iso;
                UpdateMessages("Choose weapon. (ESC to cancel)");
                choosingWeapon = true;
            }
            else
            {
                _selectedItem.iso.Use(playerStats, _selectedItem);
                ApplyChangesInInventory(null);
                FinishPlayersTurn();
            }
        }
    }

    private void Use()
    {
        if (_selectedItem.iso is Gem gem)
        {
            gemToConnect = _selectedItem.iso;
            UpdateMessages("Choose weapon.");
            choosingWeapon = true;
        }
        else
        {
            _selectedItem.iso.Use(playerStats, _selectedItem);

            if(_selectedItem.iso is PotionSO potion)
            {
                UpdateMessages($"You drank the <color={potion.I_color}>{potion.I_name}</color>.");
            }

            if (playerStats.isBlind && _selectedItem.iso is ScrollSO scr || _selectedItem.iso is SpellbookSO spl)
            {
                ApplyChangesInInventory(null);
                
                FinishPlayersTurn();
            }
            else
            {
                _selectedItem.identified = true;

                UpdateItemStats(_selectedItem);

                ApplyChangesInInventory(null);
                
                FinishPlayersTurn();
            }
        }
    }

    public void LearnSkill(SkillScriptableObject skill)
    {
        if (!ExistingSkills.Contains(skill))
        {
            ExistingSkills.Add(skill);
        }
    }
    public void UnlearnSkill(SkillScriptableObject skill)
    {
        ExistingSkills.Remove(skill);
        if (LastSkill==skill)
        {
            LastSkill = null;
        }
    }

    public void ThrowItem()
    {
        CloseEQ();
        Targeting.IsTargeting = true;
        IsThrowingItem = true;
        ItemThrowHelper.PrepareItem(playerStats,_selectedItem);
        UpdateMessages("Choose a target for the throw");
    }

    private void Upgrade()
    {
        itemToAnvil = _selectedItem.GetComponent<Item>();
        anvil.UseAnvil();
    }

    bool CanItemBeDroppedHere(Vector2Int pos)
    {
        if(MapManager.map[pos.x, pos.y].item == null && MapManager.map[pos.x, pos.y].isWalkable && MapManager.map[pos.x, pos.y].enemy == null && MapManager.map[pos.x, pos.y].type != "Door" && MapManager.map[pos.x, pos.y].structure == null)
        {
            return true;
        }
        else return false;
    }
    bool choosingWeapon;
    Item choosenWeapon;
    [HideInInspector] public ItemScriptableObject gemToConnect;

    void OnGUI()
    {
        if(cheatMenu)
        {
            // Make a text field that modifies stringToEdit.
            cheatString = GUI.TextField(new Rect(10, 10, 200, 20), cheatString, 25);
        }       
    }

    public IEnumerator WaitTurn(int turns = 0)
    {
        waiting = true;

        for (int i = 0; i < turns; i++)
        {         
            yield return new WaitForSeconds(0.15f);
            FinishPlayersTurn();
        }

        /*if (readingBook != null)
        {
            if (UnityEngine.Random.Range(1, 36) > playerStats.__intelligence)
            {
                int randomBadEffect = UnityEngine.Random.Range(1, 3);

                if (randomBadEffect == 1)
                {
                    UpdateMessages("<color=red>As you read the book, it radiates explosive energy in your face!</color>");
                    playerStats.TakeDamage(UnityEngine.Random.Range(3, 8),ItemScriptableObject.damageType.normal);
                }
                else if (randomBadEffect == 2)
                {
                    UpdateMessages("<color=red>As you read the book, you feel a wrenching sensation.</color>");
                    int randomX = 0;
                    int randomY = 0;

                    bool loopBreaker = false;

                    for (int i = 0; i < 500; i++) //500 is nuber of tries
                    {
                        if (loopBreaker) break;

                        randomX = UnityEngine.Random.Range(1, DungeonGenerator.dungeonGenerator.mapWidth);
                        randomY = UnityEngine.Random.Range(1, DungeonGenerator.dungeonGenerator.mapHeight);

                        if (MapManager.map[randomX, randomY].isWalkable && MapManager.map[randomX, randomY].enemy == null && MapManager.map[randomX, randomY].structure == null)
                        {
                            MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].hasPlayer = false;
                            MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].letter = "";
                            MapManager.map[MapManager.playerPos.x, MapManager.playerPos.y].timeColor = new Color(0, 0, 0);
                            PlayerMovement.playerMovement.position = new Vector2Int(randomX, randomY);
                            MapManager.map[randomX, randomY].hasPlayer = true;
                            MapManager.map[randomX, randomY].letter = "<color=green>@</color>";
                            MapManager.map[randomX, randomY].timeColor = new Color(0.5f, 1, 0);
                            MapManager.playerPos = new Vector2Int(randomX, randomY);

                            loopBreaker = true;

                            DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
                        }
                    }
                }
                ApplyChangesInInventory(readingBook);
                readingBook = null;
            }
            else
            {
                UpdateMessages($"<color=lightblue>Puff!</color> You have succesfully read the <color={readingBook.I_color}>{readingBook.I_name}</color>. It fades away...");
                playerStats.rememberedSpells.Add(readingBook);
                UpdateGrimoireQueue($"<color={readingBook.I_color}>{readingBook.I_name}</color>");
                ApplyChangesInInventory(readingBook);
                readingBook = null;
            }
        }*/

        waiting = false;
        waitingCoroutine = null;
        StopAllCoroutines();
    }

    public void OpenEQ()
    {
        UpdateInventoryText();
        selectedItem = 0;
        selector.GetComponent<Text>().enabled = true;
        player.canMove = false;
        inventoryOpen = true;
        invBorder.SetActive(true);
        mapText.SetActive(false);
        mainUItext.enabled = false;
        selector.GetComponent<Text>().enabled = true;
        try { UpdateItemStats(playerStats.itemsInEqGO[0]); }
        catch { }
    }

    public void CloseEQ()
    {
        Targeting.IsTargeting = false;

        GameManager.manager.anvil = null;
        if (choosingWeapon) choosingWeapon = false;
        selectedItem = 0;
        player.canMove = true;
        inventoryOpen = false;

        openGrimoire = false;

        SkillsOpen = false;
        SkillCasting = false;


        invBorder.SetActive(false);
        mapText.SetActive(true);
        mainUItext.enabled = true;
        selector.anchoredPosition = new Vector3(selector.anchoredPosition.x, -284, 0);
        selector.GetComponent<Text>().enabled = false;
        anvilMenuOpened = false;
    }

    public void FirstTurn()
    {
        fv.Compute(MapManager.playerPos, playerStats.viewRange);

        isPlayerTurn = true;

        GetComponent<Tasks>().EventsOnStartOfTheGame();

        DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);

        UpdateMessages("<i><b><color=purpe>You awaken in unfamiliar territory. Your head hurts and it stinks. You have no idea what has happened to you or what is going on.</color></b></i>");

        UpdateMessages("<color=yellow>Press</color> / <color=yellow>for controls.</color>");
    }

    public void StartPlayersTurn()
    {
        isPlayerTurn = true;
        OnPlayerTurnStarts();

        foreach(var item in playerStats.itemsInEqGO)
        {
            if(item.iso is SpellbookSO book)
            {
                if(item.spellbookCooldown > 0)
                {
                    item.spellbookCooldown--;
                }
            }
        }
    }

    public void OnPlayerTurnStarts()
    {
        /*
         * when player turn starts do all 
         * tasks that should be doone when player turn starts
        */
        playerTurnTasks?.Invoke();
        tasks.DoTasks();
    }

    public void DecisionTurn()
    {

    }

    public void FinishPlayersTurn()
    {
        isPlayerTurn = false;

        fv.Compute(MapManager.playerPos, playerStats.viewRange);

        dungeonGenerator.DrawMap(MapManager.map);
        
        turns++;

        EnemyTurn();
    }

    public void EnemyTurn()
    {
        foreach(var enemy in enemies.ToList())
        {
            if (enemy!=null && enemy.gameObject.activeInHierarchy)
            {
                enemy.GetComponent<RoamingNPC>()?.DoTurn();
                enemy.GetComponent<OmniBehaviour>()?.AI.Calculate(enemy.GetComponent<OmniBehaviour>());
            }
        }

        dungeonGenerator.DrawMap(MapManager.map);

        StartPlayersTurn();
    }

    public void DeadTurn()
    {

    }

    public void UpdateEquipment(Text _text, string _name)
    {
        _text.text = _name;
    }

    public void UpdateMessages(string newMessage)
    {
        if (m_Messages.Count >= 8)
        {
            m_Messages.Dequeue();
        }

        m_Messages.Enqueue(newMessage);

        UpdateText();
    } 

    public void UpdateText()
    {
        messagesText = "";

        foreach(System.Object obj in m_Messages)
        {
            String str = obj as String;
            messagesText = (messagesText + "\n" + str);
        }
        messages.text = messagesText;
    }

    public void InitSkillWindow()
    {
        SkillsOpen = true;
        SkillCasting = false;

        SelectedSkillIndex = 0;
        TopVisibleSkillIndex = 0;

        player.canMove = false;

        invBorder.SetActive(true);
        mapText.SetActive(false);
        mainUItext.enabled = false;
        //selector.GetComponent<Text>().enabled = true;
    }

    public void CheckSkillLearning()
    {
        foreach (var skill in ExistingSkills)
        {
            if (skill.IsShown(playerStats))
            {
                if (!LearnedSkills.Contains(skill))
                {
                    // the skill has not been learned before so we have inform the player that they learned a new skill
                    LearnedSkills.Add(skill);
                    UpdateMessages(skill.LearnMessage);
                }
            }
        }
    }

    public void UpdateSkillText()
    {
        SkillText = "";

        VisibleSkills.Clear();
        foreach (var skill in ExistingSkills)
        {
            if (skill.IsShown(playerStats))
            {
                VisibleSkills.Add(skill);
            }
        }
        // we cant select nothin

        SelectedSkillIndex = Mathf.Clamp(SelectedSkillIndex, 0, VisibleSkills.Count - 1);
        TopVisibleSkillIndex = Mathf.Clamp(TopVisibleSkillIndex, SelectedSkillIndex - 15, SelectedSkillIndex);

        for (int i = TopVisibleSkillIndex; i < Mathf.Min(VisibleSkills.Count,TopVisibleSkillIndex + 15); i++)
        {
            if (i == SelectedSkillIndex)
            {
                SkillText += "> ";
            }
            else
            {
                SkillText += "  ";
            }
            if (VisibleSkills[i].IsCastable(playerStats)&&playerStats.__blood>=VisibleSkills[i].BloodCost)
            {
                SkillText += $"<color={VisibleSkills[i].DisplayColor}>{VisibleSkills[i].Name}</color>";
            }
            else
            {
                SkillText += "<color=#6b6b6b>" + VisibleSkills[i].Name + "</color>";
            }
            SkillText += "\n";
        }

        inventory.text = SkillText;
    }

    /*public void UpdateGrimoireQueue(string newSpell)
    {
        if (m_grimoire.Count >= 17)
        {
            m_grimoire.Dequeue();
        }

        m_grimoire.Enqueue(newSpell);


        UpdateGrimoireText();
    }*/

    /*public void UpdateGrimoireText()
    {
        grimoireText = "";

        int index = 0;

        foreach (System.Object obj in m_grimoire)
        {
            String str = obj as String;
            try
            {
                grimoireText = grimoireText + str + "\n";
                index++;
            }
            catch
            {

            }
        }
        inventory.text = grimoireText;
    }*/
    
    public void UpdateInventoryQueue(string newItem)
    {
        if (m_Inventory.Count >= 17)
        {
            m_Inventory.Dequeue();
        }

        m_Inventory.Enqueue(newItem);
        

        UpdateInventoryText();
    }

    public void UpdateInventoryText()
    {
        inventoryText = "";

        int index = 0;

        foreach (System.Object obj in m_Inventory)
        {
            String str = obj as String;
            try
            {
                if(playerStats.itemsInEqGO[index].isEquipped)
                {
                    inventoryText = inventoryText + "(E)" + str + "\n";
                }
                else
                {
                    inventoryText = inventoryText + str + "\n";
                }
                index++;    
            }
            catch
            {

            }
        }
        inventory.text = inventoryText;
    }

    public void UpdateItemStats(Item item = null)
    {
        if (item == null)
        {
            itemName.text = "";
            itemType.text = "";
            itemEffects.text = "";
            itemRare.text = "";
        }
        else
        {
            if (item.iso is PotionSO potion && potion.previouslyDrank)
            {
                item.identified = true;
            }

            if (item.identified) itemName.text = $"<color={item.iso.I_color}>{item.iso.I_name}</color>";
            else itemName.text = $"<color=purple>{item.iso.I_unInName}</color>";

            itemType.text = item.iso.I_itemType.ToString();

            if (item.identified)
            {
                itemEffects.text = "";
                if(item.iso.effect != "")
                {
                    itemEffects.text = item.iso.effect + "\n";                 
                }
                if (item.cursed == true && item.equippedPreviously)
                {
                    itemEffects.text += "<color=red>Cursed</color> \n";
                }
                /*itemEffects.text =
                    $"{item.iso.effect}" +
                    "\n" +
                    (item.cursed == true && item.equippedPreviously ? "<color=red>Cursed</color> \n" : "\n");*/

                if (item.iso is PotionSO) { }
                else
                {
                    if (item._BUC == Item.BUC.cursed) itemEffects.text += "<color=red>Cursed</color> \n";
                    else if (item._BUC == Item.BUC.blessed) itemEffects.text += "<color=yellow>Blessed</color> \n";
                }

                if (item.iso.bonusToHealth != 0) itemEffects.text += "<color=red>HP</color>: " + (item.iso.bonusToHealth + item.Anvil_bonusToHealth) + "\n";
                if (item.iso.bonusToStrength != 0) itemEffects.text += "<color=red>STR</color>: " + (item.iso.bonusToStrength + item.Anvil_bonusToStrength) + "\n";
                if (item.iso.bonusToIntelligence != 0) itemEffects.text += "<color=red>INT</color>: " + (item.iso.bonusToIntelligence + item.Anvil_bonusToIntelligence) + "\n";
                if (item.iso.bonusToDexterity != 0) itemEffects.text += "<color=red>DEX</color>: " + (item.iso.bonusToDexterity + item.Anvil_bonusToDexterity) + "\n";
                if (item.iso.bonusToEndurance != 0) itemEffects.text += "<color=red>END</color>: " + (item.iso.bonusToEndurance + item.Anvil_bonusToEndurance) + "\n";
                if (item.iso.bonusToNoise != 0) itemEffects.text += "<color=red>Noise</color>: " + (item.iso.bonusToNoise + item.Anvil_bonusToNoise) + "\n";

                itemEffects.text += "Weight: " + $"<color=green>{item.iso.I_weight}</color>" + "\n";
            }
            else itemEffects.text = "???" + "\n" + "Weight: " + $"<color=green>{item.iso.I_weight}</color>";

            /*if (item.sockets == 1)
            {
                itemEffects.text += "\n" + "Socket: " + (item.socket1 == null ? "" : $"<color={item.socket1.I_color}>{item.socket1.I_name}</color>");
            }
            else if (item.sockets == 2)
            {
                itemEffects.text += "\n" + "Socket: " + (item.socket1 == null ? "" : $"<color={item.socket1.I_color}>{item.socket1.I_name}</color>");
                itemEffects.text += "\n" + "Socket: " + (item.socket2 == null ? "" : $"<color={item.socket2.I_color}>{item.socket2.I_name}</color>");
            }
            else if (item.sockets == 3)
            {
                itemEffects.text += "\n" + "Socket: " + (item.socket1 == null ? "" : $"<color={item.socket1.I_color}>{item.socket1.I_name}</color>");
                itemEffects.text += "\n" + "Socket: " + (item.socket2 == null ? "" : $"<color={item.socket2.I_color}>{item.socket2.I_name}</color>");
                itemEffects.text += "\n" + "Socket: " + (item.socket3 == null ? "" : $"<color={item.socket3.I_color}>{item.socket3.I_name}</color>");
            }*/

            if (item.identified)
            {
                switch (item.iso.I_rareness)
                {
                    case ItemScriptableObject.rareness.common:
                        itemRare.text = "<color=grey>Common</color>";
                        break;
                    case ItemScriptableObject.rareness.rare:
                        itemRare.text = "<color=green>Rare</color>";
                        break;
                    case ItemScriptableObject.rareness.very_rare:
                        itemRare.text = "<color=yellow>Very rare</color>";
                        break;
                    case ItemScriptableObject.rareness.mythical:
                        itemRare.text = "<color=red>Mythical</color>";
                        break;
                }
            }
            else itemRare.text = "<color=purple>???</color>";
        }
    }

    public void UpdateSkillStats()
    {
        if (SelectedSkillIndex< VisibleSkills.Count)
        {
            SkillScriptableObject skill = VisibleSkills[SelectedSkillIndex];
            itemName.text = $"<color={skill.DisplayColor}>{skill.Name}</color>";

            itemRare.text = $"Costs <color=red>{skill.BloodCost}</color> Blood";
            itemType.text = $"{skill.Range} Tile Range";

            itemEffects.text = skill.Description;
        }
        else
        {
            itemName.text = $"";

            itemRare.text = $"";
            itemType.text = $"";

            itemEffects.text = "";
        }             
    }

    public void UpdateSpellStats(ItemScriptableObject iso)
    {
        itemName.text = $"<color={iso.I_color}>{iso.I_name}</color>";
        itemType.text = iso.I_itemType.ToString();

        itemEffects.text = iso.effect;

        switch (iso.I_rareness)
        {
            case ItemScriptableObject.rareness.common:
                itemRare.text = "<color=grey>Common</color>";
                break;
            case ItemScriptableObject.rareness.rare:
                itemRare.text = "<color=green>Rare</color>";
                break;
            case ItemScriptableObject.rareness.very_rare:
                itemRare.text = "<color=yellow>Very rare</color>";
                break;
            case ItemScriptableObject.rareness.mythical:
                itemRare.text = "<color=red>Mythical</color>";
                break;
        }
    }

    public void IncreaseStat(string stat)
    {
        if(playerStats.skillpoints <= 0)
        {
            playerStats._strengthButton.SetActive(false);
            //playerStats._intelligenceButton.SetActive(false);
            playerStats._dexterityButton.SetActive(false);
            playerStats._enduranceButton.SetActive(false);
            return;
        }  

        if(stat == "str")
        {
            playerStats.__strength++;
            playerStats.UpdateText(PlayerStats.statType.strength);
            playerStats.skillpoints--;
        }
        /*else if(stat == "int")
        {
            playerStats.__intelligence++;
            playerStats.UpdateText(PlayerStats.statType.intelligence);
            playerStats.skillpoints--;
        }*/
        else if(stat == "dex")
        {
            playerStats.__dexterity++;
            playerStats.UpdateText(PlayerStats.statType.dexterity);
            playerStats.skillpoints--;
        }
        else if (stat == "end")
        {
            playerStats.__endurance++;
            playerStats.UpdateText(PlayerStats.statType.endurance);
            playerStats.skillpoints--;
        }      

        if(playerStats.skillpoints <= 0)
        {
            playerStats._strengthButton.SetActive(false);
            //playerStats._intelligenceButton.SetActive(false);
            playerStats._dexterityButton.SetActive(false);
            playerStats._enduranceButton.SetActive(false);
            return;
        } 
    }

    public void ApplyChangesInInventory(ItemScriptableObject _item)
    {
        if(_item != null)
        {
            if (playerStats.itemsInEqGO.Select(x => x.iso).ToList().Contains(_item))
            {
                int index = playerStats.itemsInEqGO.Select(x => x.iso).ToList().IndexOf(_item);              
                playerStats.itemsInEqGO.RemoveAt(index);
                playerStats.currentItems--;
            }            
        }

        m_Inventory.Clear();

        playerStats.currentWeight = 0;

        foreach (var item in playerStats.itemsInEqGO)
        {
            if (item.identified)
            {
                UpdateInventoryQueue($"<color={item.iso.I_color}>{item.iso.I_name}</color>");
            }
            else
            {
                UpdateInventoryQueue($"<color=purple>{item.iso.I_unInName}</color>");
            }

            playerStats.currentWeight += item.iso.I_weight;
        }

        if (m_Inventory.Count == 0) inventory.text = "";

        playerStats.UpdateCapacity();
        UpdateInventoryText();
    }

    public void SetDeadScreen()
    {

    }
}