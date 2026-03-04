using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.Actor.Managers
{
    public class UnitUpdateManager : AManager<UnitUpdateManager>
    {
        void Update()
        {
            float dt = Time.deltaTime * UnitTimeFlowManager.TimeFlow;
            for (int i = 0; i < UnitRegisterManager.RegisteredUnits.Count; i++)
                UnitRegisterManager.RegisteredUnits[i].OnUpdate(dt);
        }
        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime * UnitTimeFlowManager.TimeFlow;
            for (int i = 0; i < UnitRegisterManager.RegisteredUnits.Count; i++)
                UnitRegisterManager.RegisteredUnits[i].OnFixedUpdate(dt);
        }
        void LateUpdate()
        {
            float dt = Time.deltaTime * UnitTimeFlowManager.TimeFlow;
            for (int i = 0; i < UnitRegisterManager.RegisteredUnits.Count; i++)
                UnitRegisterManager.RegisteredUnits[i].OnLateUpdate(dt);
        }
    }
}