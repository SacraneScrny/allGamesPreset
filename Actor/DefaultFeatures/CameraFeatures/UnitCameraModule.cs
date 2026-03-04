using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;
using Sackrany.Variables.ExpandedVariable.Entities;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.CameraFeatures
{
    public class UnitCameraModule : Module, ILateUpdateModule
    {
        [Dependency] public Camera Camera;
        [Template] UnitCamera _template;
        
        public ExpandedFloat CameraOrthographic { get; private set; }
        public ExpandedFloat CameraFov { get; private set; }
        public ExpandedVector3 CameraPosition { get; private set; }
        public ExpandedQuaternion CameraRotation { get; private set; }
        
        protected override void OnAwake()
        {
            CameraOrthographic = Camera.orthographicSize;
            CameraFov = Camera.fieldOfView;
            CameraPosition = Camera.transform.position;
            CameraRotation = Camera.transform.rotation;
        }
        
        public void OnLateUpdate(float deltaTime)
        {
            var targetPos = CameraPosition;
            if (_template.pixelPerfect)
                targetPos = SnapToPixel(targetPos);
            Camera.transform.position = targetPos;
            
            Camera.transform.rotation = CameraRotation;
            Camera.fieldOfView = math.clamp(CameraFov, 40, 130);
            Camera.orthographicSize = math.clamp(CameraOrthographic, 0.1f, 100);
        }
        
        Vector3 SnapToPixel(Vector3 worldPos)
        {
            float unitsPerPixel = (Camera.orthographicSize * 2f) / Screen.height;

            worldPos.x = Mathf.Round(worldPos.x / unitsPerPixel) * unitsPerPixel;
            worldPos.y = Mathf.Round(worldPos.y / unitsPerPixel) * unitsPerPixel;

            return worldPos;
        }
    }

    [Serializable]
    public struct UnitCamera : ModuleTemplate<UnitCameraModule>
    {
        public bool pixelPerfect;
    }
}