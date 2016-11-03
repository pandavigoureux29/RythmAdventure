﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class ProfileManager : MonoBehaviour {

    private static ProfileManager _instance;

    public Profile profile;

    [SerializeField] bool m_resetPrefsAtLaunch = false;
    
    public static ProfileManager instance{
        get{
            if(_instance == null)
            {
                GameObject go = Instantiate(Resources.Load("prefabs/Profile") as GameObject);
                _instance = go.GetComponent<ProfileManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;

        if( m_resetPrefsAtLaunch)
            PlayerPrefs.DeleteAll();

        LoadProfile();
        DontDestroyOnLoad(this.gameObject);
    }

    #region PROFILE_SAVE_LOAD
    public void ResetProfile()
    {
        LoadDefaultProfile();
    }

    void LoadProfile()
    {
        string json = PlayerPrefs.GetString("profile");
        if (string.IsNullOrEmpty(json))
        {
            //Load profile by default
            LoadDefaultProfile();
            return;
        }
        //PArse JSON
        profile = JsonUtility.FromJson<Profile>(json);
    }

    public void SaveProfile()
    {
        string json = JsonUtility.ToJson(profile);
        PlayerPrefs.SetString("profile", json);
        Debug.Log("[Saved Profile] " + json);
    }

#if UNITY_EDITOR
    public void SaveDefaultProfile()
    {
        string path = Application.dataPath + "/Resources/database/defaultProfile.json";
        string text = JsonUtility.ToJson(profile);

        System.IO.File.WriteAllText(path, text);
        AssetDatabase.Refresh();
    }
#endif

    void LoadDefaultProfile()
    {
        TextAsset json = Resources.Load("database/defaultProfile") as TextAsset;
        profile = JsonUtility.FromJson(json.text, typeof(Profile)) as Profile;
        Resources.UnloadAsset(json);
    }

    #endregion

    public Profile GetProfile()
    {
        return profile;
    }

    #region CHARACTERS

    public CharacterData GetCharacter(string _id)
    {
        foreach (var chara in profile.Characters)
        {
            if (chara.Id == _id)
                return chara;
        }
        return null;
    }

    public Stats GetCharacterStats(string _id)
    {
        var chara = GetCharacter(_id);
        var levelupdata = DataManager.instance.CharacterManager.GetLevelByXp(chara.Job, chara.Xp);
        return levelupdata!=null ? levelupdata.Stats : null;
    }

    public void AddCharacterXp(string _id, int _xp)
    {
        var charaSave = GetCharacter(_id);
        if( charaSave != null)
        {
            charaSave.Xp += _xp;
        }
    }

    public List<CharacterData> GetCurrentTeam()
    {
        List<CharacterData> chars = new List<CharacterData>();
        foreach (string teamMateName in profile.CurrentTeam)
        {
            var charaSave = GetCharacter(teamMateName);
            if (charaSave != null)
                chars.Add(charaSave);
        }
        return chars;
    }

    public void ReplacePartyCharacter(string oldCharId, string newCharId) 
    {
        for(int i=0; i < profile.CurrentTeam.Count; ++i)
        {
            if( profile.CurrentTeam[i] == oldCharId)
            {
                profile.CurrentTeam[i] = newCharId;
                break;
            }
        }
        SaveProfile();
    }

    #endregion

    #region MAPS

    public Map GetMapData(string _mapName)
    {
        foreach( var map in profile.Maps)
        {
            if( map.Name.ToUpper() == _mapName.ToUpper() )
            {
                return map;
            }
        }
        return null;
    }

    #endregion

    [System.Serializable]
    public class Profile
    {
        [SerializeField] public string PlayerName = "player";
        [SerializeField] public int Xp = 0;
        [SerializeField] public bool Initialized = false;

        [SerializeField] public List<CharacterData> Characters = new List<CharacterData>();

        [SerializeField] public List<string> CurrentTeam;

        //Progression
        [SerializeField] public List<Map> Maps = new List<Map>();

        public Profile()
        {
            Characters = new List<CharacterData>()
            {
                new CharacterData("player1"),
                new CharacterData("player2"),
                new CharacterData("player3")
            };

            CurrentTeam = new List<string>() { "player1", "player3", "player1" };
        }

    }

}
