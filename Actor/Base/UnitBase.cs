using Sackrany.Actor.Modules.Modules;

namespace Sackrany.Actor.Base
{
    public abstract class UnitBase
    {
        protected Unit.Unit Unit;
        protected ModulesController Controller;
        
        public bool HasUnit => Unit != null;
        public bool HasController => Controller != null;
        
        public void FillUnit(Unit.Unit unit) => Unit = unit;
        public void FillController(ModulesController controller) => Controller = controller;
    }
}