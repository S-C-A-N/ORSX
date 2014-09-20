namespace ORSX
{
    public class ORSX_ResourceScanner : PartModule
    {
        [KSPField(isPersistant = false, guiActive = true, guiName = "Abundance")] public string Ab;

        protected double abundance = 0;
        [KSPField(isPersistant = false)] public bool mapViewAvailable = false;
        [KSPField(isPersistant = false)] public string resourceName = "";

        [KSPEvent(guiActive = true, guiName = "Display Hotspots", active = true)]
        public void DisplayResource()
        {
            ORSX_PlanetaryResourceMapData.setDisplayedResource(resourceName);
        }

        [KSPEvent(guiActive = true, guiName = "Hide Hotspots", active = true)]
        public void HideResource()
        {
            ORSX_PlanetaryResourceMapData.setDisplayedResource("");
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor)
            {
                return;
            }
            part.force_activate();
        }

        public override void OnUpdate()
        {
            Events["DisplayResource"].active =
                Events["DisplayResource"].guiActive =
                    !ORSX_PlanetaryResourceMapData.resourceIsDisplayed(resourceName) && mapViewAvailable;
            Events["DisplayResource"].guiName = "Display " + resourceName + " hotspots";
            Events["HideResource"].active =
                Events["HideResource"].guiActive =
                    ORSX_PlanetaryResourceMapData.resourceIsDisplayed(resourceName) && mapViewAvailable;
            Events["HideResource"].guiName = "Hide " + resourceName + " hotspots";
            Fields["Ab"].guiName = resourceName + " abundance";
            if (abundance > 0.001)
            {
                Ab = (abundance*100.0).ToString("0.00") + "%";
            }
            else
            {
                Ab = (abundance*1000000.0).ToString("0.0") + "ppm";
            }
            ORSX_PlanetaryResourceMapData.updatePlanetaryResourceMap();
        }

        public override void OnFixedUpdate()
        {
            CelestialBody body = vessel.mainBody;
            ORSX_PlanetaryResourcePixel res_pixel =
                ORSX_PlanetaryResourceMapData.getResourceAvailability(vessel.mainBody.flightGlobalsIndex, resourceName,
                    body.GetLatitude(vessel.transform.position), body.GetLongitude(vessel.transform.position));
            abundance = res_pixel.getAmount();
        }
    }
}