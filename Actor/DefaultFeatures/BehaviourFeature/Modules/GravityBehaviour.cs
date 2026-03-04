using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Static;
using Sackrany.Variables.ExpandedVariable.Entities;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules
{
    /// <summary>
    /// LONGER SEARCH
    /// </summary>
    public abstract class GravityBehaviourModule : Module
    {
        [Template] protected GravityBehaviour _template;
        
        public ExpandedFloat Gravity;
        
        protected override void OnAwake()
        {
            Gravity = _template.gravity;
        }
        protected override void OnReset()
        {
            Gravity.Clear();
        }
    }

    public class CharacterController_GravityBehaviour : GravityBehaviourModule, IUpdateModule
    {
        [Dependency] CharacterController _characterController;
        
        public void OnUpdate(float deltaTime)
        {
            if (_characterController.isGrounded) return;
            _characterController.Move(Vector3.up * Gravity * deltaTime);
        }
    }
    public class Rigidbody_GravityBehaviour : GravityBehaviourModule, IFixedUpdateModule
    {
        [Dependency] Rigidbody _rigidbody;
        
        public void OnFixedUpdate(float deltaTime)
        {
            _rigidbody.AddForce(Vector3.up * Gravity * deltaTime);
        }
    }
    
    [Serializable]
    public struct GravityBehaviour : ModuleTemplate
    {
        public ControllerType controllerType;
        public float gravity;
        
        public int GetId() => controllerType switch
        {
            ControllerType.CharacterController => ModuleRegistry.GetId<CharacterController_GravityBehaviour>(),
            _ => ModuleRegistry.GetId<Rigidbody_GravityBehaviour>()
        };
        public Module GetInstance() => controllerType switch
        {
            ControllerType.CharacterController => new CharacterController_GravityBehaviour(),
            _ => new Rigidbody_GravityBehaviour()
        };

        public enum ControllerType
        {
            CharacterController,
            Rigidbody
        }
    }
}