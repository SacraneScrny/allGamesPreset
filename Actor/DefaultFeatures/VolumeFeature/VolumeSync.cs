using System;

using Sackrany.Actor.Modules;
using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Modules.Modules;

namespace Sackrany.Actor.DefaultFeatures.VolumeFeature
{
    public class VolumeSyncModule : Module, IUpdateModule
    {
        [Dependency] VolumeModule _volumeModule;
        public void OnUpdate(float deltaTime)
        {
            _volumeModule.SyncVolume();
        }
    }
    public class VolumeFixedSyncModule : Module, IFixedUpdateModule
    {
        [Dependency] VolumeModule _volumeModule;
        public void OnFixedUpdate(float deltaTime)
        {
            _volumeModule.SyncVolume();
        }
    }
    public class VolumeLateSyncModule : Module, ILateUpdateModule
    {
        [Dependency] VolumeModule _volumeModule;
        public void OnLateUpdate(float deltaTime)
        {
            _volumeModule.SyncVolume();
        }
    }

    [Serializable]
    public struct VolumeSync : ModuleTemplate<VolumeSyncModule>
    {
        public SyncType type;
        public enum SyncType
        {
            Default,
            Fixed,
            Late
        }
        
        public Type GetModuleType() => type switch
        {
            SyncType.Fixed => typeof(VolumeFixedSyncModule),
            SyncType.Late => typeof(VolumeLateSyncModule),
            _ => typeof(VolumeSyncModule)
        };
        public Module GetModuleInstance() => type switch
        {
            SyncType.Fixed => new VolumeFixedSyncModule(),
            SyncType.Late => new VolumeLateSyncModule(),
            _ => new VolumeSyncModule()
        };
    }
}