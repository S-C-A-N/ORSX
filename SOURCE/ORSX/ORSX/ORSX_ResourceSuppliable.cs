using System;

namespace ORSX
{
    public interface ORSX_ResourceSuppliable
    {
        void receiveFNResource(double power_supplied, String resourcename);
        float consumeFNResource(double power_to_consume, String resourcename);
        float consumeFNResource(float power_to_consume, String resourcename);
        string getResourceManagerDisplayName();
        int getPowerPriority();
    }
}