using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.CustomRandom.Global;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules
{
    public class HitReactionBehaviourModule : Module, IUpdateModule
    {
        [Dependency] private BigHealthBehaviourModule _bigHealthBehaviourModule;
        private HitReactionBehaviour _template;
        public HitReactionBehaviourModule Construct(HitReactionBehaviour template)
        {
            _template = template;
            return this;
        }
        protected override void OnStart()
        {
            if (TryGet(out MovementBehaviourModule movement))
            {
                _bigHealthBehaviourModule.OnDamaged += (x)
                    => movement.AdditionalVelocity += x.Direction.normalized * x.Damage * _template.reactionPower;

                _bigHealthBehaviourModule.OnDamaged += (x) =>
                {
                    //var sound = CMS_Manager.CMS
                        //.Get(_template.soundEffects[GlobalRandom.Current.Next(_template.soundEffects.Length)]).POOL();
                    //sound.transform.position = x.position;
                };
                _bigHealthBehaviourModule.OnDamaged += (x) =>
                {
                    // var vfx = CMS_Manager.CMS
                    //     .Get(_template.particleEffects[GlobalRandom.Current.Next(_template.particleEffects.Length)]).POOL();
                    // vfx.transform.position = Unit.transform.position;
                };
                
                _bigHealthBehaviourModule.OnDamaged += (x) => hitScale += new Vector3(
                    GlobalRandom.Current.NextFloat(),
                    GlobalRandom.Current.NextFloat(),
                    GlobalRandom.Current.NextFloat()
                    ).normalized * x.Damage * _template.reactionPower;
            }

            //Controller.UnitScale.Add_PostAdditional(() => hitScale);
        }

        private Vector3 hitScale;
        public void OnUpdate(float deltaTime)
        {
            hitScale = Vector3.Lerp(hitScale, Vector3.zero, deltaTime * 15f);
        }
    }
    
    [Serializable]
    public struct HitReactionBehaviour : ModuleTemplate<HitReactionBehaviourModule>
    {
        public float reactionPower;
    }
}