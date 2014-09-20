using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ORSX
{
    public class ORSX_AtmosphericResourceHandler
    {
        protected static Dictionary<int, List<ORSX_AtmosphericResource>> body_atmospheric_resource_list =
            new Dictionary<int, List<ORSX_AtmosphericResource>>();

        public static double getAtmosphericResourceContent(int refBody, string resourcename)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            ORSX_AtmosphericResource resource =
                bodyAtmosphericComposition.FirstOrDefault(oor => oor.getResourceName() == resourcename);
            return resource != null ? resource.getResourceAbundance() : 0;
        }

        public static double getAtmosphericResourceContentByDisplayName(int refBody, string resourcename)
        {
            List<ORSX_AtmosphericResource> bodyAtmosphericComposition = getAtmosphericCompositionForBody(refBody);
            ORSX_AtmosphericResource resource =
                bodyAtmosphericComposition.FirstOrDefault(oor => oor.getDisplayName() == resourcename);
            return resource != null ? resource.getResourceAbundance() : 0;
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
            var bodyAtmosphericComposition = new List<ORSX_AtmosphericResource>();
            try
            {
                if (body_atmospheric_resource_list.ContainsKey(refBody))
                {
                    return body_atmospheric_resource_list[refBody];
                }
                ConfigNode atmospheric_resource_pack =
                    GameDatabase.Instance.GetConfigNodes("ATMOSPHERIC_RESOURCE_PACK_DEFINITION").FirstOrDefault();
                Debug.Log("[ORSX] Loading atmospheric data from pack: " +
                          (atmospheric_resource_pack.HasValue("name")
                              ? atmospheric_resource_pack.GetValue("name")
                              : "unknown pack"));
                if (atmospheric_resource_pack != null)
                {
                    List<ConfigNode> atmospheric_resource_list =
                        atmospheric_resource_pack.GetNodes("ATMOSPHERIC_RESOURCE_DEFINITION")
                            .Where(res => res.GetValue("celestialBodyName") == FlightGlobals.Bodies[refBody].name)
                            .ToList();
                    bodyAtmosphericComposition =
                        atmospheric_resource_list.Select(
                            orsc =>
                                new ORSX_AtmosphericResource(
                                    orsc.HasValue("resourceName") ? orsc.GetValue("resourceName") : null,
                                    double.Parse(orsc.GetValue("abundance")), orsc.GetValue("guiName"))).ToList();
                    if (bodyAtmosphericComposition.Any())
                    {
                        bodyAtmosphericComposition =
                            bodyAtmosphericComposition.OrderByDescending(bacd => bacd.getResourceAbundance()).ToList();
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