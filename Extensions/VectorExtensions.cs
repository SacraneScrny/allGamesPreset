using System.Runtime.CompilerServices;

using UnityEngine;

namespace Sackrany.Extensions
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public static class VectorExtensions
    {
        // =========================
        // HELPERS
        // =========================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool AlmostZero(float v, float eps) => Mathf.Abs(v) <= eps;

        // =========================
        // VECTOR2
        // =========================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 With(this Vector2 v, float? x = null, float? y = null) => new Vector2(x ?? v.x, y ?? v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Add(this Vector2 v, float? x = null, float? y = null)
            => new Vector2(v.x + (x ?? 0f), v.y + (y ?? 0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(this Vector2 v, float? x = null, float? y = null)
            => new Vector2(v.x * (x ?? 1f), v.y * (y ?? 1f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Swap(this Vector2 v) => new Vector2(v.y, v.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Abs(this Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Sign(this Vector2 v)
            => new Vector2(v.x > 0f ? 1f : (v.x < 0f ? -1f : 0f), v.y > 0f ? 1f : (v.y < 0f ? -1f : 0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
            => new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp01(this Vector2 v) => new Vector2(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Round(this Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(this Vector2 v) => new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceil(this Vector2 v) => new Vector2(Mathf.Ceil(v.x), Mathf.Ceil(v.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Snap(this Vector2 v, float grid)
            => new Vector2(Mathf.Round(v.x / grid) * grid, Mathf.Round(v.y / grid) * grid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Rotate90CW(this Vector2 v) => new Vector2(v.y, -v.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Rotate90CCW(this Vector2 v) => new Vector2(-v.y, v.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MaxComponent(this Vector2 v) => Mathf.Max(v.x, v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MinComponent(this Vector2 v) => Mathf.Min(v.x, v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Axis DominantAxis(this Vector2 v) => v.x >= v.y ? Axis.X : Axis.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this Vector2 v) => v.x == 0f && v.y == 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this Vector2 v, float eps = 1e-6f)
            => AlmostZero(v.x, eps) && AlmostZero(v.y, eps);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Manhattan(this Vector2 a, Vector2 b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Chebyshev(this Vector2 a, Vector2 b) => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 To(this Vector2 from, Vector2 to, float t) => Vector2.Lerp(from, to, t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Towards(this Vector2 from, Vector2 to, float maxDistanceDelta)
            => Vector2.MoveTowards(from, to, maxDistanceDelta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Shuffle(this Vector2 v) => new Vector2(v.y, v.x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.x, v.y);
        // =========================
        // VECTOR2 INT
        // =========================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int With(this Vector2Int v, int? x = null, int? y = null)
            => new Vector2Int(x ?? v.x, y ?? v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int Add(this Vector2Int v, int? x = null, int? y = null)
            => new Vector2Int(v.x + (x ?? 0), v.y + (y ?? 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int Multiply(this Vector2Int v, int? x = null, int? y = null)
            => new Vector2Int(v.x * (x ?? 1), v.y * (y ?? 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int Swap(this Vector2Int v) => new Vector2Int(v.y, v.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int Clamp(this Vector2Int v, Vector2Int min, Vector2Int max)
            => new Vector2Int(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Manhattan(this Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Chebyshev(this Vector2Int a, Vector2Int b)
            => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        // =========================
        // VECTOR3
        // =========================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
            => new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Add(this Vector3 v, float? x = null, float? y = null, float? z = null)
            => new Vector3(v.x + (x ?? 0f), v.y + (y ?? 0f), v.z + (z ?? 0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(this Vector3 v, float? x = null, float? y = null, float? z = null)
            => new Vector3(v.x * (x ?? 1f), v.y * (y ?? 1f), v.z * (z ?? 1f));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(this Vector3 v, Vector3 m) => Multiply(v, m.x, m.y, m.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(this Vector3 v, Vector3Int m) => Multiply(v, m.x, m.y, m.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Swap(this Vector3 v, bool xy = false, bool xz = false, bool yz = false)
        {
            float x = v.x, y = v.y, z = v.z;
            if (xy) (x, y) = (y, x);
            if (xz) (x, z) = (z, x);
            if (yz) (y, z) = (z, y);
            return new Vector3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 FlattenY(this Vector3 v) => new Vector3(v.x, 0f, v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(this Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sign(this Vector3 v)
            => new Vector3(v.x > 0f ? 1f : (v.x < 0f ? -1f : 0f), v.y > 0f ? 1f : (v.y < 0f ? -1f : 0f),
                v.z > 0f ? 1f : (v.z < 0f ? -1f : 0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
            => new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y),
                Mathf.Clamp(v.z, min.z, max.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp01(this Vector3 v)
            => new Vector3(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y), Mathf.Clamp01(v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(this Vector3 v)
            => new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(this Vector3 v)
            => new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceil(this Vector3 v) => new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Snap(this Vector3 v, Vector3 grid)
            => new Vector3(Mathf.Round(v.x / grid.x) * grid.x, Mathf.Round(v.y / grid.y) * grid.y,
                Mathf.Round(v.z / grid.z) * grid.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MaxComponent(this Vector3 v) => Mathf.Max(v.x, Mathf.Max(v.y, v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MinComponent(this Vector3 v) => Mathf.Min(v.x, Mathf.Min(v.y, v.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Axis DominantAxis(this Vector3 v)
        {
            float ax = Mathf.Abs(v.x), ay = Mathf.Abs(v.y), az = Mathf.Abs(v.z);
            if (ax >= ay && ax >= az) return Axis.X;
            if (ay >= ax && ay >= az) return Axis.Y;
            return Axis.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this Vector3 v) => v.x == 0f && v.y == 0f && v.z == 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this Vector3 v, float eps = 1e-6f)
            => AlmostZero(v.x, eps) && AlmostZero(v.y, eps) && AlmostZero(v.z, eps);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Manhattan(this Vector3 a, Vector3 b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Chebyshev(this Vector3 a, Vector3 b)
            => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Max(Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 To(this Vector3 from, Vector3 to, float t) => Vector3.Lerp(from, to, t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Towards(this Vector3 from, Vector3 to, float maxDistanceDelta)
            => Vector3.MoveTowards(from, to, maxDistanceDelta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnPlaneFast(this Vector3 v, Vector3 planeNormal)
        {
            // v - n * (v·n)
            float d = Vector3.Dot(v, planeNormal);
            return v - planeNormal * d;
        }

        /// <summary>
        /// Shuffle the components in a deterministic rotate manner: (x,y,z) -> (y,z,x)
        /// If you want random permutations, use ShuffleRandom
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Shuffle(this Vector3 v) => new Vector3(v.y, v.z, v.x);

        /// <summary>
        /// Random permutation of xyz (uses UnityEngine.Random). Optional seed if you want some determinism:
        /// if seed != null, uses System.Random seeded RNG instead.
        /// </summary>
        public static Vector3 ShuffleRandom(this Vector3 v, int? seed = null)
        {
            if (seed.HasValue)
            {
                var rnd = new System.Random(seed.Value);
                int r = rnd.Next(0, 6);
                return PermuteByIndex(v, r);
            }
            else
            {
                int r = UnityEngine.Random.Range(0, 6);
                return PermuteByIndex(v, r);
            }

            static Vector3 PermuteByIndex(Vector3 vv, int i)
            {
                // 6 permutations
                switch (i)
                {
                    case 0: return new Vector3(vv.x, vv.y, vv.z);
                    case 1: return new Vector3(vv.x, vv.z, vv.y);
                    case 2: return new Vector3(vv.y, vv.x, vv.z);
                    case 3: return new Vector3(vv.y, vv.z, vv.x);
                    case 4: return new Vector3(vv.z, vv.x, vv.y);
                    default: return new Vector3(vv.z, vv.y, vv.x);
                }
            }
        }

        // =========================
        // VECTOR3 INT
        // =========================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int With(this Vector3Int v, int? x = null, int? y = null, int? z = null)
            => new Vector3Int(x ?? v.x, y ?? v.y, z ?? v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Add(this Vector3Int v, int? x = null, int? y = null, int? z = null)
            => new Vector3Int(v.x + (x ?? 0), v.y + (y ?? 0), v.z + (z ?? 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Multiply(this Vector3Int v, int? x = null, int? y = null, int? z = null)
            => new Vector3Int(v.x * (x ?? 1), v.y * (y ?? 1), v.z * (z ?? 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Swap(this Vector3Int v, bool xy = false, bool xz = false, bool yz = false)
        {
            int x = v.x, y = v.y, z = v.z;
            if (xy) (x, y) = (y, x);
            if (xz) (x, z) = (z, x);
            if (yz) (y, z) = (z, y);
            return new Vector3Int(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Clamp(this Vector3Int v, Vector3Int min, Vector3Int max)
            => new Vector3Int(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y),
                Mathf.Clamp(v.z, min.z, max.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Manhattan(this Vector3Int a, Vector3Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Chebyshev(this Vector3Int a, Vector3Int b)
            => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Max(Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z)));

        // =========================
        // VECTOR4-ish helpers (light touch)
        // =========================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToVector4(this Vector3 v, float w = 0f) => new Vector4(v.x, v.y, v.z, w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 FromVector4(this Vector4 v) => new Vector3(v.x, v.y, v.z);
    }
}