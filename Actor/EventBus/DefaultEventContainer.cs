using UnityEngine.Scripting;

namespace Sackrany.Actor.EventBus
{
    [Preserve] public class OnDamage : AEvent<OnDamage> { }
    [Preserve] public class OnDamagePostProcess : AEvent<OnDamagePostProcess> { }
    [Preserve] public class OnHeal : AEvent<OnHeal> { }
    [Preserve] public class OnDied : AEvent<OnDied> { }
}