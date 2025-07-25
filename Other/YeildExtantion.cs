using System;
using UnityEngine;

namespace PaleCourtCharms
{
    public sealed class WaitSecWhile : CustomYieldInstruction
    {
        private float _time;
        private Func<bool> _pred;
        
        public WaitSecWhile(Func<bool> predicate, float seconds)
        {
            _pred = predicate;
            _time = seconds;
        }
        
        public override bool keepWaiting
        {
            get
            {
                _time -= Time.unscaledDeltaTime;
                
                return _pred() && _time > 0f;
            }
        }
    }
}