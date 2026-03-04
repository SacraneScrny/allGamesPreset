using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Utils.Pool.Extensions;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules
{
    public class BulletBehaviourModule : Module, IUpdateModule
    {
        [Template] BulletBehaviour bullet;

        float time;
        public void OnUpdate(float deltaTime)
        {
            //Unit.gameObject.name = $"Bullet {time}";
            time += deltaTime;
            Unit.transform.Translate(Unit.transform.forward * deltaTime * bullet.speed, Space.World);
            if (time >= 3f)
                Unit.RELEASE();
        }
    }
    
    [Serializable]
    public struct BulletBehaviour : ModuleTemplate<BulletBehaviourModule>
    {
        [HashKey] public float speed;
    }
}