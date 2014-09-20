﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNoise.Unity.Operator;
using UnityEngine;


namespace ORSX
{
    public class ORSX_SurfaceScanner : PartModule
    {
        [KSPField(isPersistant = false)] public string resourceName = "";

        [KSPField(isPersistant = false, guiActive = true, guiName = "Abundance")] public string Ab;

        [KSPField] public float maxAltitude = 500f;

        [KSPField] public bool isActive = false;

        protected double abundance = 0;
        private ORSX_PlanetaryResourceInfo resourceInfo = null;

        public override void OnStart(PartModule.StartState state)
        {
            if (state == StartState.Editor)
            {
                return;
            }
            this.part.force_activate();
        }

        public override void OnUpdate()
        {
            if (!isActive)
            {
                Ab = "off";
                return;
            }

            ORSX_PlanetaryResourceMapData.updatePlanetaryResourceMap();
            if (resourceInfo == null)
                if (ORSX_PlanetaryResourceMapData.getPlaneteryResourceMapData.ContainsKey(resourceName))
                    resourceInfo = ORSX_PlanetaryResourceMapData.getPlaneteryResourceMapData[resourceName];
            vessel.GetHeightFromTerrain();
            if (vessel.heightFromTerrain > maxAltitude)
            {
                Ab = "Too High";
                return;
            }


            Fields["Ab"].guiName = resourceName + " abundance";
            if (resourceInfo != null)
            {
                if (resourceInfo.getResourceScale() == 1)
                {
                    Ab = (abundance*100.0).ToString("0.00") + "%";
                }
                else
                {
                    Ab = (abundance*1000000.0).ToString("0.0") + "ppm";
                }
            }
            else
                Ab = "Broken:(";

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
