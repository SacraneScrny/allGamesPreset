using System;

namespace Sackrany.CustomRandom.Entities
{
    [Serializable]
    public struct RandomItem
    {
        public int id;
        public float chance;

        public RandomItem(int id, float chance)
        {
            this.id = id;
            this.chance = chance;
        }
    }
}