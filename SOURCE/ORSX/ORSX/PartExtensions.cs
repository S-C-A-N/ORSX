using System;
using System.Collections.Generic;

namespace ORSX
{
    public static class PartExtensions
    {
        public static IEnumerable<PartResource> GetConnectedResources(this Part part, PartResourceDefinition definition)
        {
            var resources = new List<PartResource>();
            part.GetConnectedResources(definition.id, definition.resourceFlowMode, resources);
            return resources;
        }

        public static IEnumerable<PartResource> GetConnectedResources(this Part part, String resourcename)
        {
            PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition(resourcename);
            return GetConnectedResources(part, definition);
        }

        public static double ImprovedRequestResource(this Part part, String resourcename, double resource_amount)
        {
            return ORSX_Helper.fixedRequestResource(part, resourcename, resource_amount);
        }
    }
}