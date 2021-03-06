﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anvil : Structure
{
    public override void Use()
    {
        if (!GameManager.manager.anvilMenuOpened)
        {
            GameManager.manager.player.StopAllCoroutines();
            GameManager.manager.anvilMenuOpened = true;
            GameManager.manager.OpenEQ();
            GameManager.manager.anvil = this;
        }
    }

    public void UseAnvil()
    {
        if(GameManager.manager.itemToAnvil.isEquipped)
        {
            GameManager.manager.UpdateMessages("You can't upgrade equipped item.");
            return;
        }
        if (GameManager.manager.itemToAnvil.upgradeLevel == 0)
        {
            if (GameManager.manager.playerStats.__blood >= 10)
            {
                if (Random.Range(0, 101) > 5)
                {
                    GameManager.manager.playerStats.LossBlood(10);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 1)
        {
            if (GameManager.manager.playerStats.__blood >= 15)
            {
                if (Random.Range(0, 101) > 10)
                {
                    GameManager.manager.playerStats.LossBlood(15);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 2)
        {
            if (GameManager.manager.playerStats.__blood >= 20)
            {
                if (Random.Range(0, 101) > 15)
                {
                    GameManager.manager.playerStats.LossBlood(20);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 3)
        {
            if (GameManager.manager.playerStats.__blood >= 25)
            {
                if (Random.Range(0, 101) > 20)
                {
                    GameManager.manager.playerStats.LossBlood(25);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 4)
        {
            if (GameManager.manager.playerStats.__blood >= 30)
            {
                if (Random.Range(0, 101) > 30)
                {
                    GameManager.manager.playerStats.LossBlood(30);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 5)
        {
            if (GameManager.manager.playerStats.__blood >= 35)
            {
                if (Random.Range(0, 101) > 40)
                {
                    GameManager.manager.playerStats.LossBlood(35);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 6)
        {
            if (GameManager.manager.playerStats.__blood >= 45)
            {
                if (Random.Range(0, 101) > 50)
                {
                    GameManager.manager.playerStats.LossBlood(45);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 7)
        {
            if (GameManager.manager.playerStats.__blood >= 55)
            {
                if (Random.Range(0, 101) > 65)
                {
                    GameManager.manager.playerStats.LossBlood(55);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 8)
        {
            if (GameManager.manager.playerStats.__blood >= 65)
            {
                if (Random.Range(0, 101) > 80)
                {
                    GameManager.manager.playerStats.LossBlood(65);
                    GameManager.manager.itemToAnvil.upgradeLevel++;
                    GameManager.manager.UpdateMessages("You succesfully upgraded item!");
                    Upgrade();
                }
                else
                {
                    GameManager.manager.UpdateMessages($"<color={GameManager.manager.itemToAnvil.iso.I_color}>{GameManager.manager.itemToAnvil.iso.I_name}</color> broke. ;c");
                    GameManager.manager.ApplyChangesInInventory(GameManager.manager.itemToAnvil.iso);
                }
            }
            else
            {
                GameManager.manager.UpdateMessages("You don't have enough <color=red>blood</color> to sacrifice.");
            }
        }
        else if (GameManager.manager.itemToAnvil.upgradeLevel == 9)
        {

        }

        GameManager.manager.itemToAnvil = null;
    }

    private void Upgrade()
    {      
        int i = Random.Range(1, 11);

        if(i <= 5)
        {
            IncreaseRandomStat();
        }
        else if(i < 9)
        {
            IncreaseRandomStat();
            IncreaseRandomStat();
        }
        else
        {
            IncreaseRandomStat();
            IncreaseRandomStat();
            IncreaseRandomStat();
        }

        GameManager.manager.UpdateItemStats(GameManager.manager.itemToAnvil);
    }

    private void IncreaseRandomStat()
    {
        int i = Random.Range(1, 5);

        if(i == 1)
        {
            GameManager.manager.itemToAnvil.Anvil_bonusToDexterity++;
        }
        else if(i == 2)
        {
            GameManager.manager.itemToAnvil.Anvil_bonusToEndurance++;
        }
        else if (i == 3)
        {
            GameManager.manager.itemToAnvil.Anvil_bonusToIntelligence++;
        }
        else if (i == 4)
        {
            GameManager.manager.itemToAnvil.Anvil_bonusToStrength++;
        }
    }

    public override void WalkIntoTrigger()
    {
    }
}
