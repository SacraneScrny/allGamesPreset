using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.CameraFeatures
{
    public class CameraFollowModule : Module, IUpdateModule
    {
        [Template] CameraFollow _template;
        [Dependency] UnitCameraModule _unitCameraModule;
        
        protected override void OnStart()
        {
            _unitCameraModule.CameraPosition.Add_PostAdditional(() => _actualPos);
        }

        Vector3 _lastPos;
        Vector3 _actualPos;
        Vector3 Offset()
        {
            if (_template.target == null)
                return _lastPos;

            Vector3 mask = (Vector3)_template.mask;

            Vector3 maskedTarget = Vector3.Scale(_template.target.position, mask);
            Vector3 maskedCamera = Vector3.Scale(_lastPos, mask);

            Vector3 delta = maskedTarget - maskedCamera;
            float distance = delta.magnitude;

            if (distance > _template.deadZoneRadius)
            {
                maskedCamera = maskedTarget - delta.normalized * _template.deadZoneRadius;

                _lastPos = new Vector3(
                    mask.x != 0 ? maskedCamera.x / mask.x : _lastPos.x,
                    mask.y != 0 ? maskedCamera.y / mask.y : _lastPos.y,
                    mask.z != 0 ? maskedCamera.z / mask.z : _lastPos.z
                );
            }

            return _lastPos;
        }
        
        public void OnUpdate(float deltaTime)
        {
            _actualPos = Vector3.Lerp(_actualPos, Offset(), deltaTime * 6f);
        }
    }

    [Serializable]
    public struct CameraFollow : ModuleTemplate<CameraFollowModule>
    {
        public Transform target;
        public Vector3Int mask;
        public float deadZoneRadius;
    }
}