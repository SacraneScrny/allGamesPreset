namespace Sackrany.Actor.Base
{
    public abstract class AUnitData
    {
        private protected Unit.Unit _unit;
        public void Initialize(Unit.Unit unit)
        {
            _unit = unit;
            OnInitialize();
        }
        private protected virtual void OnInitialize() { }
        
        public abstract void Reset();
    }
}