using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Variables.ExpandedVariable.Entities;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules
{
    public class SoundBehaviourModule : Module
    {
        [Dependency] SoundBehaviour _template;
        [Dependency] MovementBehaviourModule _movement;
        [Dependency(true)] JumpingBehaviourModule _jumping;
        
        private ExpandedFloat _stepDistance;

        /*private Queue<HashCMS> _randomSteps = new Queue<HashCMS>();
        private Queue<HashCMS> _randomJumps = new Queue<HashCMS>();

        protected override void OnAwake()
        {
            _stepDistance = _template.stepDistance;
        }
        protected override void OnStart()
        {
            UpdateRandomLists();
            
            _stepDistance.Clear();
            _stepDistance.Add_Multiply(() => 1f / _movement.LastMoveSpeed);
            _stepDistance.Add_PostAdditional(() => _movement.IsGrounded ? 0 : 100);
            _movement.OnGrounded += (_) => distance = float.MaxValue;
            
            if (_jumping != null)
            {
                _jumping.OnJump += () =>
                {
                    // var sound = _randomJumps.Dequeue();
                    // var soundObject = CMS_Manager.CMS.Get(sound).POOL();
                    // soundObject.transform.position = Unit.transform.position;
                    UpdateRandomLists();
                };
            }
        }

        void UpdateRandomLists()
        {
            if (_randomSteps.Count == 0) _randomSteps = new Queue<HashCMS>(_template.stepSounds.Shuffle());
            if (_randomJumps.Count == 0) _randomJumps = new Queue<HashCMS>(_template.jumpSounds.Shuffle());
        }
        
        float distance;
        public void OnUpdate(float deltaTime)
        {
            float mgn = _movement.DeltaPositionMagnitude;
            if (mgn <= float.Epsilon * 2) return;
            distance += deltaTime;
            if (distance > _stepDistance)
            {
                // var sound = _randomSteps.Dequeue();
                // var soundObject = CMS_Manager.CMS.Get(sound).POOL();
                // soundObject.transform.position = Unit.transform.position;
                // distance = 0;
                UpdateRandomLists();
            }
        }*/
    }
    
    [Serializable]
    public struct SoundBehaviour : ModuleTemplate<SoundBehaviourModule>
    {
        public float stepDistance;
    }
}