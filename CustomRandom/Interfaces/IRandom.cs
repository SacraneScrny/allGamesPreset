using UnityEngine;

namespace Sackrany.CustomRandom.Interfaces
{
    public interface IRandom
    {
        public uint GetState { get; }
        public void Init(uint seed);

        public uint Next();
        public int Next(int max);
        public int Next(int min, int max);
        public float Next(Vector2Int range) => Next(range.x, range.y);

        public float NextFloat();
        public float NextFloat(float max);
        public float NextFloat(float min, float max);
        public float NextFloat(Vector2 range) => NextFloat(range.x, range.y);
        
        public Vector2 NextVector2(bool mayNegative = false);
        public Vector2 NextVector2(float max, bool mayNegative = false);
        public Vector2 NextVector2(float min, float max, bool mayNegative = false);

        public Vector3 NextVector3(bool mayNegative = false);
        public Vector3 NextVector3(float max, bool mayNegative = false);
        public Vector3 NextVector3(float min, float max, bool mayNegative = false);
        
        public void NextArray(ref int[] array, int min, int max);
        public void NextArrayFloat(ref float[] array, float min, float max);
    }
}