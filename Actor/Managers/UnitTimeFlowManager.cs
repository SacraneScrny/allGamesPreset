using Sackrany.Utils;
using Sackrany.Variables.ExpandedVariable.Entities;

namespace Sackrany.Actor.Managers
{
    public class UnitTimeFlowManager : AManager<UnitTimeFlowManager>
    {
        public ExpandedFloat UnitsTimeFlow = new (1f);
        
        float _lastTimeFlow = 0f;
        public static float TimeFlow => Instance._lastTimeFlow;
        private protected override void OnInitialize()
        {
            _lastTimeFlow = UnitsTimeFlow;
        }
        void Update()
        {
            _lastTimeFlow = UnitsTimeFlow;
        }
    }
}