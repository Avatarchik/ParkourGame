using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerAbilities : MonoBehaviour 
{
    [SerializeField]
    public List<Stats> attributes = new List<Stats>();

    [SerializeField]
    public List<Abilities> abilities = new List<Abilities>();

    public AnimationCurve enduranceLevel = new AnimationCurve(new Keyframe(4.0f, 5.0f), new Keyframe(5.0f, 1.0f), new Keyframe(9.0f, 0.0f));

    private void Awake()
    {
       // InitializeAttributes();
    }

    protected void InitializeAttributes()
    {

    }

    private void Update()
    {
        
    }

    [System.Serializable]
    public class Stats
    {
        public int hp;
        public int xp;
        public int stamina;
        public int maxStamina;
        public int agility;
        public int skillPoints;
        public int maxHealth;
        public int damageResist;
        public int money;
    }

    [System.Serializable]
    public class Abilities
    {
        public string stuff;
    }
}