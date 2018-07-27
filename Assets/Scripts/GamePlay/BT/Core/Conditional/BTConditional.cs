using UnityEngine;
using System.Collections;

namespace BT {

	/// <summary>
	/// BTConditional is the base class for conditional nodes.
	/// It is usually used to check conditions.
	/// 
	/// Concrete conditional classes inheriting from this class should override the Check method.
	/// </summary>
	public abstract class BTConditional : BTNode {

		sealed public override BTResult Tick () {
			if (Check()) {
				return BTResult.Success;
			}
			else {
				return BTResult.Failed;
			}
		}

		/// <summary>
		/// This is where the condition check happens.
		/// </summary>
		public virtual bool Check () {
			return false;
		}
	}

    public class BaseCondiction : BTConditional
    {
        public delegate bool ExternalFunc();
        protected ExternalFunc externalFunc;
    }

    public class Precondition : BaseCondiction
    {
        public Precondition(ExternalFunc func, string nodeName) { externalFunc = func; name = nodeName; }

        public override bool Check()
        {
            if (externalFunc != null) return externalFunc();
            else return false;
        }
    }

    public class PreconditionNOT : BaseCondiction
    {
        public PreconditionNOT(ExternalFunc func, string nodeName) { externalFunc = func; name = nodeName; }

        public override bool Check()
        {
            if (externalFunc != null) return !externalFunc();
            else return false;
        }
    }

}