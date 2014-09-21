namespace ORSX
{
    //TODO: Merge this with AirScoop
    internal class ORSX_AtmosphericExtractor : ORSX_ResourceSuppliableModule
    {
        //Persistent True
        [KSPField(isPersistant = true)] public bool IsEnabled = false;
        private double electrical_power_ratio;
        [KSPField(isPersistant = false)] public string extractActionName;

        // Persistent False
        [KSPField(isPersistant = false)] public float extractionRatePerTon;
        private double extraction_rate_d;
        [KSPField(isPersistant = false)] public float powerConsumption;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Power")] public string powerStr;
        [KSPField(isPersistant = false)] public bool resourceManaged;
        [KSPField(isPersistant = false)] public string resourceName;
        [KSPField(isPersistant = false, guiActive = true, guiName = "S")] public string resourceRate;
        [KSPField(isPersistant = false)] public string resourceToUse;

        //GUI
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")] public string statusTitle;
        [KSPField(isPersistant = false)] public string stopActionName;
        [KSPField(isPersistant = false)] public string unitName;

        [KSPEvent(guiActive = true, guiName = "Start Action", active = true)]
        public void startResourceExtraction()
        {
            IsEnabled = true;
        }

        [KSPEvent(guiActive = true, guiName = "Stop Action", active = true)]
        public void stopResourceExtration()
        {
            IsEnabled = false;
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor)
            {
                return;
            }
            Events["startResourceExtraction"].guiName = extractActionName;
            Events["stopResourceExtration"].guiName = stopActionName;
            Fields["statusTitle"].guiName = unitName;
            part.force_activate();
        }

        public override void OnUpdate()
        {
            double resource_abundance =
                ORSX_AtmosphericResourceHandler.getAtmosphericResourceContent(vessel.mainBody.flightGlobalsIndex,
                    resourceName);
            bool resource_available = false;
            if (resource_abundance > 0)
            {
                resource_available = true;
            }
            Events["startResourceExtraction"].active = !IsEnabled && resource_available;
            Events["stopResourceExtration"].active = IsEnabled;
            if (IsEnabled)
            {
                Fields["powerStr"].guiActive = true;
                Fields["resourceRate"].guiActive = true;
                statusTitle = "Active";
                double power_required = powerConsumption;
                powerStr = (power_required*electrical_power_ratio).ToString("0.000") + " MW / " +
                           power_required.ToString("0.000") + " MW";
                double resource_density = PartResourceLibrary.Instance.GetDefinition(resourceName).density;
                double resource_rate_per_hour = extraction_rate_d*resource_density*3600;
                resourceRate = formatMassStr(resource_rate_per_hour);
            }
            else
            {
                Fields["powerStr"].guiActive = false;
                Fields["resourceRate"].guiActive = false;
                statusTitle = "Offline";
            }
        }

        public override void OnFixedUpdate()
        {
            if (IsEnabled)
            {
                double power_requirements = powerConsumption;
                double extraction_time = extractionRatePerTon;
                if (vessel.altitude > ORSX_Helper.getMaxAtmosphericAltitude(vessel.mainBody))
                {
                    IsEnabled = false;
                    return;
                }

                double electrical_power_provided = 0;
                if (resourceManaged)
                {
                    electrical_power_provided = consumeFNResource(power_requirements*TimeWarp.fixedDeltaTime,
                        resourceToUse);
                }
                else
                {
                    electrical_power_provided = part.RequestResource(resourceToUse,
                        power_requirements*TimeWarp.fixedDeltaTime);
                }

                if (power_requirements > 0)
                {
                    electrical_power_ratio = electrical_power_provided/TimeWarp.fixedDeltaTime/power_requirements;
                }
                else
                {
                    if (power_requirements < 0)
                    {
                        IsEnabled = false;
                        return;
                    }
                    electrical_power_ratio = 1;
                }
                double resource_abundance =
                    ORSX_AtmosphericResourceHandler.getAtmosphericResourceContent(vessel.mainBody.flightGlobalsIndex,
                        resourceName);
                double extraction_rate = resource_abundance*extraction_time*electrical_power_ratio*
                                         part.vessel.atmDensity;
                if (resource_abundance > 0)
                {
                    double resource_density = PartResourceLibrary.Instance.GetDefinition(resourceName).density;
                    //extraction_rate_d = -part.RequestResource(resourceName, -extraction_rate / resource_density * TimeWarp.fixedDeltaTime) / TimeWarp.fixedDeltaTime;
                    extraction_rate_d =
                        -ORSX_Helper.fixedRequestResource(part, resourceName,
                            -extraction_rate/resource_density*TimeWarp.fixedDeltaTime)/TimeWarp.fixedDeltaTime;
                }
                else
                {
                    IsEnabled = false;
                }
            }
        }

        protected string formatMassStr(double mass)
        {
            if (mass > 1)
            {
                return mass.ToString("0.000") + " mT/hour";
            }
            if (mass > 0.001)
            {
                return (mass*1000).ToString("0.000") + " kg/hour";
            }
            if (mass > 1e-6)
            {
                return (mass*1e6).ToString("0.000") + " g/hour";
            }
            if (mass > 1e-9)
            {
                return (mass*1e9).ToString("0.000") + " mg/hour";
            }
            return (mass*1e12).ToString("0.000") + " ug/hour";
        }
    }
}