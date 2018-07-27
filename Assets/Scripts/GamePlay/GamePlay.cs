using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay : MonoBehaviour
{
    void Awake()
    {
        string filePath =
#if UNITY_ANDROID && !UNITY_EDITOR
                      Application.streamingAssetsPath;
#elif UNITY_IPHONE && !UNITY_EDITOR
                       file:// + Application.dataPath +"/Raw/"
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
                      "file://" + Application.dataPath + "/StreamingAssets/";
#else
                       string.Empty;  
#endif

        Application.targetFrameRate = 30;

        LoadAll(filePath);
    }

    public void LoadAll(string filePath)
    {
        // load skill
        StartCoroutine(SkillConfig.singleton.load(filePath));

        // load buff
        StartCoroutine(BuffCofig.singleton.load(filePath));
        StartCoroutine(BuffCofig.singleton.loadAreaTriggerBuff(filePath));

        // load player
        StartCoroutine(PlayerConfig.singleton.load(filePath));

        // load moster
        StartCoroutine(MonsterConfig.singleton.load(filePath));

        Invoke("CreateRole", 0.1f);
    }

    public void CreateRole()
    {
        RoleMgr.singleton.CreatePlayer(1, GroupType.FriendPlayer, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        RoleMgr.singleton.CreateMonster(1, GroupType.Monster, new Vector3(1, 0, 0), new Vector3(0, 180, 0));
        RoleMgr.singleton.CreateMonster(2, GroupType.Monster, new Vector3(0, 0, 1), new Vector3(0, 180, 0));
    }

    private void Start()
    {

    }

    void Update ()
    {
        TimeTaskManager.Tick();
        AreaBuffTriggerMgr.singleton.OnUpdate();
    }
}
