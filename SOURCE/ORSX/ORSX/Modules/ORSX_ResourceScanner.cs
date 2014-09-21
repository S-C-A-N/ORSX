using System;
using UnityEngine;

namespace ORSX //GOOD
{
    public class ORSX_ResourceScanner : PartModule, IAnimatedModule
    {
        [KSPField(isPersistant = false, guiActive = true, guiName = "Abundance")] 
        public string Ab;
        protected double abundance = 0;
        
        [KSPField(isPersistant = false)] 
        public bool mapViewAvailable = false;

      
        [KSPField(isPersistant = false)] 
        public string resourceName = "";

        [KSPField]
        public bool isActive = false;

        [KSPField]
        public float maxAbundanceAltitude = 500000f;

        [KSPEvent(guiActive = true, guiName = "Display Hotspots", active = true)]
        public void DisplayResource()
        {
            ORSX_PlanetaryResourceMapData.setDisplayedResource(resourceName);
            isActive = true;
        }

        [KSPEvent(guiActive = true, guiName = "Hide Hotspots", active = true)]
        public void HideResource()
        {
            ORSX_PlanetaryResourceMapData.setDisplayedResource("");
            isActive = false;
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor)
            {
                return;
            }
            Setup();
        }

        private void Setup()
        {
            Events["DisplayResource"].guiName = "Display " + resourceName + " hotspots";
            Events["HideResource"].guiName = "Hide " + resourceName + " hotspots";
            Fields["Ab"].guiName = resourceName + " abundance";
            ToggleEvent("DisplayResource", false);
            ToggleEvent("HideResource", false);
            part.force_activate();
        }

        public override void OnUpdate()
        {
            if (mapViewAvailable)
            {
                CheckHotSpotDisplay();
            }
            CheckAbundanceDisplay();
        }

        private void CheckAbundanceDisplay()
        {
            if (Utilities.GetAltitude(vessel) > maxAbundanceAltitude)
            {
                Ab = "Too high";
            }
            else
            {
                DisplayAbundance();
            }
        }

        private void DisplayAbundance()
        {
            if (abundance > 0.001)
            {
                Ab = (abundance * 100.0).ToString("0.00") + "%";
            }
            else
            {
                Ab = (abundance * 100.0).ToString("0.0000") + "%";
            }
        }

        private void CheckHotSpotDisplay()
        {
            if (ORSX_PlanetaryResourceMapData.resourceIsDisplayed(resourceName))
            {
                ToggleEvent("DisplayResource", false);
                ToggleEvent("HideResource", true);
            }
            else
            {
                ToggleEvent("DisplayResource", true);
                ToggleEvent("HideResource", false);
            }
            ORSX_PlanetaryResourceMapData.updatePlanetaryResourceMap();
        }

        private void ToggleEvent(string name, bool status)
        {
            Events[name].active = status;
            Events[name].guiActive = status;
        }


        public override void OnFixedUpdate()
        {
            CelestialBody body = vessel.mainBody;
            ORSX_PlanetaryResourcePixel res_pixel =
                ORSX_PlanetaryResourceMapData.getResourceAvailability(vessel.mainBody.flightGlobalsIndex, resourceName,
                    body.GetLatitude(vessel.transform.position), body.GetLongitude(vessel.transform.position));
            abundance = res_pixel.getAmount();
        }

        public void EnableModule()
        {
            isEnabled = true;
        }

        public void DisableModule()
        {
            isEnabled = false;
        }

        public bool ModuleIsActive()
        {
            return isActive;
        }
    }
}