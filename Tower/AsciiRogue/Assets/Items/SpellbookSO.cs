﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Spellbook")]
public class SpellbookSO : ItemScriptableObject, IRestrictTargeting
{
    public enum school
    {
        chaos,
        Blood,
        Plague
    }
    public school _school;

    public enum type
    {
        self,
        target
    }
    public type _type;

    public enum spell
    {
        DrainLife,
        BloodForBlood,
        BloodRestore,
        BloodPact,
        Cauterize,
        RemovePoison,
        CausticDart,
        Anoint,
        Purify,
        Invisiblity,
        Root,
        Poisonbolt,
        FingerOfDeath
    }
    public spell _spell;

    public int spellLevel;
    public int spellDuration;
    public int spellBloodCost;

    [Header("Learning Settings")]
    public int duration = 5; //how many times u can try to read it
    public int learnDuration; //how many turns does it take to learn it

    public override void Use(MonoBehaviour foo, Item itemObject)
    {
        if(GameManager.manager.playerStats.isBlind)
        {
            GameManager.manager.UpdateMessages("You can't read because you are <color=green>blind</color>!");
            return;
        }
        else
        {
            GameManager.manager.UpdateMessages("You start to page through the ornate tome...");
            itemObject.durationLeft--;
            CastSpell(foo);
        }
    }

    public override void OnPickup(MonoBehaviour foo)
    {
        
    }

    public void CastSpell(MonoBehaviour foo)
    {
        if(GameManager.manager.playerStats.__blood < spellBloodCost)
        {
            GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to cast this spell.");
            return;
        }
        if (_type == type.self)
        {
            UseSpell(foo);
            GameManager.manager.CloseEQ();
        }
        else
        {
            GameManager.manager.CloseEQ();

            if (foo is PlayerStats player)
            {
                player.usedScrollOrBook = this;
                player.usingSpellScroll = true;
                Targeting.IsTargeting = true;
            }

            
            GameManager.manager.FinishPlayersTurn();
        }

        GameManager.manager.playerStats.LossBlood(spellBloodCost);
    }

    public void UseSpell(MonoBehaviour foo)
    {
        switch (_spell)
        {
            case spell.BloodForBlood:
                BloodForBlood(foo);
                break;
            case spell.BloodRestore:
                BloodRestore(foo); 
                break;
            case spell.BloodPact:
                if(foo is PlayerStats player)
                {
                    if(!player.isBleeding)
                    {
                        BloodPact(foo);
                    }
                    else
                    {
                        GameManager.manager.UpdateMessages("You can't read this book while you are <color=red>bleeding.</color>");
                    }
                }               
                break;
            case spell.Cauterize:
                Cauterize(foo);  
                break;
            case spell.RemovePoison:
                RemovePoison(foo);  
                break;
            case spell.CausticDart:
                CausticDart(foo);
                break;
            case spell.Anoint:
                Anoint(foo);
                break;
            case spell.Purify:
                Purify(foo);
                break;
            case spell.Invisiblity:
                Invisiblity(foo);
                break;
            case spell.Root:
                Root(foo);
                break;
            case spell.Poisonbolt:
                Poisonbolt(foo);
                break;
            case spell.FingerOfDeath:
                FingerOfDeath(foo);
                break;
        }

        if(foo is PlayerStats _player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
            }
        }
    }

    public void FingerOfDeath(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().TakeDamage(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().maxHp, ItemScriptableObject.damageType.magic);
                GameManager.manager.UpdateMessages($"You read the <color=red>Book of Finger of Death</color>.");
            }
            else if(MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>Finger of Death</color>.");
                GameManager.manager.UpdateMessages($"You feel painful energy bursting through body...");
                player.TakeDamage(player.__maxHp, damageType.normal);
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>Finger of Death</color> but nothing happens.");
            }
        }
    }

    public void DrainLife(MonoBehaviour foo, Item item)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                int damage = Random.Range(1, 10) + Random.Range(1, 10) + player.__intelligence / 10;
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().TakeDamage(damage, ItemScriptableObject.damageType.magic);
                player.__currentHp += damage;
                item.spellbookCooldown = 20;
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You drained <color=red>{damage}</color> health.");
            }
            else if(MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                item.spellbookCooldown = 20;
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You feel pain but but it goes away after a while.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void Poisonbolt(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().dotDuration = spellDuration;
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().DamageOverTurn(); 
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. Monster is now poisoned.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                if(player.isPoisoned)
                {
                    player.poisonDuration += spellDuration;
                    GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>.");
                }
                else
                {
                    player.IncreasePoisonDuration(spellDuration);
                    player.Poison();
                    GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You are now <color=green>poisoned</color>.");
                }
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void Root(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().rooted = true;
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().rootDuration = 10 + Mathf.FloorToInt(player.__intelligence / 10);  
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. Monster can't move!");
            }
        }
    }

    public void Invisiblity(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().MakeInvisible();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                player.invisibleDuration = 20 + player.__intelligence;
                player.Invisible();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void BloodForBlood(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().TakeDamage(Mathf.FloorToInt((20 + player.__intelligence) / 5), damageType.magic);
                player.TakeDamage(5, damageType.normal);
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You dealt {(20 + player.__intelligence) / 5} damage to the monster.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                player.TakeDamage(5, damageType.normal);
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You feel piercing pain.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void BloodRestore(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                player.BloodRestore();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void BloodPact(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                player.__currentHp += 27; //25 + 2 because 2 is dealed from bleeding
                if (player.__currentHp > player.__maxHp)
                {
                    player.__currentHp -= (player.__currentHp - player.__maxHp);

                }

                player.IncreaseBleedingDuration(20 - (player.__intelligence / 7));
                player.Bleeding();

                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You restore 25 health but you are <color=red>bleeding<color> now!");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void Cauterize(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                if (player.isBleeding)
                {
                    player.CureBleeding();
                }

                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You are no longer bleeding!");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void RemovePoison(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                if (player.isPoisoned)
                {
                    player.CurePoison();
                    GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You are no longer poisoned!");
                }
                else GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void CausticDart(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if(MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().TakeDamage(10 + Mathf.FloorToInt(player.__intelligence / 7), damageType.magic);
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You dealt {10 + Mathf.FloorToInt(player.__intelligence / 7)} damage to the monster.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                player.TakeDamage(10 + Mathf.FloorToInt(player.__intelligence / 7), damageType.normal);
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. You feel piercing pain.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>. but nothing happens.");
            }
        }
    }

    public void Anoint(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                player.Anoint();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>.");
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public void Purify(MonoBehaviour foo)
    {
        if(foo is PlayerStats player)
        {
            if (MapManager.map[Targeting.Position.x, Targeting.Position.y].hasPlayer)
            {
                if (player.isPoisoned)
                {
                    player.CurePoison();
                    GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color>.");
                }
                else
                {
                    GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
                }
            }
            else if (MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy != null)
            {
                MapManager.map[Targeting.Position.x, Targeting.Position.y].enemy.GetComponent<RoamingNPC>().WakeUp();
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
            else
            {
                GameManager.manager.UpdateMessages($"You read the book called <color=red>{I_name}</color> but nothing happens.");
            }
        }
    }

    public override void onEquip(MonoBehaviour foo)
    {
    }

    public override void onUnequip(MonoBehaviour foo)
    {
    }
    public bool IsValidTarget()
    {
        // TODO: code is required here
        return true;
    }

    public bool AllowTargetingMove()
    {
        // TODO: code is required here
        return true;
    }
}
