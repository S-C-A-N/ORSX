namespace ORSX
{
    public class ORSX_ModuleResourceExtraction : ORSX_ResourceSuppliableModule
    {
        //Persistent True
        [KSPField(isPersistant = true)] public bool IsEnabled = false;
        private double electrical_power_ratio;
        [KSPField(isPersistant = false)] public string extractActionName;

        // Persistent False
        [KSPField(isPersistant = false)] public float extractionRateLandPerTon;
        [KSPField(isPersistant = false)] public float extractionRateOceanPerTon;
        private double extraction_rate_d;
        [KSPField(isPersistant = false)] public float powerConsumptionLand;
        [KSPField(isPersistant = false)] public float powerConsumptionOcean;
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
            if (HighLogic.LoadedSceneIsFlight)
            {
                double resource_abundance = 0;
                bool resource_available = false;
                if (vessel.Landed)
                {
                    ORSX_PlanetaryResourcePixel current_resource_abundance_pixel =
                        ORSX_PlanetaryResourceMapData.getResourceAvailabilityByRealResourceName(
                            vessel.mainBody.flightGlobalsIndex, resourceName, vessel.latitude, vessel.longitude);
                    resource_abundance = current_resource_abundance_pixel.getAmount();
                }
                else if (vessel.Splashed)
                {
                    resource_abundance =
                        ORSX_OceanicResourceHandler.getOceanicResourceContent(vessel.mainBody.flightGlobalsIndex,
                            resourceName);
                }
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
                    double power_required = 0;
                    if (vessel.Landed)
                    {
                        power_required = powerConsumptionLand;
                    }
                    else if (vessel.Splashed)
                    {
                        power_required = powerConsumptionOcean;
                    }
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
        }

        public override void OnFixedUpdate()
        {
            if (IsEnabled)
            {
                double power_requirements = 0;
                double extraction_time = 0;
                if (vessel.Landed)
                {
                    power_requirements = powerConsumptionLand;
                    extraction_time = extractionRateLandPerTon;
                }
                else if (vessel.Splashed)
                {
                    power_requirements = powerConsumptionOcean;
                    extraction_time = extractionRateOceanPerTon;
                }
                else
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
                double resource_abundance = 0;
                if (vessel.Landed)
                {
                    ORSX_PlanetaryResourcePixel current_resource_abundance_pixel =
                        ORSX_PlanetaryResourceMapData.getResourceAvailabilityByRealResourceName(
                            vessel.mainBody.flightGlobalsIndex, resourceName, vessel.latitude, vessel.longitude);
                    resource_abundance = current_resource_abundance_pixel.getAmount();
                }
                else if (vessel.Splashed)
                {
                    resource_abundance =
                        ORSX_OceanicResourceHandler.getOceanicResourceContent(vessel.mainBody.flightGlobalsIndex,
                            resourceName);
                }
                double extraction_rate = resource_abundance*extraction_time*electrical_power_ratio;
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

        public override string getResourceManagerDisplayName()
        {
            return unitName;
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

        public override string GetInfo()
        {
            string infostr = "Resource Produced: " + resourceName + "\n";
            if (powerConsumptionLand >= 0)
            {
                infostr += "Power Consumption (Land): " + powerConsumptionLand + " MW\n";
            }
            if (powerConsumptionOcean >= 0)
            {
                infostr += "Power Consumption (Ocean): " + powerConsumptionOcean + " MW";
            }
            return infostr;
        }
    }
}