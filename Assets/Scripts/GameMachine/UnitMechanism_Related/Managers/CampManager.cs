using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class CampManager : MonoBehaviour
{
    public static CampManager Instance;

    void Awake()
    {
        Instance = this;

    }

    private Dictionary<int,Camp> Camps;

    void Start()
    {
        Camps = new Dictionary<int, Camp>();
        createCamp();
        createCamp();
        // foreach (Camp camp in Camps.Values)
        // {
        //     Debug.Log(camp.campName);
        // }

        setCampRelation(1, 2, false);

    }


    public void createCamp()
    {
        int currID = Camps.Count() + 1;
        Camps.Add(currID, new Camp( currID, "Camp" + currID));
        
    }

    public void createCamp(String campName)
    {
        int currID = Camps.Count() + 1;
        Camps.Add(currID, new Camp( currID, campName));
        
    }

    public Camp getCamp(int campID)
    {
        return Camps[campID];
    }

    public void setCampRelation(int campID1, int campID2, bool isFriendly)
    {
        Camp camp1 = Camps[campID1];
        Camp camp2 = Camps[campID2];

        if (isFriendly && !camp1.friendlyCamp.Contains(camp2) && !camp2.friendlyCamp.Contains(camp1) )
        {
            camp1.friendlyCamp.Add(camp2);
            camp2.friendlyCamp.Add(camp1);
        }
        else
        {
            if (!camp1.enemyCamp.Contains(camp2) && !camp2.enemyCamp.Contains(camp1) )
            {
                camp1.enemyCamp.Add(camp2);
                camp2.enemyCamp.Add(camp1);
            }
            
        }
    }

    public bool isCampFriendly(int campID1, int campID2)
    {
        Camp camp1 = Camps[campID1];
        Camp camp2 = Camps[campID2];

        if (camp1.friendlyCamp.Contains(camp2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isCampEnemy(int campID1, int campID2)
    {
        Camp camp1 = Camps[campID1];
        Camp camp2 = Camps[campID2];

        if (camp1.enemyCamp.Contains(camp2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public class Camp
    {
        public int id;
        public String campName;

        public List<Camp> friendlyCamp = new List<Camp>();
        public List<Camp> enemyCamp = new List<Camp>();

        public Camp(int id, String name)
        {
            this.id = id;
            campName = name;
        }
    }

}


