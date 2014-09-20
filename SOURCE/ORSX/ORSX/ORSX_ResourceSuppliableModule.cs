using System;
using System.Collections.Generic;

namespace ORSX
{
    public abstract class ORSX_ResourceSuppliableModule : PartModule, ORSX_ResourceSuppliable, ORSX_ResourceSupplier
    {
        protected Dictionary<String, bool> fnresource_manager_responsibilities = new Dictionary<String, bool>();

        protected Dictionary<String, ORSX_ResourceManager> fnresource_managers =
            new Dictionary<String, ORSX_ResourceManager>();

        protected Dictionary<String, double> fnresource_supplied = new Dictionary<String, double>();

        protected String[] resources_to_supply;

        public void receiveFNResource(double power, String resourcename)
        {
            //resourcename = resourcename.ToLower();
            if (fnresource_supplied.ContainsKey(resourcename))
            {
                fnresource_supplied[resourcename] = power;
            }
            else
            {
                fnresource_supplied.Add(resourcename, power);
            }
        }

        public float consumeFNResource(double power, String resourcename)
        {
            power = Math.Max(power, 0);
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }
            if (!fnresource_supplied.ContainsKey(resourcename))
            {
                fnresource_supplied.Add(resourcename, 0);
            }
            double power_taken = Math.Max(Math.Min(power, fnresource_supplied[resourcename]*TimeWarp.fixedDeltaTime), 0);
            fnresource_supplied[resourcename] -= power_taken;
            ORSX_ResourceManager mega_manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);

            mega_manager.powerDraw(this, power);
            return (float) power_taken;
        }

        public float consumeFNResource(float power, String resourcename)
        {
            return consumeFNResource((double) power, resourcename);
        }

        public virtual string getResourceManagerDisplayName()
        {
            return ClassName;
        }

        public virtual int getPowerPriority()
        {
            return 2;
        }

        public double supplyFNResource(double supply, String resourcename)
        {
            supply = Math.Max(supply, 0);
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.powerSupply(this, supply);
        }

        public float supplyFNResource(float supply, String resourcename)
        {
            return (float) supplyFNResource((double) supply, resourcename);
        }

        public float supplyFNResourceFixedMax(float supply, float maxsupply, String resourcename)
        {
            return (float) supplyFNResourceFixedMax(supply, (double) maxsupply, resourcename);
        }

        public double supplyFNResourceFixedMax(double supply, double maxsupply, String resourcename)
        {
            supply = Math.Max(supply, 0);
            maxsupply = Math.Max(maxsupply, 0);
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.powerSupplyFixedMax(this, supply, maxsupply);
        }

        public float supplyManagedFNResource(float supply, String resourcename)
        {
            return (float) supplyManagedFNResource((double) supply, resourcename);
        }

        public double supplyManagedFNResource(double supply, String resourcename)
        {
            supply = Math.Max(supply, 0);
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.managedPowerSupply(this, supply);
        }

        public float supplyManagedFNResourceWithMinimum(float supply, float rat_min, String resourcename)
        {
            return (float) supplyManagedFNResourceWithMinimum(supply, (double) rat_min, resourcename);
        }

        public double supplyManagedFNResourceWithMinimum(double supply, double rat_min, String resourcename)
        {
            supply = Math.Max(supply, 0);
            rat_min = Math.Max(rat_min, 0);
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.managedPowerSupplyWithMinimum(this, supply, rat_min);
        }

        public float getCurrentResourceDemand(String resourcename)
        {
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.getCurrentResourceDemand();
        }

        public float getStableResourceSupply(String resourcename)
        {
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.getStableResourceSupply();
        }

        public float getCurrentUnfilledResourceDemand(String resourcename)
        {
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.getCurrentUnfilledResourceDemand();
        }

        public double getResourceBarRatio(String resourcename)
        {
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.getResourceBarRatio();
        }

        public double getSpareResourceCapacity(String resourcename)
        {
            if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
            {
                return 0;
            }

            ORSX_ResourceManager manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
            return manager.getSpareResourceCapacity();
        }

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor && resources_to_supply != null)
            {
                foreach (String resourcename in resources_to_supply)
                {
                    ORSX_ResourceManager manager;

                    if (getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
                    {
                        manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
                        if (manager == null)
                        {
                            manager = createResourceManagerForResource(resourcename);
                            print("[ORSX] Creating Resource Manager for Vessel " + vessel.GetName() + " (" + resourcename +
                                  ")");
                        }
                    }
                    else
                    {
                        manager = createResourceManagerForResource(resourcename);

                        print("[ORSX] Creating Resource Manager for Vessel " + vessel.GetName() + " (" + resourcename +
                              ")");
                    }
                }
            }
        }

        public override void OnFixedUpdate()
        {
            if (resources_to_supply != null)
            {
                foreach (String resourcename in resources_to_supply)
                {
                    ORSX_ResourceManager manager;

                    if (!getOvermanagerForResource(resourcename).hasManagerForVessel(vessel))
                    {
                        manager = createResourceManagerForResource(resourcename);
                        print("[ORSX] Creating Resource Manager for Vessel " + vessel.GetName() + " (" + resourcename +
                              ")");
                    }
                    else
                    {
                        manager = getOvermanagerForResource(resourcename).getManagerForVessel(vessel);
                        if (manager == null)
                        {
                            manager = createResourceManagerForResource(resourcename);
                            print("[ORSX] Creating Resource Manager for Vessel " + vessel.GetName() + " (" + resourcename +
                                  ")");
                        }
                    }

                    if (manager.getPartModule().vessel != vessel || manager.getPartModule() == null)
                    {
                        manager.updatePartModule(this);
                    }

                    if (manager.getPartModule() == this)
                    {
                        manager.update();
                    }
                }
            }
        }

        protected virtual ORSX_ResourceManager createResourceManagerForResource(string resourcename)
        {
            return getOvermanagerForResource(resourcename).createManagerForVessel(this);
        }

        protected virtual ORSX_ResourceOvermanager getOvermanagerForResource(string resourcename)
        {
            return ORSX_ResourceOvermanager.getResourceOvermanagerForResource(resourcename);
        }
    }
}