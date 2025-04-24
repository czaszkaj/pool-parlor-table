using UnityEngine;

namespace YAPT.Global.Rules
{
    public abstract class RuleBase : ScriptableObject
    {
        public abstract void ApplyRule();
    }
}