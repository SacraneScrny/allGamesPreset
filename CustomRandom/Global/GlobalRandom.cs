using System;

using Sackrany.CustomRandom.Entities;
using Sackrany.CustomRandom.Interfaces;

namespace Sackrany.CustomRandom.Global
{
    public static class GlobalRandom
    {
        private static IRandom _currentRandom;

        public static IRandom Current
        {
            get
            {
                if (_currentRandom == null)
                {
                    _currentRandom = new XorShiftManaged();
                    _currentRandom.Init((uint)DateTime.Now.Ticks);
                }

                return _currentRandom;
            }
        }
    }
}