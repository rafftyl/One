using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace One
{
	public abstract class InjectionRuleProvider : MonoBehaviour
	{
		public abstract Dictionary<System.Type, System.Type> InjectionRules { get; }
	}
}
