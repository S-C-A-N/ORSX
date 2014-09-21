using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ORSX
{
    public class ORSX_OceanicResourceHandler
    {
        protected static Dictionary<int, List<ORSX_OceanicResource>> body_oceanic_resource_list = new Dictionary<int, List<ORSX_OceanicResource>>();

        public static double getOceanicResourceContent(int refBody, string resourcename)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = getOceanicCompositionForBody(refBody);
            if (bodyOceanicComposition.Count > 0)
            {
                foreach (ORSX_OceanicResource bodyAtmosphericResource in bodyOceanicComposition)
                {
                    if (bodyAtmosphericResource.getResourceName() == resourcename)
                    {
                        return bodyAtmosphericResource.getResourceAbundance();
                    }
                }
            }
            return 0;
        }

        public static double getOceanicResourceContent(int refBody, int resource)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = getOceanicCompositionForBody(refBody);
            if (bodyOceanicComposition.Count > resource)
            {
                return bodyOceanicComposition[resource].getResourceAbundance();
            }
            return 0;
        }

        public static string getOceanicResourceName(int refBody, int resource)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = getOceanicCompositionForBody(refBody);
            if (bodyOceanicComposition.Count > resource)
            {
                return bodyOceanicComposition[resource].getResourceName();
            }
            return null;
        }

        public static string getOceanicResourceDisplayName(int refBody, int resource)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = getOceanicCompositionForBody(refBody);
            if (bodyOceanicComposition.Count > resource)
            {
                return bodyOceanicComposition[resource].getDisplayName();
            }
            return null;
        }

        public static List<ORSX_OceanicResource> getOceanicCompositionForBody(int refBody)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = new List<ORSX_OceanicResource>();
            try
            {
                if (body_oceanic_resource_list.ContainsKey(refBody))
                {
                    return body_oceanic_resource_list[refBody];
                }
                else
                {
                    ConfigNode[] bodyOceanicResourceList = GameDatabase.Instance.GetConfigNodes("ORSX_OCEANIC_RESOURCE").Where(res => res.GetValue("celestialBodyName") == FlightGlobals.Bodies[refBody].name).ToArray();
                    foreach (ConfigNode bodyOceanicConfig in bodyOceanicResourceList)
                    {
                        string resourcename = null;
                        if (bodyOceanicConfig.HasValue("resourceName"))
                        {
                            resourcename = bodyOceanicConfig.GetValue("resourceName");
                        }
                        double resourceabundance = double.Parse(bodyOceanicConfig.GetValue("abundance"));
                        string displayname = bodyOceanicConfig.GetValue("guiName");
                        ORSX_OceanicResource bodyOceanicResource = new ORSX_OceanicResource(resourcename, resourceabundance, displayname);
                        bodyOceanicComposition.Add(bodyOceanicResource);
                    }
                    if (bodyOceanicComposition.Count > 1)
                    {
                        bodyOceanicComposition = bodyOceanicComposition.OrderByDescending(bacd => bacd.getResourceAbundance()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return bodyOceanicComposition;
        }
    }
}
