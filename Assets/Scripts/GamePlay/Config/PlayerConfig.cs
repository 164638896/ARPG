using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class PlayerConfig
{
    private Dictionary<int/*typeId*/, PlayerData> mPlayerConfig = new Dictionary<int, PlayerData>();

    static PlayerConfig _singleton = null;
    static public PlayerConfig singleton
    {
        get
        {
            if (_singleton == null) _singleton = new PlayerConfig();
            return _singleton;
        }
    }

    public IEnumerator load(string filePath)
    {
        string url = filePath + "/config/player.xml";
        WWW www = new WWW(url);
        yield return www;

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(www.text);
        XmlNode root = xmldoc.SelectSingleNode("root");

        XmlNodeList childs = root.SelectNodes("player");
        foreach (XmlNode skillNode in childs)
        {
            PlayerData info = new PlayerData();

            XmlElement skillxml = (XmlElement)skillNode;

            int typeId = int.Parse(skillxml.GetAttribute("id"));
            string res = skillxml.GetAttribute("res");
            info.mHp = int.Parse(skillxml.GetAttribute("hp"));
            info.mAtk = int.Parse(skillxml.GetAttribute("atk"));
            info.mDef = int.Parse(skillxml.GetAttribute("def"));
            info.mMoveSpeed = int.Parse(skillxml.GetAttribute("MoveSpeed"));
            info.mObjPrefab = Resources.Load(res, typeof(GameObject));

            ParseString(ref info.mMulSkillList, skillxml.GetAttribute("MulSkill"));
            ParseString(ref info.mSkillList, skillxml.GetAttribute("Skill"));

            mPlayerConfig.Add(typeId, info);
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

    public PlayerData GetPlayerData(int typeId)
    {
        PlayerData data;
        mPlayerConfig.TryGetValue(typeId, out data);
        return data;
    }
}
