﻿namespace ORSX
{
    public class ORSX_AtmosphericResource
    {
        protected double abundance;
        protected string displayname;
        protected string resourcename;

        public ORSX_AtmosphericResource(string resourcename, double abundance, string displayname)
        {
            this.resourcename = resourcename;
            this.abundance = abundance;
            this.displayname = displayname;
        }

        public string getDisplayName()
        {
            return displayname;
        }

        public string getResourceName()
        {
            return resourcename;
        }

        public double getResourceAbundance()
        {
            return abundance;
        }
    }
}