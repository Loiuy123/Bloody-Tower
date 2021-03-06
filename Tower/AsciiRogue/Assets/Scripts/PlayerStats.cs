﻿using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

public class PlayerStats : MonoBehaviour, IUnit
{
    public ItemScriptableObject startingWeapon;

    public int maxHp { get; set; }
    public int currHp { get; set; }
    public int str { get; set; }
    public int intell { get; set; }
    public int dex { get; set; }
    public int end { get; set; }
    public int lvl { get; set; }
    public int sanity { get; set; }

    [HideInInspector] public int experience;
    [HideInInspector] public int experienceNeededToLvlUp;
    [HideInInspector] public int coins;
    [HideInInspector] public int blood;
    [HideInInspector] public int noise;
    [HideInInspector] public int maxWeight = 60;
    [HideInInspector] public int currentWeight;
    [HideInInspector] public int startingViewRange = 7;

    public int _viewRange;
    public int viewRange
    {
        get
        {
            return _viewRange;
        }
        set
        {
            _viewRange = RecalculateViewRange();
        }
    }
    [HideInInspector] public int maximumInventorySpace = 15;
    [HideInInspector] public int currentItems;
    [HideInInspector] public int skillpoints;
    [HideInInspector] public int ac { get; set; } = 0;

    #region Statistics
    public int __maxHp
    {
        get
        {
            return maxHp;
        }
        set
        {
            maxHp = value;
            UpdateText(statType.hp);
        }
    }
    public int __currentHp
    {
        get
        {
            return currHp;
        }
        set
        {
            currHp = value;
            if (currHp > __maxHp) currHp = __maxHp;
            UpdateText(statType.hp);
        }
    }
    public int __strength
    {
        get
        {
            return str;
        }
        set
        {
            if (value > __strength) //we gained strength
            {
                str = value;

                maxWeight += 5;

                UpdateCapacity();

                __maxHp++;
            }
            else
            {
                str = value;

                maxWeight -= 5;

                UpdateCapacity();

                __maxHp--;
            }
            UpdateText(statType.strength);
        }
    }
    public int __intelligence
    {
        get
        {
            return intell;
        }
        set
        {
            intell = value;
            //UpdateText(statType.intelligence);
        }
    }
    public int __sanity
    {
        get
        {
            return sanity;
        }
        set
        {
            sanity = value;
            if (sanity > 100) sanity = 100;
            if (sanity < 0) sanity = 0;
            UpdateText(statType.sanity);
        }
    }
    public int __dexterity
    {
        get
        {
            return dex;
        }
        set
        {
            dex = value;
            UpdateArmorClass();

            UpdateText(statType.dexterity);
        }
    }
    public int __endurance
    {
        get
        {
            return end;
        }
        set
        {
            if (value > end) //we gained end
            {
                end = value;

                __maxHp += 2;

                maxWeight++;

                UpdateCapacity();
            }
            else //we losed end
            {
                end = value;

                __maxHp -= 2;

                maxWeight--;

                UpdateCapacity();
            }
            UpdateText(statType.endurance);
        }
    }
    public int __lvl
    {
        get
        {
            return lvl;
        }
        set
        {
            lvl = value;
            //lvl = value;
            UpdateText(statType.lvl);
        }
    }
    public int __experience
    {
        get
        {
            return experience;
        }
        set
        {
            experience = value;
            //experience = value;
            UpdateText(statType.experience);
        }
    }
    public int __experienceNeededToLvlUp
    {
        get
        {
            return experienceNeededToLvlUp;
        }
        set
        {
            experienceNeededToLvlUp = value;
            //experienceNeededToLvlUp = value;
            UpdateText(statType.experience);
        }
    }
    public int __coins
    {
        get
        {
            return coins;
        }
        set
        {
            coins = value;
            UpdateText(statType.coins);
        }
    }
    public int __noise
    {
        get
        {
            return noise;
        }
        set
        {
            noise = value;
            UpdateText(statType.noise);
        }
    }
    public int __blood
    {
        get
        {
            return blood;
        }
        set
        {
            blood = value;
            if (blood > 100) blood = 100;
            if (blood < 0) Die();

            if (blood <= 25)
            {
                Blindness();
                __sanity -= 8;
            }
            if (blood <= 50 && dexModifier == 1)
            {
                dexModifier = 0.6f;
                dexLost = __dexterity - Mathf.RoundToInt(__dexterity * dexModifier);
                __dexterity -= dexLost;
                __sanity -= 5;
            }
            if (blood <= 75 && strModifier == 1)
            {
                strModifier = 0.6f;
                strLost = __strength - Mathf.RoundToInt(__strength * strModifier);
                __strength -= strLost;
                __sanity -= 5;
            }

            if (blood > 75)
            {
                if (strModifier != 1)
                {
                    strModifier = 1;
                    __strength += strLost;
                }
                if (dexModifier != 1)
                {
                    dexModifier = 1;
                    __dexterity += dexLost;
                }
                if (isBlind) CureBlindness();
            }
            else if (blood > 50)
            {
                if (dexModifier != 1)
                {
                    dexModifier = 1;
                    __dexterity += dexLost;
                }
                if (isBlind) CureBlindness();
            }
            else if (blood > 25)
            {
                if (isBlind) CureBlindness();
            }

            UpdateText(statType.blood);
        }
    }
    public int armorClass
    {
        get
        {
            return ac;
        }
        set
        {
            ac = value;
            //UpdateText(statType.ac);
        }
    }

    private float strModifier = 1;
    private float dexModifier = 1;

    private int strLost, dexLost;

    [HideInInspector] public enum statType
    {
        hp,
        strength,
        sanity,
        dexterity,
        endurance,
        lvl,
        experience,
        coins,
        ac,
        noise,
        blood
    }
    [HideInInspector] public statType _statType;

    private GameObject statsCanvas;

    #endregion

    private GameManager gameManager;  

    #region text meshes
    [Header("Text Components")]
    [UnityEngine.Serialization.FormerlySerializedAs("_nick")]
    [HideInInspector] public TextMeshProUGUI _nickText;
    [UnityEngine.Serialization.FormerlySerializedAs("_health")]
    [HideInInspector] public TextMeshProUGUI _healthText;
    [UnityEngine.Serialization.FormerlySerializedAs("_strength")]
    [HideInInspector] public TextMeshProUGUI _strengthText;
    [UnityEngine.Serialization.FormerlySerializedAs("_dexteirty")]
    [HideInInspector] public TextMeshProUGUI _dexteirtyText;
    [UnityEngine.Serialization.FormerlySerializedAs("_endurance")]
    [HideInInspector] public TextMeshProUGUI _enduranceText;
    [UnityEngine.Serialization.FormerlySerializedAs("_lvl")]
    [HideInInspector] public TextMeshProUGUI _lvlText;
    [UnityEngine.Serialization.FormerlySerializedAs("_experience")]
    [HideInInspector] public TextMeshProUGUI _experienceText;
    [UnityEngine.Serialization.FormerlySerializedAs("_coins")]
    [HideInInspector] public TextMeshProUGUI _coinsText;
    [UnityEngine.Serialization.FormerlySerializedAs("_ac")]
    [HideInInspector] public TextMeshProUGUI _acText;
    [UnityEngine.Serialization.FormerlySerializedAs("_noise")]
    [HideInInspector] public TextMeshProUGUI _noiseText;
    [UnityEngine.Serialization.FormerlySerializedAs("_blood")]
    [HideInInspector] public TextMeshProUGUI _bloodText;
    [UnityEngine.Serialization.FormerlySerializedAs("_sanity")]
    [HideInInspector] public TextMeshProUGUI _sanityText;

    //buttons
    [HideInInspector] public GameObject _strengthButton;
    [HideInInspector] public GameObject _dexterityButton;
    [HideInInspector] public GameObject _enduranceButton;
    #endregion

    #region Effects
    /** EFFECTS **/
    public bool isPoisoned;
    public int poisonDuration;

    private bool isInFire;

    public bool hasFireResistance;
    public int fireResistanceDuration;

    public bool hasPoisonResistance;
    public int poisonResistanceDuration;

    public bool hasRegeneration;
    public int regenerationDuration;

    public bool isBleeding;
    public int bleegingDuration;

    public bool isInvisible;
    public int invisibleDuration;

    public bool isBlind;
    private int viewRangeInBlindMode = 3;

    [HideInInspector] public bool fullLevelVision = false;

    private List<string> effects = new List<string>();
    public TextMeshProUGUI effectsText;

    public bool isDead;
    public int ResultStatus = 0;

    /** SPELLS **/
    [HideInInspector] public bool HasBloodRestore;
    [HideInInspector] public int BloodRestoreDuration;

    [HideInInspector] public bool HasAnoint;
    [HideInInspector] public int AnointDuration;

    public List<SpellbookSO> rememberedSpells = new List<SpellbookSO>();

    #endregion

    #region Body Parts & Text
    [Header("Items")]
    [HideInInspector] public List<Item> itemsInEqGO;
    [HideInInspector] public Text Head;
    [HideInInspector] public Item _head;

    [HideInInspector] public Text Body;
    [HideInInspector] public Item _body;

    [HideInInspector] public Text LHand;
    [HideInInspector] public Item _Lhand;

    [HideInInspector] public Text RHand;
    [HideInInspector] public Item _Rhand;

    [HideInInspector] public Text Ring;
    [HideInInspector] public Item _ring;

    [HideInInspector] public Text Legs;
    [HideInInspector] public Item _legs;

    [HideInInspector] public Text Capacity;

    #endregion

    //WAND STUFF
    [SerializeField] public bool usingWand;
    [SerializeField] public List<Vector2Int> wand_path;
    [SerializeField] public WandSO usedWand;

    //SCROLLS & SPELLBOOKS
    [HideInInspector] public bool usingSpellScroll;
    [HideInInspector] public ItemScriptableObject usedScrollOrBook;

    //DIALOGUE
    public bool dialogue;
    public RoamingNPC npcDialogue;

    [HideInInspector] public Vector2Int pos => PlayerMovement.playerMovement.position;
    public string noun => "the player";

    [TextArea]
    public string deadText;

    public void Start()
    {
        _viewRange = startingViewRange;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        statsCanvas = GameObject.Find("Stats");      

        Head = GameObject.Find("Helm").GetComponent<Text>();
        Body = GameObject.Find("Chest").GetComponent<Text>();
        LHand = GameObject.Find("LeftHand").GetComponent<Text>();
        RHand = GameObject.Find("RightHand").GetComponent<Text>();
        Ring = GameObject.Find("Ring").GetComponent<Text>();
        Legs = GameObject.Find("Legs").GetComponent<Text>();
        Capacity = GameObject.Find("Capacity").GetComponent<Text>();

        _strengthButton = GameObject.Find("strengthLevelUp");
        _dexterityButton = GameObject.Find("dexterityLevelUp");
        _enduranceButton = GameObject.Find("enduranceLevelUp");

        _strengthButton.SetActive(false);
        _dexterityButton.SetActive(false);
        _enduranceButton.SetActive(false);

        __strength = 10;
        __dexterity = 10;
        __endurance = 10;

        __sanity = 100;
        __blood = 100;

        __maxHp = __strength + (__endurance * 2) - 10;
        __currentHp = __maxHp;

        __lvl = lvl;
        __experience = experience;
        __experienceNeededToLvlUp = experienceNeededToLvlUp;
        __coins = coins;

        __experienceNeededToLvlUp = 10;
        //__experienceNeededToLvlUp = NextLevelXpReq();

        Capacity.text = "Capacity: 0" + " / " + maxWeight;

        effectsText = GameObject.Find("Effects").GetComponent<TextMeshProUGUI>();
        StartCoroutine(UpdateEffects());

        //Add starting weapon
        if(!startingWeapon) return;

        currentItems++;

        startingWeapon.OnPickup(this);

        GameObject g = Instantiate(gameManager.itemSpawner.itemPrefab);
        g.GetComponent<Item>().iso = startingWeapon;

        itemsInEqGO.Add(g.GetComponent<Item>());

        gameManager.UpdateInventoryQueue($"<color={startingWeapon.I_color}>{startingWeapon.I_name}</color>");
        UpdateCapacity();

        UpdateText(statType.sanity);
        UpdateText(statType.blood);
    }

    public void Update()
    {
        Targeting.UpdateTargeting();

        if (usingWand)
        {

            if (usedWand._spellType == WandSO.spellType.ray)
            {
                if (!usedWand.AllowTargetingMove())
                {
                    Targeting.Revert();
                }

                wand_path = LineAlg.GetPointsOnLine(
                    MapManager.playerPos.x, MapManager.playerPos.y,
                    Targeting.Position.x, Targeting.Position.y);

                foreach (var cell in wand_path)
                {
                    MapManager.map[cell.x, cell.y].decoy = $"<color=yellow>\u205C</color>";
                }
            }
            else if (usedWand._spellType == WandSO.spellType.point)
            {
                if (!usedWand.AllowTargetingMove())
                {
                    Targeting.Revert();
                }

                wand_path = LineAlg.GetPointsOnLine(
                MapManager.playerPos.x, MapManager.playerPos.y,
                Targeting.Position.x, Targeting.Position.y);

                foreach (var cell in wand_path)
                {
                    MapManager.map[cell.x, cell.y].decoy = $"<color=yellow>\u205C</color>";
                }
            } //todo
            else
            {
                if (usedWand.IsValidTarget())
                {
                    usingWand = false;
                    Targeting.IsTargeting = false;
                    usedWand.UseWandSpell();
                    usedWand = null;
                    PlayerMovement.playerMovement.canMove = true;
                }
                else
                {
                    // you cannot target that guys
                    Debug.Log("not the right target");
                }
            }

            if (Controls.GetKeyDown(Controls.Inputs.Use))
            {
                if (usedWand.IsValidTarget())
                {
                    usingWand = false;
                    Targeting.IsTargeting = false;
                    usedWand.UseWandSpell();
                    usedWand = null;
                    gameManager.ApplyChangesInInventory(null);
                    PlayerMovement.playerMovement.canMove = true;
                }
                else
                {
                    // you cannot target that guys
                    Debug.Log("not the right target");
                }
            }
            DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
        }

        if (usingSpellScroll)
        {
            if (PlayerMovement.playerMovement.canMove)
            {
                Debug.Log(MapManager.playerPos + " " + Targeting.Position);
                PlayerMovement.playerMovement.canMove = false;
                gameManager.UpdateMessages("Choose target of your spell. <color=pink>(Numpad 8 4 6 2, Enter/Space to confirm, Escape to cancel)</color>");
            }
            if (usedScrollOrBook is IRestrictTargeting check)
            {
                if (check.IsValidTarget())
                {
                    MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = $"<color=yellow>\u205C</color>";
                }
                else
                {
                    MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = $"<color={ColorUtility.ToHtmlStringRGB(Color.gray)}>\u205C</color>";
                }
            }
            else
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].decoy = $"<color=yellow>\u205C</color>";
            }

            
            DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);

            if (Controls.GetKeyDown(Controls.Inputs.Use))
            {
                if (usedScrollOrBook is IRestrictTargeting target)
                {
                    if (target.IsValidTarget())
                    {
                        PlayerMovement.playerMovement.canMove = true;
                        Targeting.IsTargeting = false;
                        usingSpellScroll = false;
                        if (usedScrollOrBook is ScrollSO scroll)
                        {
                            scroll.UseSpell(this);
                        }
                        else if (usedScrollOrBook is SpellbookSO book)
                        {
                            book.UseSpell(this);
                        }
                        usedScrollOrBook = null;
                        gameManager.ApplyChangesInInventory(null);
                    }
                }
                DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
            }
            if (Controls.GetKeyDown(Controls.Inputs.CancelButton))
            {
                PlayerMovement.playerMovement.canMove = true;
                Targeting.IsTargeting = false;
                usingSpellScroll = false;
                usedScrollOrBook = null;
                DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
            }
        }
    
        if(dialogue)
        {
            if(PlayerMovement.playerMovement.canMove)
            {
                GameManager.manager.player.StopAllCoroutines();

                PlayerMovement.playerMovement.canMove = false;

                gameManager.UpdateMessages("<b><color=red>???</color></b>: <i>Ahh, you're no jailer are you? No, no, you're from far away...</i>");
            
                gameManager.UpdateMessages("<color=red><b>???</b></color>: <i>These bastards locked me here. I'm not asking for much, just cut the knots on my wrists, I'll be fine somehow, but in return I can give you something that I took from the priests.</i>");

                gameManager.UpdateMessages("<color=red><b>???</b></color>: <i>We are alike, we should support each other.</i>");

                gameManager.UpdateMessages("\n <i>[1. Help] [2. Refuse] [3. Cancel]</i>");
            }

            if(Controls.GetKeyDown(Controls.Inputs.InventoryChoice1))
            {               
                gameManager.UpdateMessages("\n <b><color=red>Grim</color></b>: <i>Very well. I am Grim, I am grateful for your help. As I promised, in return for your help...</i>");

                GameObject rewardObject = Instantiate(gameManager.itemSpawner.itemPrefab);

                rewardObject.GetComponent<Item>().iso = npcDialogue.enemySO.rewardItem;

                rewardObject.transform.SetParent(MapManager.CurrentFloor.GO.transform);

                Pickup(rewardObject, rewardObject.GetComponent<Item>().iso, new Vector2Int(100,100));

                dialogue = false;
                PlayerMovement.playerMovement.canMove = true;
                npcDialogue.enemySO.finishedDialogue = true;
                npcDialogue = null;              
            }
            if(Controls.GetKeyDown(Controls.Inputs.InventoryChoice2))
            {
                gameManager.UpdateMessages("\n <b><color=red>???</color></b>: <i>Yes, well, why should you? But if you change your mind, give me a shout, eh?</i>");
                dialogue = false;
                PlayerMovement.playerMovement.canMove = true;
                npcDialogue = null;
            }
            if(Controls.GetKeyDown(Controls.Inputs.InventoryChoice3))
            {
                PlayerMovement.playerMovement.canMove = true;
                gameManager.UpdateMessages("\n <b><color=red>???</color></b>: <i>I will wait...</i>");
                dialogue = false;
                npcDialogue = null;
            }     
        }
    }

    public int RecalculateViewRange()
    {
        int _vr = 0;

        _vr = startingViewRange;
        if (MapManager.CurrentFloor.GO.GetComponent<FloorInfo>().viewRange != 666)
        {
            _vr = MapManager.CurrentFloor.GO.GetComponent<FloorInfo>().viewRange;
        }

        if(_Lhand != null)
        {
            if(_Lhand.iso is WeaponsSO w1)
            {
                if(_vr < startingViewRange)
                {
                    _vr += w1.lightFactor;
                }
            }
        }

        if (_Rhand != null)
        {
            if (_Rhand.iso is WeaponsSO w2)
            {
                if (_vr < startingViewRange)
                {
                    _vr += w2.lightFactor;
                }
            }
        }

        if (_vr > startingViewRange) _vr = startingViewRange;

        DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);

        return _vr;
    }

    private bool CellIsAvailable(int x, int y)
    {
        if (MapManager.map[x, y].type != "Wall" 
            && MapManager.map[x, y].type != "Door" 
            && y > 0 
            && y < DungeonGenerator.dungeonGenerator.mapHeight 
            && x > 0 
            && x < DungeonGenerator.dungeonGenerator.mapWidth 
            && Vector2Int.Distance(MapManager.playerPos, new Vector2Int(x, y)) < usedWand.maxDistance)
        {
            return true;
        }
        else return false;
    }

    public void UpdateArmorClass()
    {
        armorClass = 0;
        armorClass += __dexterity;

        if(_head != null && _head.iso is ArmorSO armor)
        {
            armorClass += armor.armorClass;
        }
        if (_body != null && _body.iso is ArmorSO armor1)
        {
            armorClass += armor1.armorClass;
        }
        if (_legs != null && _legs.iso is ArmorSO armor2)
        {
            armorClass += armor2.armorClass;
        }
    }

    public void Pickup(GameObject itemObject, ItemScriptableObject iso, Vector2Int position)
    {
        currentItems++;
        iso.OnPickup(this);
        itemsInEqGO.Add(itemObject.GetComponent<Item>());

        foreach (var skill in iso.SkillsToLearn)
        {
            gameManager.LearnSkill(skill);
        }
        
        try {MapManager.map[position.x, position.y].item = null;}
        catch{}

        if (itemObject.GetComponent<Item>().identified == true)
        {
            gameManager.UpdateMessages($"You picked up the <color={iso.I_color}>{iso.I_name}</color>.");
            gameManager.UpdateInventoryQueue($"<color={iso.I_color}>{iso.I_name}</color>");
        }
        else
        {
            gameManager.UpdateMessages($"You picked up the <color=purple>{iso.I_unInName}</color>.");
            gameManager.UpdateInventoryQueue($"<color=purple>{iso.I_unInName}</color>");
        }
        GameManager.manager.ApplyChangesInInventory(null);
        UpdateCapacity();
    }

    public void UpdateCapacity()
    {
        int capacity = 0;

        foreach (var item in itemsInEqGO)
        {
            capacity += item.iso.I_weight;
        }

        Capacity.text = "Capacity: " + capacity + " / " + maxWeight;
    }

    public void UpdateText(statType stat)
    {
        switch (stat)
        {
            case statType.hp:
                _healthText.text = "Hp: " + $"<color=green>{__currentHp} / {__maxHp}</color>";
                break;
            case statType.dexterity:
                _dexteirtyText.text = "Dex: " + $"<color=green>{__dexterity}</color>";
                break;
            case statType.coins:
                _coinsText.text = "$: " + $"<color=green>{__coins}</color>";
                break;
            /*case statType.intelligence:
                _intelligenceText.text = "Int: " + $"<color=green>{__intelligence}</color>";
                break;*/
            case statType.strength:
                _strengthText.text = "Str: " + $"<color=green>{__strength}</color>";
                break;
            case statType.endurance:
                _enduranceText.text = "End: " + $"<color=green>{__endurance}</color>";
                break;
            case statType.experience:
                _experienceText.text = "Exp: " + $"<color=green>{__experience} / {__experienceNeededToLvlUp}</color>";
                break;
            case statType.lvl:
                _lvlText.text = "Lvl: " + $"<color=green>{lvl}</color>";
                break;
            case statType.ac:
                _acText.text = "AC: " + $"<color=green>{armorClass}</color>";
                break;
            case statType.noise:
                _noiseText.text = "Noise: " + $"<color=green>{__noise}</color>";
                break;
            case statType.blood:
                _bloodText.text = "Blood: " + $"<color=red>{__blood}%</color>";
                break;
            case statType.sanity:
                if(__sanity <= 20) _sanityText.text = "Sanit: " + $"<color=red>{__sanity}% <size=15>(Insane)</size></color>";
                else if(__sanity <= 50) _sanityText.text = "Sanit: " + $"<color=red>{__sanity}% <size=15>(Paranoid)</size></color>";
                else if(__sanity <= 70) _sanityText.text = "Sanit: " + $"<color=red>{__sanity}% <size=15>(Nervous)</size></color>";
                else _sanityText.text = "Sanit: " + $"<color=red>{__sanity}% <size=15>(Excellent)</size></color>";
                break;
        }
    } //update stats in ui

    /**
     *  Returns the Xp cap required for the next level increment
     */
    private int NextLevelXpReq()
    {
        int req = Mathf.FloorToInt(__experienceNeededToLvlUp * 1.35f); // Placeholder formula
        return req;
    }

    /**
     *  Distributes Xp to level up character
     *  Updates __experience by "trading" points for next level
     *  Updates __lvl to current Xp Level
     *  Updates __experienceNeededToLvlUp to meet current XP and Level
     */
    public void UpdateLevel(int xp)
    {
        __experience += xp;
        RunManager.CurrentRun.Set(RunManager.Names.XPEarned, RunManager.CurrentRun.Get<int>(RunManager.Names.XPEarned) + xp);

        if (__experienceNeededToLvlUp <= __experience) //level up
        {          
            __lvl++;
            RunManager.CurrentRun.Set(RunManager.Names.CharLevel, __lvl);
            skillpoints++;          
            gameManager.UpdateMessages
                (
                    "<color=#ff0000>Y</color>" +
                    "<color=#ff2a00>o</color>" +
                    "<color=#ff5500>u</color>" +
                    "<color=#ff7f00> l</color>" +
                    "<color=#ffaa00>e</color>" +
                    "<color=#ffd400>v</color>" +
                    "<color=#ffff00>e</color>" +
                    "<color=#80ff00>l</color>" +
                    "<color=#00ff00>e</color>" +
                    "<color=#00ff66>d</color>" +
                    "<color=#00ffaa> u</color>" +
                    "<color=#0055ff>p</color>" +
                    "<color=#0000ff>!</color>"
                );

            __currentHp += Mathf.RoundToInt(__maxHp * 0.1f);

            _strengthButton.SetActive(true);
            //_intelligenceButton.SetActive(true);
            _dexterityButton.SetActive(true);
            _enduranceButton.SetActive(true);

            __experience -= __experienceNeededToLvlUp;
            __experienceNeededToLvlUp = NextLevelXpReq();
        }
    }

    public void TakeDamage(int damage,ItemScriptableObject.damageType type)
    {
        __sanity--;
        __currentHp -= damage;
        LossBlood(Mathf.RoundToInt(damage / 2));
        gameManager.gameObject.GetComponent<MousePointer>().StopAllCoroutines();

        if(!isDead && __currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        PlayerMovement.playerMovement.StopAllCoroutines();
        gameManager.UpdateMessages("You are <color=#990000>dead</color>.");
        if (_ring != null && _ring.iso.I_name == "Ring of Life Saving")
        {
            EffectCoroutine = LifeSaveEffect();
            StartCoroutine(EffectCoroutine);
            gameManager.UpdateMessages("<color=yellow>But wait... Your medallion begins to glow! You feel much better! The medallion crumbles to dust!</color>");
            __currentHp = __maxHp;
            __blood = 100;
            gameManager.ApplyChangesInInventory(_ring.iso);
            gameManager.UpdateEquipment(Ring, "<color=#ffffff>Ring: </color>");
            _ring = null;
        }
        else
        {
            gameManager.deadScreenAnimation.SetActive(true);
            gameManager.deadScreenAnimation.GetComponentInChildren<Animation>().Play();
            PlayerMovement.playerMovement.canMove = false;
            isDead = true;
            ResultStatus = 0;
        }
    }

    IEnumerator EffectCoroutine;

    public IEnumerator LifeSaveEffect()
    {
        Vector2Int playerPos = MapManager.playerPos;

        int x = 0;

        for (int i = 1; i < 60; i++)
        {
            x = playerPos.x;
            for (int i1 = playerPos.y + i; i1 >= playerPos.y; i1--)
            {              
                try { MapManager.map[x, i1].decoy = "<color=yellow>*</color>"; }
                catch { }

                try { MapManager.map[x, playerPos.y + (playerPos.y - i1)].decoy = "<color=yellow>*</color>"; }
                catch { }

                try {MapManager.map[x, i1].decoy = "<color=yellow>*</color>"; }
                catch { }

                try { MapManager.map[x, playerPos.y + (playerPos.y - i1)].decoy = "<color=yellow>*</color>"; }
                catch { }
                x++;
            }

            x = playerPos.x;

            for (int i1 = playerPos.y + i; i1 >= playerPos.y; i1--)
            {
                try { MapManager.map[x, i1].decoy = "<color=yellow>*</color>"; }
                catch { }

                try { MapManager.map[x, playerPos.y + (playerPos.y - i1)].decoy = "<color=yellow>*</color>"; }
                catch { }

                try { MapManager.map[x, i1].decoy = "<color=yellow>*</color>"; }
                catch { }

                try { MapManager.map[x, playerPos.y + (playerPos.y - i1)].decoy = "<color=yellow>*</color>"; }
                catch { }
                x--;
            }
            yield return new WaitForSeconds(0.001f);
            DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
        }

        yield return null;
    }

    public void LossBlood(int amount)
    {
        __blood -= amount;
    }

    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void IncreasePoisonDuration(int amount) { poisonDuration += amount; }

    public void Poison()
    {
        if (!isPoisoned)
        {
            if (hasPoisonResistance) return;

            gameManager.UpdateMessages("You are <color=green>poisoned</color>!");
            isPoisoned = true;

            gameManager.playerTurnTasks += Poison;

            effects.Add("<color=green>Poisoned</color>");
        }
       
        if(poisonDuration > 0)
        {
            TakeDamage(1, ItemScriptableObject.damageType.normal);
            poisonDuration--;

            if(UnityEngine.Random.Range(1, 100) == 1)
            {
                __currentHp = 0;
            }
        }
        else
        {
            CurePoison();
        }
    }

    public void CurePoison()
    {
        isPoisoned = false;
        gameManager.playerTurnTasks -= Poison;

        int index = System.Array.IndexOf(effects.ToArray(), "<color=green>Poisoned</color>");
        effects.RemoveAt(index);
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void IncreaseFireResistanceDuration(int amount) { fireResistanceDuration += amount; }

    public void FireResistance()
    {
        if (!hasFireResistance)
        {
            hasFireResistance = true;

            gameManager.playerTurnTasks += FireResistance;

            gameManager.UpdateMessages("Now you are now <color=red>fire</color> resistant.");

            effects.Add("<color=red>Fire Res.</color>");
        }

        if (fireResistanceDuration > 0)
        {
            fireResistanceDuration--;
        }
        else
        {
            RemoveFireResistance();
        }
    }

    public void RemoveFireResistance()
    {
        hasFireResistance = false;

        gameManager.playerTurnTasks -= FireResistance;

        gameManager.UpdateMessages("You are no longer <color=red>fire</color> resistant.");

        int index = System.Array.IndexOf(effects.ToArray(), "<color=red>Fire Res.</color>");
        effects.RemoveAt(index);
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void IncreasePoisonResistanceDuration(int amount) { poisonResistanceDuration += amount; }

    public void PoisonResistance()
    {
        if (!hasPoisonResistance)
        {
            hasPoisonResistance = true;

            gameManager.playerTurnTasks += PoisonResistance;

            gameManager.UpdateMessages("You are now <color=green>poison</color> resistant.");

            effects.Add("<color=green>Poison Res.</color>");
        }

        if (poisonResistanceDuration > 0)
        {
            if (isPoisoned)
            {
                CurePoison();
            }
        }
        else
        {
            RemovePoisonResistance();
        }
    }

    public void RemovePoisonResistance()
    {
        hasPoisonResistance = false;

        gameManager.playerTurnTasks -= PoisonResistance;

        gameManager.UpdateMessages("You are no longer <color=gree>poison</color> resistant.");

        int index = System.Array.IndexOf(effects.ToArray(), "<color=green>Poison Res.</color>");
        effects.RemoveAt(index);
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void IncreaseRegenerationDuration(int amount) { regenerationDuration += amount; }

    public void Regeneration()
    {
        if (!hasRegeneration)
        {
            hasRegeneration = true;

            gameManager.playerTurnTasks += Regeneration;

            gameManager.UpdateMessages("You have gained <color=#e69138>regeneration.</color>");

            effects.Add("<color=#e69138>Regen.</color>");
        }

        if (regenerationDuration > 0)
        {
            regenerationDuration--;
            if (__currentHp < __maxHp)
            {
                __currentHp++;
            }
        }
        else
        {
            RemoveRegeneration();
        }
    }

    public void RemoveRegeneration()
    {
        hasRegeneration = false;

        gameManager.playerTurnTasks -= Regeneration;

        int index = System.Array.IndexOf(effects.ToArray(), "<color=#e69138>Regen.</color>");
        effects.RemoveAt(index);
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void FullRestore()
    {
        __currentHp = __maxHp;

        gameManager.UpdateMessages("Your health has been restored.");
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void Invisible()
    {
        gameManager.UpdateMessages("You are now <color=lightblue>invisible</color>.");
        isInvisible = true;
    }

    public void RemoveInvisibility()
    {
        isInvisible = false;

        gameManager.UpdateMessages("You are no longer <color=lightblue>invisible</color>.");
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void IncreaseBleedingDuration(int amount) { bleegingDuration += amount; }

    public void Bleeding()
    {
        if (!isBleeding)
        {
            isBleeding = true;

            gameManager.playerTurnTasks += Bleeding;
            //bleegingDuration = 20 - (__intelligence / 7);

            gameManager.UpdateMessages("You are now <color=red>bleeding.</color>");

            effects.Add("<color=red>Bleeding</color>");
        }

        if (bleegingDuration > 0)
        {
            bleegingDuration--;
            TakeDamage(1, ItemScriptableObject.damageType.normal);
            __blood -= 1;
        }
        else
        {
            CureBleeding();
        }
    }

    public void CureBleeding()
    {
        isBleeding = false;
        gameManager.playerTurnTasks -= Bleeding;

        int index = System.Array.IndexOf(effects.ToArray(), "<color=red>Bleeding</color>");
        effects.RemoveAt(index);

        gameManager.UpdateMessages("You are no longer <color=re>bleeding.</color>");
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================
    public void Blindness()
    {
        if (!isBlind) GameManager.manager.UpdateMessages("You are <color=red>blind</color> now!");
        isBlind = true;
        foreach (var tile in MapManager.map)
        {
            tile.isExplored = false;
            tile.isVisible = false;
        }
        viewRange = viewRangeInBlindMode;

        gameManager.fv.Compute(MapManager.playerPos, viewRange);
        DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
    }

    public void CureBlindness()
    {
        GameManager.manager.UpdateMessages("You are no longer <color=red>blind</color>!");
        isBlind = false;
        viewRange = startingViewRange;
    }
    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================

    public void Stun()
    {
        PlayerMovement.playerMovement.canMove = false;
        PlayerMovement.playerMovement.isStunned = true;
    }

    //=========================================
    //++++++++++++++++++++++++++++++++++++++++
    //=========================================

    public void FullVision()
    {
        if (!fullLevelVision) fullLevelVision = true;

        Debug.Log("Drank");

        foreach (var tile in MapManager.map)
        {
            if (tile.type == "Wall" || tile.type == "Floor")
            {
                tile.isVisible = true;
                tile.isExplored = true;
            }

        }

        DungeonGenerator.dungeonGenerator.DrawMap(MapManager.map);
    }

    public void BloodRestore()
    {
        if (!HasBloodRestore)
        {
            HasBloodRestore = true;

            gameManager.playerTurnTasks += BloodRestore;
            BloodRestoreDuration = 10 + __intelligence / 10;
        }

        if (BloodRestoreDuration > 0)
        {
            BloodRestoreDuration--;
            __currentHp += 2;
            __blood += 2;
        }
        else
        {
            HasBloodRestore = false;
            gameManager.playerTurnTasks -= BloodRestore;

            return;
        }

        if (__currentHp >= __maxHp)
        {
            HasBloodRestore = false;
            gameManager.playerTurnTasks -= BloodRestore;
        }
    }

    public void Anoint()
    {
        if (!HasAnoint)
        {
            HasAnoint = true;

            gameManager.playerTurnTasks += Anoint;
            AnointDuration = 20 + (__intelligence / 4);

            effects.Add("<color=green>Anoint</color>");
        }

        if (AnointDuration > 0)
        {
            AnointDuration--;
            if (__currentHp + 1 <= __maxHp)
            {
                __currentHp++;
            }
        }
        else
        {
            HasAnoint = false;
            gameManager.playerTurnTasks -= Anoint;

            int index = System.Array.IndexOf(effects.ToArray(), "<color=green>Anoint</color>");
            effects.RemoveAt(index);
        }
    }


    public void MeltItem()
    {
        Item randomMeltedItem = null;

        try
        {
            randomMeltedItem = itemsInEqGO[UnityEngine.Random.Range(0, itemsInEqGO.Count)];
        }
        catch { }

        if (randomMeltedItem != null)
        {
            int index = 0;
            gameManager.UpdateMessages($"Your <color={randomMeltedItem.iso.I_color}>{randomMeltedItem.iso.I_name}</color> melts!");
            index = itemsInEqGO.IndexOf(randomMeltedItem);
            if (itemsInEqGO[index].isEquipped)
            {
                Item _selectedItem = itemsInEqGO[index];
                switch (_selectedItem.iso.I_whereToPutIt)
                {
                    case ItemScriptableObject.whereToPutIt.head:
                        _head = null;
                        GameManager.manager.UpdateEquipment(Head, "<color=#ffffff>Helm: </color>");

                        if (_selectedItem.iso is ArmorSO armor)
                        {
                            armor.Use(this, _selectedItem);
                        }
                        _selectedItem.isEquipped = false;
                        break;
                    case ItemScriptableObject.whereToPutIt.body:
                        _body = null;
                        GameManager.manager.UpdateEquipment(Body, "<color=#ffffff>Chest: </color>");

                        if (_selectedItem.iso is ArmorSO armor1)
                        {
                            armor1.Use(this, _selectedItem);
                        }
                        _selectedItem.isEquipped = false;
                        break;
                    case ItemScriptableObject.whereToPutIt.hand:
                        if (_selectedItem._handSwitch == Item.hand.left)
                        {
                            _Lhand = null;
                            LHand.text = "Left Hand:";
                            if (_selectedItem.isEquipped) _selectedItem.isEquipped = false;
                            /*if (_selectedItem.iso is WeaponsSO weapon)
                            {
                                _selectedItem.UnequipWithGems();
                            }*/
                        }
                        else if (_selectedItem._handSwitch == Item.hand.right)
                        {
                            _Rhand = null;
                            RHand.text = "Right Hand:";
                            if (_selectedItem.isEquipped) _selectedItem.isEquipped = false;
                            /*if (_selectedItem.iso is WeaponsSO weapon)
                            {
                                _selectedItem.UnequipWithGems();
                            }*/
                        }
                        break;
                    case ItemScriptableObject.whereToPutIt.ring:

                        _ring = null;
                        GameManager.manager.UpdateEquipment(Ring, "<color=#ffffff>Ring: </color>");
                        _selectedItem.isEquipped = false;
                        break;
                    case ItemScriptableObject.whereToPutIt.legs:
                        _legs = null;
                        GameManager.manager.UpdateEquipment(Legs, "<color=#ffffff>Legs: </color>");

                        if (_selectedItem.iso is ArmorSO armor2)
                        {
                            armor2.Use(this, _selectedItem);
                        }
                        _selectedItem.isEquipped = false;
                        break;
                }
                GameManager.manager.UpdateInventoryText();
                _selectedItem.iso.onUnequip(this);
                _selectedItem.OnUnequip(this);
            }
            gameManager.ApplyChangesInInventory(randomMeltedItem.iso);
        }
    }

    private IEnumerator UpdateEffects()
    {
        while (true)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                effectsText.text = effects[i];
                yield return new WaitForSeconds(1);              
            }
            yield return new WaitForSeconds(1);
            effectsText.text = "";
        }
    }

    public void AddItemFromChest(ItemScriptableObject iso)
    {
        GameObject g = Instantiate(gameManager.itemSpawner.itemPrefab);
        g.GetComponent<Item>().iso = iso;
        g.GetComponent<Item>().identified = iso.normalIdentifState;

        itemsInEqGO.Add(g.GetComponent<Item>());
    }
}
