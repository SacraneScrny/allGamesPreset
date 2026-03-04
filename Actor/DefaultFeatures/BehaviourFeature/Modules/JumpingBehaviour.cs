using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Actor.Static;
using Sackrany.Variables.ExpandedVariable.Entities;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules
{
    public abstract class JumpingBehaviourModule : Module
    {
        [Template] protected JumpingBehaviour _template;
        [Dependency] protected GravityBehaviourModule _gravity;
        protected float _currentJumpVelocity;
        public float CurrentJumpVelocity => _currentJumpVelocity;
        public ExpandedFloat JumpForce;
        
        protected override void OnStart()
        {
            JumpForce = _template.jumpForce;
            JumpForce.Add_Multiply(() => _gravity.Gravity);
        }
        protected override void OnReset()
        {
            OnJump = null;
            JumpForce.Clear();
        }

        protected float jumpDeadTime;
        public void Jump()
        {
            if (jumpDeadTime > 0) return;
            if (!CanJump()) return;
            jumpDeadTime = 0.2f;
            _currentJumpVelocity = Mathf.Sqrt(-2f * JumpForce);
            OnJump?.Invoke();
        }
        
        protected virtual bool CanJump() => true;
        
        public event System.Action OnJump;
    }

    public class CharacterController_JumpingBehaviour : JumpingBehaviourModule, IUpdateModule
    {
        [Dependency] CharacterController _characterController;
        
        public void OnUpdate(float deltaTime)
        {
            jumpDeadTime -= deltaTime;
            if (_currentJumpVelocity <= 0)
            {
                _currentJumpVelocity = 0;
                return;
            }
            _characterController.Move(Vector3.up * _currentJumpVelocity * deltaTime);
            _currentJumpVelocity += _gravity.Gravity * deltaTime;
        }
        protected override bool CanJump() => !_characterController.isGrounded;
    }
    public class Rigidbody_JumpingBehaviour : JumpingBehaviourModule, IUpdateModule
    {
        [Dependency] Rigidbody _rigidbody;
        
        public void OnUpdate(float deltaTime)
        {
            jumpDeadTime -= deltaTime;
            if (_currentJumpVelocity <= 0)
            {
                _currentJumpVelocity = 0;
                return;
            }
            _rigidbody.AddForce(Vector3.up * _currentJumpVelocity * deltaTime, ForceMode.Impulse);
            _currentJumpVelocity += _gravity.Gravity * deltaTime;
        }
        protected override bool CanJump() => true;
    }
    
    [Serializable]
    public struct JumpingBehaviour : ModuleTemplate
    {
        public ControllerType controllerType;
        public float jumpForce;
        public int GetId() => controllerType switch
        {
            ControllerType.CharacterController => ModuleRegistry.GetId<CharacterController_JumpingBehaviour>(),
            _ => ModuleRegistry.GetId<Rigidbody_JumpingBehaviour>(),
        };
        public Module GetInstance() => controllerType switch
        {
            ControllerType.CharacterController => new CharacterController_JumpingBehaviour(),
            _ => new Rigidbody_JumpingBehaviour(),
        };
        
        public enum ControllerType
        {
            CharacterController,
            Rigidbody,
        }
    }
}