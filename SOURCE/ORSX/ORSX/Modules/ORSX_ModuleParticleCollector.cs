﻿using System;
using System.Diagnostics;
using LibNoise.Unity.Operator;
using UnityEngine;

namespace ORSX
{
    public class ORSX_ModuleParticleCollector : ORSX_ResourceSuppliableModule
    {
        [KSPField(isPersistant = true)]
        public bool CollectorIsEnabled = false;

        [KSPField(isPersistant = true)]
        public bool isDisabled = true;

        [KSPField(isPersistant = true)]
        public int currentresource = 0;
        
        [KSPField(isPersistant = false)]
        public float particleRate = 0;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Resource")]
        public string currentresourceStr;

        [KSPField(isPersistant = false)] 
        public float ecRequirement = 0.1f;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Flow")]
        public string resflow;

        [KSPField(isPersistant = false)]
        public float altitudeBonus = 0f;

        protected float resflowf = 0;
        private double lastUpdateTime = 0.0f;

        [KSPEvent(guiActive = true, guiName = "Activate Collector", active = true)]
        public void ActivateCollector()
        {
            CollectorIsEnabled = true;
        }

        [KSPEvent(guiActive = true, guiName = "Disable Collector", active = true)]
        public void DisableCollector()
        {
            CollectorIsEnabled = false;
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


        [KSPAction("Activate Collector")]
        public void ActivateCollectorAction(KSPActionParam param)
        {
            ActivateCollector();
        }


        [KSPAction("Disable Collector")]
        public void DisableCollectorAction(KSPActionParam param)
        {
            DisableCollector();
        }


        [KSPAction("Toggle Collector")]
        public void ToggleCollectorAction(KSPActionParam param)
        {
            if (CollectorIsEnabled)
            {
                DisableCollector();
            }
            else
            {
                ActivateCollector();
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
            Events["ActivateCollector"].active = (!CollectorIsEnabled) && (!isDisabled);
            Events["DisableCollector"].active = (CollectorIsEnabled) && (!isDisabled);
            Events["ToggleResource"].active = (CollectorIsEnabled) && (!isDisabled);
            Fields["resflow"].guiActive = (CollectorIsEnabled) && (!isDisabled);
            Fields["currentresourceStr"].guiActive = (CollectorIsEnabled) && (!isDisabled);
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
            if (CollectorIsEnabled)
            {
                string atmospheric_resource_name = ORSX_AtmosphericResourceHandler.getAtmosphericResourceName(vessel.mainBody.flightGlobalsIndex, currentresource);
                if (atmospheric_resource_name != null)
                {
                    //range is 10% of the atmosphere
                    var range = (ORSX_Helper.getMaxAtmosphericAltitude(vessel.mainBody) * 1.1);
                    range += altitudeBonus;
                    double resourcedensity = PartResourceLibrary.Instance.GetDefinition(atmospheric_resource_name).density;
                    double respcent = ORSX_AtmosphericResourceHandler.getAtmosphericResourceContent(vessel.mainBody.flightGlobalsIndex, currentresource);

                    //If we're in the narrow band of the upper atmosphere
                    if (vessel.altitude <= range 
                        && respcent > 0 
                        && vessel.altitude >= ORSX_Helper.getMaxAtmosphericAltitude(vessel.mainBody))
                    {
                        print("[ORSX] PASS ");
                        /** RAILS **/
                        if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
                        {
                            return;
                        }

                        if (lastUpdateTime == 0.0f)
                        {
                            // Just started running
                            lastUpdateTime = Planetarium.GetUniversalTime();
                            return;
                        }

                        double deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, Utilities.MaxDeltaTime);
                        lastUpdateTime += deltaTime;
                        /** RAILS **/


                        double powerrequirements = particleRate/0.15f*ecRequirement;
                        double desiredPower = powerrequirements * TimeWarp.fixedDeltaTime;
                        double maxPower = powerrequirements * Math.Max(Utilities.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime);
                        var powerRequested = Math.Min(desiredPower, maxPower);                        


                        double particles = particleRate/resourcedensity;
                        double CollectedParticles = particles*respcent;
                        float powerreceived =
                            (float)
                                Math.Max(
                                    part.RequestResource("ElectricCharge", powerRequested),
                                    0);
                        float powerpcnt = (float)(powerreceived / desiredPower);
                        resflowf =
                            (float)
                                ORSX_Helper.fixedRequestResource(part, atmospheric_resource_name,
                                    -CollectedParticles*powerpcnt*deltaTime);

                        resflowf = -resflowf/(float)deltaTime;
                    }
                }
            }
        }


        public override string getResourceManagerDisplayName()
        {
            return "Atmospheric Collector";
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddValue("lastUpdateTime", lastUpdateTime);
        }
    }
}
