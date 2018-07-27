using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuffCofig
{
    private Dictionary<int/*TypeId*/, MoveBuffConfig> mMoveBuffConfigDict = new Dictionary<int, MoveBuffConfig>();
    private Dictionary<int/*TypeId*/, BehaviorBuffConfig> mBehaviorBuffConfigDict = new Dictionary<int, BehaviorBuffConfig>();
    private Dictionary<int/*TypeId*/, HurtBuffConfig> mHurtBuffConfigDict = new Dictionary<int, HurtBuffConfig>();
    private Dictionary<int/*TypeId*/, ControlBuffConfig> mControlBuffConfigDict = new Dictionary<int, ControlBuffConfig>();
    private Dictionary<int/*TypeId*/, AreaTriggerBuffConfig> mAreaTriggerBuffConfigDict = new Dictionary<int, AreaTriggerBuffConfig>();

    static BuffCofig _singleton = null;

    static public BuffCofig singleton
    {
        get
        {
            if (_singleton == null) _singleton = new BuffCofig();
            return _singleton;
        }
    }

    public IEnumerator load(string filePath)
    {
        string url = filePath + "/config/buff.xml";
        WWW www = new WWW(url);
        yield return www;

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(www.text);
        XmlNode root = xmldoc.SelectSingleNode("root");

        XmlNode behaviorNode = root.SelectSingleNode("behavior");
        XmlNodeList behaviorChilds = behaviorNode.SelectNodes("buff");
        foreach (XmlNode buffNode in behaviorChilds)
        {
            BehaviorBuffConfig info = new BehaviorBuffConfig();
            XmlElement buffxml = (XmlElement)buffNode;
            info.mType = IBuff.BuffType.Behavior;
            info.mTypeId = int.Parse(buffxml.GetAttribute("id"));
            info.mName = buffxml.GetAttribute("Name");
            info.mDuration = float.Parse(buffxml.GetAttribute("Duration"));
            info.mCanAtk = int.Parse(buffxml.GetAttribute("CanAtk")) == 1 ? true : false;
            info.mCanMove = int.Parse(buffxml.GetAttribute("CanMove")) == 1 ? true : false;

            mBehaviorBuffConfigDict.Add(info.mTypeId, info);
        }

        XmlNode moveNode = root.SelectSingleNode("move");
        XmlNodeList moveChilds = moveNode.SelectNodes("buff");
        foreach (XmlNode buffNode in moveChilds)
        {
            MoveBuffConfig info = new MoveBuffConfig();
            XmlElement buffxml = (XmlElement)buffNode;
            info.mType = IBuff.BuffType.Move;
            info.mTypeId = int.Parse(buffxml.GetAttribute("id"));
            info.mName = buffxml.GetAttribute("Name");
            info.mDir = (MoveBuff.MoveDir)int.Parse(buffxml.GetAttribute("Dir"));
            info.mDuration = float.Parse(buffxml.GetAttribute("Duration"));
            info.mDistance = float.Parse(buffxml.GetAttribute("Distance"));
            mMoveBuffConfigDict.Add(info.mTypeId, info);
        }

        XmlNode hurtNode = root.SelectSingleNode("hurt");
        XmlNodeList hurtChilds = hurtNode.SelectNodes("buff");
        foreach (XmlNode buffNode in hurtChilds)
        {
            HurtBuffConfig info = new HurtBuffConfig();
            XmlElement buffxml = (XmlElement)buffNode;
            info.mType = IBuff.BuffType.Hurt;
            info.mTypeId = int.Parse(buffxml.GetAttribute("id"));
            info.mName = buffxml.GetAttribute("Name");
            info.mDuration = float.Parse(buffxml.GetAttribute("Duration"));
            info.mStateName = buffxml.GetAttribute("StateName");
            mHurtBuffConfigDict.Add(info.mTypeId, info);
        }

        XmlNode controlNode = root.SelectSingleNode("control");
        XmlNodeList controlChilds = controlNode.SelectNodes("buff");
        foreach (XmlNode buffNode in controlChilds)
        {
            ControlBuffConfig info = new ControlBuffConfig();
            XmlElement buffxml = (XmlElement)buffNode;
            info.mType = IBuff.BuffType.Control;
            info.mTypeId = int.Parse(buffxml.GetAttribute("id"));
            info.mName = buffxml.GetAttribute("Name");
            info.mDuration = float.Parse(buffxml.GetAttribute("Duration"));
            info.mClearBuff = int.Parse(buffxml.GetAttribute("ClearBuff")) == 1 ? true : false;
            info.mEnableBehavior = int.Parse(buffxml.GetAttribute("EnableBehavior")) == 1 ? true : false;
            info.mEnableMove = int.Parse(buffxml.GetAttribute("EnableMove")) == 1 ? true : false;
            info.mEnableHurt = int.Parse(buffxml.GetAttribute("EnableHurt")) == 1 ? true : false;
            info.mEnableSelect = int.Parse(buffxml.GetAttribute("EnableSelect")) == 1 ? true : false;
            mControlBuffConfigDict.Add(info.mTypeId, info);
        }
    }

    public IEnumerator loadAreaTriggerBuff(string filePath)
    {
        string url = filePath + "/config/AreaTriggerBuff.xml";
        WWW www = new WWW(url);
        yield return www;

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(www.text);
        XmlNode root = xmldoc.SelectSingleNode("root");

        XmlNodeList areaBuffChilds = root.SelectNodes("AreaBuff");
        foreach (XmlNode buffNode in areaBuffChilds)
        {
            AreaTriggerBuffConfig info = new AreaTriggerBuffConfig();
            XmlElement buffxml = (XmlElement)buffNode;
            info.mTypeId = int.Parse(buffxml.GetAttribute("id"));
            info.mName = buffxml.GetAttribute("Name");
            info.mDuration = float.Parse(buffxml.GetAttribute("Duration"));
            info.mShape = (AreaTriggerBuff.Shape)int.Parse(buffxml.GetAttribute("Shape"));
            SkillConfig.ParseVector3(ref info.mRadius, buffxml.GetAttribute("Radius"));
            info.mDelayTime = float.Parse(buffxml.GetAttribute("DelayTime"));
            info.mRepeatRate = float.Parse(buffxml.GetAttribute("RepeatRate"));
            info.mDestroy = (AreaTriggerBuff.Destroy)int.Parse(buffxml.GetAttribute("Destroy"));

            //info.mEffect
            //info.mDestroyEffect
            
            string behaviorBuff = buffxml.GetAttribute("BehaviorBuff");
            SkillConfig.ParseBuffString(ref info.mBuffIdList, behaviorBuff, IBuff.BuffType.Behavior);

            string moveBuff = buffxml.GetAttribute("MoveBuff");
            SkillConfig.ParseBuffString(ref info.mBuffIdList, moveBuff, IBuff.BuffType.Move);

            string hurtBuff = buffxml.GetAttribute("HurtBuff");
            SkillConfig.ParseBuffString(ref info.mBuffIdList, hurtBuff, IBuff.BuffType.Hurt);

            string controlBuff = buffxml.GetAttribute("ControlBuff");
            SkillConfig.ParseBuffString(ref info.mBuffIdList, controlBuff, IBuff.BuffType.Control);

            mAreaTriggerBuffConfigDict.Add(info.mTypeId, info);
        }
    }

    public BehaviorBuffConfig GetBehaviorBuffConfig(int typeId)
    {
        BehaviorBuffConfig info;
        mBehaviorBuffConfigDict.TryGetValue(typeId, out info);
        return info;
    }

    public MoveBuffConfig GetMoveBuffConfig(int typeId)
    {
        MoveBuffConfig info;
        mMoveBuffConfigDict.TryGetValue(typeId, out info);
        return info;
    }

    public HurtBuffConfig GetHurtBuffConfig(int typeId)
    {
        HurtBuffConfig info;
        mHurtBuffConfigDict.TryGetValue(typeId, out info);
        return info;
    }

    public ControlBuffConfig GetControlBuffConfig(int typeId)
    {
        ControlBuffConfig info;
        mControlBuffConfigDict.TryGetValue(typeId, out info);
        return info;
    }

    public AreaTriggerBuffConfig GetAreaTriggerBuffConfig(int typeId)
    {
        AreaTriggerBuffConfig info;
        mAreaTriggerBuffConfigDict.TryGetValue(typeId, out info);
        return info;
    }
}
