﻿using System;
using LibNoise.Unity.Operator;


namespace ORSX
{
    public class ORSX_ModuleAirScoop : ORSX_ResourceSuppliableModule
    {
        [KSPField(isPersistant = true)]
        public bool scoopIsEnabled = false;

        [KSPField(isPersistant = true)]
        public bool isDisabled = true;

        [KSPField(isPersistant = true)]
        public int currentresource = 0;
        [KSPField(isPersistant = false)]
        public float scoopair = 0;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Flow")]
        public string resflow;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Resource")]
        public string currentresourceStr;

        [KSPField(isPersistant = false)] 
        public float ecRequirement = 0.1f;

        protected float resflowf = 0;

        [KSPEvent(guiActive = true, guiName = "Activate Scoop", active = true)]
        public void ActivateScoop()
        {
            scoopIsEnabled = true;
        }


        [KSPEvent(guiActive = true, guiName = "Disable Scoop", active = true)]
        public void DisableScoop()
        {
            scoopIsEnabled = false;
        }


        [KSPEvent(guiActive = true, guiName = "Toggle Resource", active = true)]
        public void ToggleResource()
        {
            currentresource++;


            if (ORSX_AtmosphericResourceHandler.getAtmosphericResourceName(vessel.mainBody.flightGlobalsIndex, currentresource) == null && ORSX_AtmosphericResourceHandler.getAtmosphericResourceContent(vessel.mainBody.flightGlobalsIndex, currentresource) > 0 && currentresource != 0)
            {
                ToggleResource();
            }


            if (currentresource >= ORSX_AtmosphericResourceHandler.getAtmosphericCompositionForBody(vessel.mainBody.flightGlobalsIndex).Count)
            {
                currentresource = 0;
            }
        }


        [KSPAction("Activate Scoop")]
        public void ActivateScoopAction(KSPActionParam param)
        {
            ActivateScoop();
        }


        [KSPAction("Disable Scoop")]
        public void DisableScoopAction(KSPActionParam param)
        {
            DisableScoop();
        }


        [KSPAction("Toggle Scoop")]
        public void ToggleScoopAction(KSPActionParam param)
        {
            if (scoopIsEnabled)
            {
                DisableScoop();
            }
            else
            {
                ActivateScoop();
            }
        }


        [KSPAction("Toggle Resource")]
        public void ToggleResourceAction(KSPActionParam param)
        {
            ToggleResource();
        }


        public override void OnStart(PartModule.StartState state)
        {
            Actions["ToggleResourceAction"].guiName = Events["ToggleResource"].guiName = String.Format("Toggle Resource");


            if (state == StartState.Editor) { return; }
            this.part.force_activate();
        }

        public override void OnUpdate()
        {
            Events["ActivateScoop"].active = (!scoopIsEnabled) && (!isDisabled);
            Events["DisableScoop"].active = (scoopIsEnabled) && (!isDisabled);
            Events["ToggleResource"].active = (scoopIsEnabled) && (!isDisabled);
            Fields["resflow"].guiActive = (scoopIsEnabled) && (!isDisabled);
            Fields["currentresourceStr"].guiActive = (scoopIsEnabled) && (!isDisabled);
            double respcent = ORSX_AtmosphericResourceHandler.getAtmosphericResourceContent(vessel.mainBody.flightGlobalsIndex, currentresource) * 100;
            string resname = ORSX_AtmosphericResourceHandler.getAtmosphericResourceDisplayName(vessel.mainBody.flightGlobalsIndex, currentresource);
            if (resname != null)
            {
                currentresourceStr = resname + "(" + respcent + "%)";
            }
            resflow = resflowf.ToString("0.0000");
        }


        public override void OnFixedUpdate()
        {
            if (scoopIsEnabled)
            {
                string atmospheric_resource_name = ORSX_AtmosphericResourceHandler.getAtmosphericResourceName(vessel.mainBody.flightGlobalsIndex, currentresource);
                if (atmospheric_resource_name != null)
                {
                    //Common
                    double resourcedensity = PartResourceLibrary.Instance.GetDefinition(atmospheric_resource_name).density;
                    double respcent = ORSX_AtmosphericResourceHandler.getAtmosphericResourceContent(vessel.mainBody.flightGlobalsIndex, currentresource);

                    double airdensity = part.vessel.atmDensity / 1000;

                    
                    double powerrequirements = scoopair / 0.15f * ecRequirement;
                    double airspeed = part.vessel.srf_velocity.magnitude + 40.0;
                    double air = airspeed * airdensity * scoopair / resourcedensity;


                    if (respcent > 0 && vessel.altitude <= ORSX_Helper.getMaxAtmosphericAltitude(vessel.mainBody))
                    {
                        double scoopedAtm = air * respcent;
                        float powerreceived = (float)Math.Max(part.RequestResource("ElectricCharge", powerrequirements * TimeWarp.fixedDeltaTime), 0);
                        float powerpcnt = (float)(powerreceived / powerrequirements / TimeWarp.fixedDeltaTime);
                        resflowf = (float)ORSX_Helper.fixedRequestResource(part, atmospheric_resource_name, -scoopedAtm * powerpcnt * TimeWarp.fixedDeltaTime);
                        
                        resflowf = -resflowf / TimeWarp.fixedDeltaTime;
                    }
                }
            }
        }


        public override string getResourceManagerDisplayName()
        {
            return "Atmospheric Scoop";
        }
    }
}
