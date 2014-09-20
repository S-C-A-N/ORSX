namespace ORSX
{
    public class ORSX_PlanetaryResourcePixel
    {
        protected int body;
        protected string name;
        protected double quantity;
        protected string resourcename;

        public ORSX_PlanetaryResourcePixel(string name, double quantity, int body)
        {
            this.name = name;
            this.quantity = quantity;
            this.body = body;
        }

        public CelestialBody Body
        {
            get { return FlightGlobals.Bodies.Count > body ? FlightGlobals.Bodies[body] : null; }
        }

        public void setResourceName(string resourcename)
        {
            this.resourcename = resourcename;
        }

        public string getResourceName()
        {
            return resourcename;
        }

        public double getAmount()
        {
            return quantity;
        }

        public int getBody()
        {
            return body;
        }

        public string getName()
        {
            return name;
        }
    }
}