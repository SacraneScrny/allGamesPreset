using System;

namespace Sackrany.Actor.Traits.Tags
{
    [Serializable] public class Enemy : Tag<Enemy> { }
    [Serializable] public class Player : Tag<Player> { }
    [Serializable] public class Undead : Tag<Undead> { }
    [Serializable] public class Flying : Tag<Flying> { }
}