using System;
using System.Collections.Generic;

namespace ORSX
{
    public class ORSX_ResourceOvermanager
    {
        protected static Dictionary<String, ORSX_ResourceOvermanager> resources_managers =
            new Dictionary<String, ORSX_ResourceOvermanager>();

        protected Dictionary<Vessel, ORSX_ResourceManager> managers;
        protected String resource_name;

        public ORSX_ResourceOvermanager()
        {
        }

        public ORSX_ResourceOvermanager(String name)
        {
            managers = new Dictionary<Vessel, ORSX_ResourceManager>();
            resource_name = name;
        }

        public static ORSX_ResourceOvermanager getResourceOvermanagerForResource(String resource_name)
        {
            ORSX_ResourceOvermanager fnro;
            if (resources_managers.ContainsKey(resource_name))
            {
                fnro = resources_managers[resource_name];
            }
            else
            {
                fnro = new ORSX_ResourceOvermanager(resource_name);
                resources_managers.Add(resource_name, fnro);
            }
            return fnro;
        }

        public bool hasManagerForVessel(Vessel vess)
        {
            return managers.ContainsKey(vess);
        }

        public ORSX_ResourceManager getManagerForVessel(Vessel vess)
        {
            return managers[vess];
        }

        public void deleteManagerForVessel(Vessel vess)
        {
            managers.Remove(vess);
        }

        public void deleteManager(ORSX_ResourceManager manager)
        {
            managers.Remove(manager.getVessel());
        }

        public virtual ORSX_ResourceManager createManagerForVessel(PartModule pm)
        {
            var megamanager = new ORSX_ResourceManager(pm, resource_name);
            managers.Add(pm.vessel, megamanager);
            return megamanager;
        }
    }
}