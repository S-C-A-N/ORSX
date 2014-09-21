using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ORSX
{
    public class ORSX_AtmosphericResourceHandler
    {
        protected static Dictionary<int, List<ORSX_AtmosphericResource>> body_atmospheric_resource_list = new Dictionary<int, List<ORSX_AtmosphericResource>>();

        public static double getAtmosphericResourceContent(int refBody, string resourcename)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            if (bodyAtmosphericComposition.Count > 0)
            {
                foreach (ORSX_AtmosphericResource bodyAtmosphericResource in bodyAtmosphericComposition)
                {
                    if (bodyAtmosphericResource.getResourceName() == resourcename)
                    {
                        return bodyAtmosphericResource.getResourceAbundance();
                    }
                }
            }
            return 0;
        }

        public static double getAtmosphericResourceContentByDisplayName(int refBody, string resourcename)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            if (bodyAtmosphericComposition.Count > 0)
            {
                foreach (ORSX_AtmosphericResource bodyAtmosphericResource in bodyAtmosphericComposition)
                {
                    if (bodyAtmosphericResource.getDisplayName() == resourcename)
                    {
                        return bodyAtmosphericResource.getResourceAbundance();
                    }
                }
            }
            return 0;
        }

        public static double getAtmosphericResourceContent(int refBody, int resource)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            if (bodyAtmosphericComposition.Count > resource)
            {
                return bodyAtmosphericComposition[resource].getResourceAbundance();
            }
            return 0;
        }

        public static string getAtmosphericResourceName(int refBody, int resource)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            if (bodyAtmosphericComposition.Count > resource)
            {
                return bodyAtmosphericComposition[resource].getResourceName();
            }
            return null;
        }

        public static string getAtmosphericResourceDisplayName(int refBody, int resource)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            if (bodyAtmosphericComposition.Count > resource)
            {
                return bodyAtmosphericComposition[resource].getDisplayName();
            }
            return null;
        }

        public static List<ORSX_AtmosphericResource> getAtmosphericCompositionForBody(int refBody)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = new List<ORSX_AtmosphericResource>();
            try
            {
                if (body_atmospheric_resource_list.ContainsKey(refBody))
                {
                    return body_atmospheric_resource_list[refBody];
                }
                else
                {
                    ConfigNode[] bodyAtmosphericResourceList = GameDatabase.Instance.GetConfigNodes("ORSX_ATMOSPHERIC_RESOURCE").Where(res => res.GetValue("celestialBodyName") == FlightGlobals.Bodies[refBody].name).ToArray();
                    foreach (ConfigNode bodyAtmosphericConfig in bodyAtmosphericResourceList)
                    {
                        string resourcename = null;
                        if (bodyAtmosphericConfig.HasValue("resourceName"))
                        {
                            resourcename = bodyAtmosphericConfig.GetValue("resourceName");
                        }
                        double resourceabundance = double.Parse(bodyAtmosphericConfig.GetValue("abundance"));
                        string displayname = bodyAtmosphericConfig.GetValue("guiName");
                        ORSX_AtmosphericResource bodyAtmosphericResource = new ORSX_AtmosphericResource(resourcename, resourceabundance, displayname);
                        bodyAtmosphericComposition.Add(bodyAtmosphericResource);
                    }
                    if (bodyAtmosphericComposition.Count > 1)
                    {
                        bodyAtmosphericComposition = bodyAtmosphericComposition.OrderByDescending(bacd => bacd.getResourceAbundance()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return bodyAtmosphericComposition;
        }
    }
}
