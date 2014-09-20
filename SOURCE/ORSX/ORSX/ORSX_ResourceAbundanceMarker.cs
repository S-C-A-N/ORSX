using UnityEngine;

namespace ORSX
{
    public class ORSX_ResourceAbundanceMarker
    {
        private readonly GameObject non_scale_sphere;
        private readonly GameObject scaled_sphere;

        public ORSX_ResourceAbundanceMarker(GameObject scaled_sphere, GameObject non_scale_sphere)
        {
            this.scaled_sphere = scaled_sphere;
            this.non_scale_sphere = non_scale_sphere;
        }

        public GameObject getScaledSphere()
        {
            return scaled_sphere;
        }

        public GameObject getPlanetarySphere()
        {
            return non_scale_sphere;
        }
    }
}