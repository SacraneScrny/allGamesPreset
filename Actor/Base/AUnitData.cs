using Sackrany.Actor.UnitMono;

namespace Sackrany.Actor.Base
{
    public abstract class AUnitData
    {
        private protected Unit _unit;
        public void Initialize(Unit unit)
        {
            _unit = unit;
            OnInitialize();
        }
        private protected virtual void OnInitialize() { }
        
        public abstract void Reset();
    }
}