using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ORSX
{
    public class ORSX_OceanicResourceHandler
    {
        protected static Dictionary<int, List<ORSX_OceanicResource>> body_oceanic_resource_list =
            new Dictionary<int, List<ORSX_OceanicResource>>();

        public static double getOceanicResourceContent(int refBody, string resourcename)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = getOceanicCompositionForBody(refBody);
            ORSX_OceanicResource resource =
                bodyOceanicComposition.FirstOrDefault(oor => oor.getResourceName() == resourcename);
            return resource != null ? resource.getResourceAbundance() : 0;
        }

        public static double getOceanicResourceContent(int refBody, int resource)
        {
            List<ORSX_OceanicResource> bodyOceanicComposition = getOceanicCompositionForBody(refBody);
            if (bodyOceanicComposition.Count > resource) return bodyOceanicComposition[resource].getResourceAbundance();
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
            var bodyOceanicComposition = new List<ORSX_OceanicResource>();
            try
            {
                if (body_oceanic_resource_list.ContainsKey(refBody))
                {
                    return body_oceanic_resource_list[refBody];
                }
                ConfigNode oceanic_resource_pack =
                    GameDatabase.Instance.GetConfigNodes("OCEANIC_RESOURCE_PACK_DEFINITION").FirstOrDefault();
                Debug.Log("[ORSX] Loading oceanic data from pack: " +
                          (oceanic_resource_pack.HasValue("name")
                              ? oceanic_resource_pack.GetValue("name")
                              : "unknown pack"));
                if (oceanic_resource_pack != null)
                {
                    List<ConfigNode> oceanic_resource_list =
                        oceanic_resource_pack.GetNodes("OCEANIC_RESOURCE_DEFINITION")
                            .Where(res => res.GetValue("celestialBodyName") == FlightGlobals.Bodies[refBody].name)
                            .ToList();
                    bodyOceanicComposition =
                        oceanic_resource_list.Select(
                            orsc =>
                                new ORSX_OceanicResource(
                                    orsc.HasValue("resourceName") ? orsc.GetValue("resourceName") : null,
                                    double.Parse(orsc.GetValue("abundance")), orsc.GetValue("guiName"))).ToList();
                    if (bodyOceanicComposition.Any())
                    {
                        bodyOceanicComposition =
                            bodyOceanicComposition.OrderByDescending(bacd => bacd.getResourceAbundance()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[ORSX] Exception while loading oceanic resources : " + ex);
            }
            return bodyOceanicComposition;
        }
    }
}