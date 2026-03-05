using System;

namespace Sackrany.Actor.UnitMono
{
    public readonly struct TeamInfo : IEquatable<TeamInfo>
    {
        public readonly bool None;
        public readonly int TeamId;
        
        public TeamInfo(UnitTag tag, bool hasTeam)
        {
            None = !hasTeam;
            if (None) {
                TeamId = -1;
                return;
            }
            int hash = 0;
            foreach (var id in tag.GetIds())
                hash ^= id * 1000003;
            TeamId = hash;
        }
        
        public bool Equals(TeamInfo other) => TeamId == other.TeamId;
        public override bool Equals(object obj) => obj is TeamInfo other && Equals(other);
        public static bool operator ==(TeamInfo left, TeamInfo right) => left.Equals(right);
        public static bool operator !=(TeamInfo left, TeamInfo right) => !(left == right);
        public override int GetHashCode() => TeamId;
        
        public static TeamInfo Default => new TeamInfo(null, false);
    }
}