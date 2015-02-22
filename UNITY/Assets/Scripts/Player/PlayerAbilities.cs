using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using Rewired;

public class PlayerAbilities : MonoBehaviour 
{
    [SerializeField]
    public List<Stats> attributes = new List<Stats>();

    [SerializeField]
    public List<Abilities> abilities = new List<Abilities>();

    public AnimationCurve enduranceLevel = new AnimationCurve(new Keyframe(4.0f, 5.0f), new Keyframe(5.0f, 1.0f), new Keyframe(9.0f, 0.0f));

    private Player playerInfo;

    private void Awake()
    {
       InitializeAttributes();
       playerInfo = Rewired.ReInput.players.GetPlayer(0);
    }

    protected void InitializeAttributes()
    {
        attributes[0].damageResist = 0.0f;
        attributes[0].agility = 0;
        attributes[0].maxStamina = 10;
        attributes[0].stamina = 1.0f;
        attributes[0].xp = 0;
        attributes[0].hp = 100;
        attributes[0].skillPoints = 0;
        attributes[0].money = 100;
    }

    //private void OnGUI()
    //{
       // Vector2 size = new Vector2(240, 120);
        //float margin = 20.0f;

        //GUI.Label(new Rect(margin + size.y, Screen.height - (size.x + margin), size.x, size.y), "Health " + attributes[0].hp.ToString());
        //GUI.Label(new Rect(margin + size.y, Screen.height - (size.x), size.x, size.y), "Money " + attributes[0].money.ToString());
       // GUI.Label(new Rect(margin + size.y, Screen.height - (size.x - margin), size.x, size.y), "XP " + attributes[0].xp.ToString());


       // for (int i = 0; i < attributes.Count; i++)
       // {
       //     GUI.Label(new Rect(margin * (i + 1) + (size.x * i), Screen.height - (size.y + margin), size.x, size.y), attributes[0].ToString());
       // }
    //}

    [System.Serializable]
    public class Stats
    {
        public int hp;
        public int xp;
        public float stamina;
        public int maxStamina;
        public int agility;
        public int skillPoints;
        public int maxHealth;
        public float damageResist;
        public int money;
    }

    [System.Serializable]
    public class Abilities
    {
        public string stuff;
    }
}