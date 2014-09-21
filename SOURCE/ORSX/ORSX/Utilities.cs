using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ORSX
{
    class Utilities
    {
        const int SECONDS_PER_MINUTE = 60;
        const int SECONDS_PER_HOUR = 3600;
        const int SECONDS_PER_DAY = 6 * SECONDS_PER_HOUR;

        public static double GetAltitude(Vessel v)
        {
            v.GetHeightFromTerrain();
            double alt = v.heightFromTerrain;
            if (alt < 0)
            {
                alt = v.mainBody.GetAltitude(v.CoM);
            }
            return alt;
        }

        public static int MaxDeltaTime
        {
            get { return SECONDS_PER_DAY; }
        }
        public static int ElectricityMaxDeltaTime
        {
            get { return 1; }
        }


        public static double GetValue(ConfigNode config, string name, double currentValue)
        {
            double newValue;
            if (config.HasValue(name) && double.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }


    }
}
