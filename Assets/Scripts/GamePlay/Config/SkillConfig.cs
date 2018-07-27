using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuffId // 配置在skill表
{
    public IBuff.BuffAttach mBuffAttach;    // 谁的buff
    public IBuff.BuffStage mTrigger;      // buff什么时候触发
    public int mTypeId;                     // buff 类型ID

    public IBuff.BuffType mType;            // buff类型
}

public class SkillConfig
{
    public enum TargetType
    {
        None = 0,
        Enemy = 1,
        Friends = 2,
        Self = 3,
    }

    //这个需要做对象池,后面做
    public class SkillInfo
    {
        public int mSkillTypeId;
        public GameObject mTimeline;
        public GameObject mSelfEffect;
        public GameObject mTargetEffect;
        public string mStateName;
        public float mAtkDistance = 1.0f;
        //public bool mbTarget; // 是否需要目标
        public TargetType mTargetType;
        public List<BuffId> mBuffIdList = new List<BuffId>(); // buffId
    }

    private Dictionary<int/*typeId*/, SkillInfo> mSkillDict = new Dictionary<int, SkillInfo>();

    static SkillConfig _singleton = null;
    static public SkillConfig singleton
    {
        get
        {
            if (_singleton == null) _singleton = new SkillConfig();
            return _singleton;
        }
    }

    public IEnumerator load(string filePath)
    {
        string url = filePath + "/config/skills.xml";
        WWW www = new WWW(url);
        yield return www;

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(www.text);
        XmlNode root = xmldoc.SelectSingleNode("root");

        XmlNodeList childs = root.SelectNodes("skill");
        foreach (XmlNode skillNode in childs)
        {
            SkillInfo info = new SkillInfo();

            XmlElement skillxml = (XmlElement)skillNode;

            info.mSkillTypeId = int.Parse(skillxml.GetAttribute("id"));
            string timelinePath = skillxml.GetAttribute("TimeLine");

            info.mStateName = skillxml.GetAttribute("StateName");
            string strDis = skillxml.GetAttribute("Distance");
            if (strDis != string.Empty) info.mAtkDistance = float.Parse(strDis);

            info.mTargetType = (SkillConfig.TargetType)int.Parse(skillxml.GetAttribute("TargetType"));

            if (timelinePath != string.Empty)
            {
                Object objPrefab = Resources.Load(timelinePath, typeof(GameObject));
                if(objPrefab != null)
                {
                    info.mTimeline = UnityEngine.Object.Instantiate(objPrefab) as GameObject;
                }
            }

            string selfEffectPath = skillxml.GetAttribute("SelfEffect");
            if (selfEffectPath != string.Empty)
            {
                Object objPrefab = Resources.Load(selfEffectPath, typeof(GameObject));
                if(objPrefab != null)
                {
                    info.mSelfEffect = UnityEngine.Object.Instantiate(objPrefab) as GameObject;
                    info.mSelfEffect.SetActive(false);
                }
            }

            string targetEffectPath = skillxml.GetAttribute("TargetEffect");
            if (targetEffectPath != string.Empty)
            {
                Object objPrefab = Resources.Load(targetEffectPath, typeof(GameObject));
                if(objPrefab != null)
                {
                    info.mTargetEffect = UnityEngine.Object.Instantiate(objPrefab) as GameObject;
                    info.mTargetEffect.SetActive(false);
                }
            }

            string areaBuff = skillxml.GetAttribute("AreaBuff");
            ParseBuffString(ref info.mBuffIdList, areaBuff, IBuff.BuffType.None);

            string behaviorBuff = skillxml.GetAttribute("BehaviorBuff");
            ParseBuffString(ref info.mBuffIdList, behaviorBuff, IBuff.BuffType.Behavior);

            string moveBuff = skillxml.GetAttribute("MoveBuff");
            ParseBuffString(ref info.mBuffIdList, moveBuff, IBuff.BuffType.Move);

            string hurtBuff = skillxml.GetAttribute("HurtBuff");
            ParseBuffString(ref info.mBuffIdList, hurtBuff, IBuff.BuffType.Hurt);

            string controlBuff = skillxml.GetAttribute("ControlBuff");
            ParseBuffString(ref info.mBuffIdList, controlBuff, IBuff.BuffType.Control);

            mSkillDict.Add(info.mSkillTypeId, info);
        }
    }

    public SkillInfo GetSkillInfo(int skillId)
    {
        SkillInfo info = null;
        mSkillDict.TryGetValue(skillId, out info);
        return info;
    }

    static public void ParseBuffString(ref List<BuffId> outList, string info, IBuff.BuffType type)
    {
        string[] sArray = info.Split(';');
        for (int i = 0; i < sArray.Length; ++i)
        {
            if (sArray[i] != string.Empty)
            {
                string[] sArrayTrigger = sArray[i].Split(',');
                BuffId buff = new BuffId();
                buff.mType = type;
                buff.mBuffAttach = (IBuff.BuffAttach)int.Parse(sArrayTrigger[0]);
                buff.mTrigger = (IBuff.BuffStage)int.Parse(sArrayTrigger[1]);
                buff.mTypeId = int.Parse(sArrayTrigger[2]);
                outList.Add(buff);
            }
        }
    }

    static public void ParseVector3(ref Vector3 v, string info)
    {
        string[] sArray = info.Split(',');
        if (sArray.Length == 3)
        {
            v.x = int.Parse(sArray[0]);
            v.y = int.Parse(sArray[1]);
            v.z = int.Parse(sArray[2]);
        }
    }
}
