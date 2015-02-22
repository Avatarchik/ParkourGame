using UnityEngine;
using System.Collections;
using System;

namespace Attributes
{
    public static class AttributeMath
    {
        public static void IncreaseAgility(ref float walkSpeed, ref float runSpeed)
        {
            float speedIncrease = (runSpeed - walkSpeed) * 0.075f;
            walkSpeed += speedIncrease;
            runSpeed += speedIncrease;
        }

        public static void IncreaseStamina(ref float stamina, int maxStamina)
        {   
            float staminaIncrease = stamina * 0.215f;
            stamina += staminaIncrease;
            stamina = Mathf.Clamp(stamina, 0.0f, (float)maxStamina);
        }

        public static void IncreaseMaxHealth(ref int healthPoints, int currentHealth, int maxHealth)
        {
            healthPoints += 20;
            healthPoints = Mathf.Clamp(healthPoints, currentHealth, maxHealth);
        }

        public static void UseSkillPoint(ref int skillPoints)
        {
            if(skillPoints >= 1)
                skillPoints--;
            if (skillPoints == 0)
                throw new Exception("There is no more points left to use!");
        }

        public static void IncreaseDamageResist(ref float damageResist)
        {
            if(damageResist == 0.0f)
                damageResist += 5.0f;
            else
            {
                float increaseResist = damageResist * 0.4125f;
                damageResist += increaseResist;
            }
        }

        public static void IncreaseXP(ref int xp, int xpToBeAdded)
        {
            xp += xpToBeAdded;
        }

        public static void IncreaseMoney(ref int money, int moneyToBeAdded)
        {
            money += moneyToBeAdded;
        }

    }
}
