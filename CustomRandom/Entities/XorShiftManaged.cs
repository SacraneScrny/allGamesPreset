using Sackrany.CustomRandom.Interfaces;

using UnityEngine;

namespace Sackrany.CustomRandom.Entities
{
    public class XorShiftManaged : IRandom
    {
        private uint _state;
        public uint GetState => _state;
        private CurrentNext currentNext;

        public void Init(uint seed = 123)
        {
            currentNext = rejectionSampling_Next;

            if (seed == 0) seed = 2463534242;
            _state = seed;
        }

        public uint Next()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }
        public int Next(int max)
        {
            if (max == 0)
                return 0;
            return currentNext(max);
        }
        public int Next(int min, int max)
        {
            return min + Next(max - min);
        }

        public float NextFloat()
        {
            return NextFloat(0, 1);
        }
        public float NextFloat(float max)
        {
            return NextFloat(0, max);
        }
        public float NextFloat(float min, float max)
        {
            return min + (float)Next() / uint.MaxValue * (max - min);
        }

        public Vector2 NextVector2(bool mayNegative = false)
        {            
            float x = 1;
            float y = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector2(NextFloat() * x, NextFloat() * y);
        }
        public Vector2 NextVector2(float max, bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector2(NextFloat(max) * x, NextFloat(max) * y);
        }
        public Vector2 NextVector2(float min, float max, bool mayNegative = false)
        {            
            float x = 1;
            float y = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector2(NextFloat(min, max) * x, NextFloat(min, max) * y);
        }
        
        public Vector3 NextVector3(bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            float z = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
                z = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector3(NextFloat() * x, NextFloat() * y, NextFloat() * z);
        }
        public Vector3 NextVector3(float max, bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            float z = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
                z = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector3(NextFloat(max) * x, NextFloat(max) * y, NextFloat(max) * z);
        }
        public Vector3 NextVector3(float min, float max, bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            float z = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
                z = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector3(NextFloat(min, max) * x, NextFloat(min, max) * y, NextFloat(min, max) * z);
        }

        public void NextArray(ref int[] array, int min, int max)
        {
            if (array == null)
                return;
            for (var i = 0; i < array.Length; i++)
                array[i] = Next(min, max);
        }
        public void NextArrayFloat(ref float[] array, float min, float max)
        {
            if (array == null)
                return;
            for (var i = 0; i < array.Length; i++)
                array[i] = NextFloat(min, max);
        }

        private int rejectionSampling_Next(int max)
        {
            //в душе не ебу надо тут +1 или нет, TODO
            var threshold = ((ulong)uint.MaxValue + 1) / (uint)max * (uint)max;

            uint result;
            do
            {
                result = Next();
            } while ((ulong)result >= threshold);

            return (int)(result % (uint)max);
        }
        private int default_Next(int max)
        {
            return (int)(Next() % max);
        }

        private delegate int CurrentNext(int max);
        
        //0 1 2
        //0 1 2
        //0 1 2
    }
    
    public struct XorShiftUnmanaged : IRandom
    {
        private uint _state;
        public uint GetState => _state;

        public void Init(uint seed = 123)
        {
            if (seed == 0) seed = 2463534242;
            _state = seed;
        }
        public XorShiftUnmanaged(uint seed = 123)
        {
            if (seed == 0) seed = 2463534242;
            _state = seed;
        }
        public XorShiftUnmanaged(IRandom random)
        {
            _state = random.GetState;
        }

        public uint Next()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }
        public int Next(int max)
        {
            if (max == 0)
                return 0;
            return rejectionSampling_Next(max);
        }
        public int Next(int min, int max)
        {
            return min + Next(max - min);
        }

        public float NextFloat()
        {
            return NextFloat(0, 1);
        }
        public float NextFloat(float max)
        {
            return NextFloat(0, max);
        }
        public float NextFloat(float min, float max)
        {
            return min + (float)Next() / uint.MaxValue * (max - min);
        }

        public void NextArray(ref int[] array, int min, int max)
        {
            if (array == null)
                return;
            for (var i = 0; i < array.Length; i++)
                array[i] = Next(min, max);
        }
        public void NextArrayFloat(ref float[] array, float min, float max)
        {
            if (array == null)
                return;
            for (var i = 0; i < array.Length; i++)
                array[i] = NextFloat(min, max);
        }
        
        public Vector2 NextVector2(bool mayNegative = false)
        {            
            float x = 1;
            float y = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector2(NextFloat() * x, NextFloat() * y);
        }
        public Vector2 NextVector2(float max, bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector2(NextFloat(max) * x, NextFloat(max) * y);
        }
        public Vector2 NextVector2(float min, float max, bool mayNegative = false)
        {            
            float x = 1;
            float y = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector2(NextFloat(min, max) * x, NextFloat(min, max) * y);
        }
        
        public Vector3 NextVector3(bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            float z = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
                z = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector3(NextFloat() * x, NextFloat() * y, NextFloat() * z);
        }
        public Vector3 NextVector3(float max, bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            float z = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
                z = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector3(NextFloat(max) * x, NextFloat(max) * y, NextFloat(max) * z);
        }
        public Vector3 NextVector3(float min, float max, bool mayNegative = false)
        {
            float x = 1;
            float y = 1;
            float z = 1;
            if (mayNegative)
            {
                x = NextFloat() > 0.5f ? 1 : -1;
                y = NextFloat() > 0.5f ? -1 : 1;
                z = NextFloat() > 0.5f ? -1 : 1;
            }
            return new Vector3(NextFloat(min, max) * x, NextFloat(min, max) * y, NextFloat(min, max) * z);
        }

        private int rejectionSampling_Next(int max)
        {
            //в душе не ебу надо тут +1 или нет, TODO
            var threshold = ((ulong)uint.MaxValue + 1) / (uint)max * (uint)max;

            uint result;
            do
            {
                result = Next();
            } while ((ulong)result >= threshold);

            return (int)(result % (uint)max);
        }
    }
}