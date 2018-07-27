using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RoleMgr
{
    private Dictionary<int, GameObject> mPlayerDict = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> mMonsterDict = new Dictionary<int, GameObject>();

    SortedList<float, Role> sortedList = new SortedList<float, Role>();

    public int mInstanceId = 0;

    static RoleMgr _singleton = null;
    static public RoleMgr singleton
    {
        get
        {
            if (_singleton == null) _singleton = new RoleMgr();
            return _singleton;
        }
    }

    public GameObject CreatePlayer(int typeId, GroupType groupType, Vector3 pos, Vector3 eulerAngles)
    {
        PlayerData data = PlayerConfig.singleton.GetPlayerData(typeId);
        data.mInstId = mInstanceId++;
        data.mGroupType = groupType;

        GameObject playerGO = UnityEngine.Object.Instantiate(data.mObjPrefab) as GameObject;
        Player player = playerGO.GetComponent<Player>();
        if (player)
        {
            player.mRoleData = data;
        }

        playerGO.transform.position = pos;
        playerGO.transform.Rotate(eulerAngles);

        mPlayerDict.Add(data.mInstId, playerGO);

        return playerGO;
    }

    public void CreateMonster(int typeId, GroupType groupType, Vector3 pos, Vector3 eulerAngles)
    {
        MonsterData data = MonsterConfig.singleton.GetPlayerData(typeId);
        data.mInstId = mInstanceId++;
        data.mGroupType = groupType;

        GameObject monsterGO = UnityEngine.Object.Instantiate(data.mObjPrefab) as GameObject;
        Monster monster = monsterGO.GetComponent<Monster>();
        if (monster)
        {
            monster.mRoleData = data;
        }

        monsterGO.transform.position = pos;
        monsterGO.transform.Rotate(eulerAngles);

        mMonsterDict.Add(data.mInstId, monsterGO);
    }

    public int FindNearMonster(Vector3 heroPos)
    {
        sortedList.Clear();

        foreach (var item in mMonsterDict)
        {
            GameObject targetGameObject = item.Value;
            if (targetGameObject != null)
            {
                Role role = targetGameObject.GetComponent<Role>();
                if (role.mBuffSystem.EnableSelect() && role.mRoleData.mHp > 0)
                {
                    float dis = Vector3.Distance(role.transform.position, heroPos);

                    if (!sortedList.ContainsKey(dis))
                    {
                        sortedList.Add(dis, role);
                    }
                }
            }
        }

        foreach (var item in sortedList)
        {
            return item.Value.mRoleData.mInstId;
        }

        return -1;
    }

    public Role GetRole(int instanceId)
    {
        GameObject go = GetMonster(instanceId);
        if (go == null)
        {
            go = GetPlayer(instanceId);
        }

        if (go == null)
            return null;

        return go.GetComponent<Role>();
    }

    public GameObject GetPlayer(int instanceId)
    {
        GameObject go;
        mPlayerDict.TryGetValue(instanceId, out go);
        return go;
    }

    public GameObject GetMonster(int instanceId)
    {
        GameObject go;
        mMonsterDict.TryGetValue(instanceId, out go);
        return go;
    }
}