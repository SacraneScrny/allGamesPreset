using System;

using UnityEngine.Scripting;

namespace Sackrany.Actor.Traits.Tags
{
    [Serializable] [Preserve] public class Enemy : Tag<Enemy> { }
    [Serializable] [Preserve] public class Player : Tag<Player> { }
    [Serializable] [Preserve] public class Undead : Tag<Undead> { }
    [Serializable] [Preserve] public class Flying : Tag<Flying> { }
}