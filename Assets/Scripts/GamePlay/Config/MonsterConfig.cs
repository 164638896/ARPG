using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class MonsterConfig
{
    private Dictionary<int, MonsterData> mMonsterConfig = new Dictionary<int, MonsterData>();

    static MonsterConfig _singleton = null;
    static public MonsterConfig singleton
    {
        get
        {
            if (_singleton == null) _singleton = new MonsterConfig();
            return _singleton;
        }
    }

    public IEnumerator load(string filePath)
    {
        string url = filePath + "/config/monster.xml";

        WWW www = new WWW(url);
        yield return www;

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(www.text);
        XmlNode root = xmldoc.SelectSingleNode("root");

        XmlNodeList childs = root.SelectNodes("monster");
        foreach (XmlNode skillNode in childs)
        {
            MonsterData info = new MonsterData();

            XmlElement skillxml = (XmlElement)skillNode;

            int id = int.Parse(skillxml.GetAttribute("id"));
            string res = skillxml.GetAttribute("res");
            info.mHp = int.Parse(skillxml.GetAttribute("hp"));
            info.mAtk = int.Parse(skillxml.GetAttribute("atk"));
            info.mDef = int.Parse(skillxml.GetAttribute("def"));
            info.mMoveSpeed = int.Parse(skillxml.GetAttribute("MoveSpeed"));
            info.mObjPrefab = Resources.Load(res, typeof(GameObject));
            ParseString(ref info.mSkillList, skillxml.GetAttribute("Skill"));

            mMonsterConfig.Add(id, info);
        }
    }

    public void ParseString(ref List<int> outList, string info)
    {
        string[] sArray = info.Split(',');
        for (int i = 0; i < sArray.Length; ++i)
        {
            if (sArray[i] != string.Empty)
            {
                outList.Add(int.Parse(sArray[i]));
            }
        }
    }

    public MonsterData GetPlayerData(int typeId)
    {
        MonsterData data;
        mMonsterConfig.TryGetValue(typeId, out data);
        return data;
    }
}