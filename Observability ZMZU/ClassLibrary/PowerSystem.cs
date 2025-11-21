

namespace ClassLibrary
{
    public class PowerSystem
    {
        public string MeasurementType;

        public int Node;

        public string EnergySystem;

        public string UnifiedEnergySystem;

        public PowerSystem() : this("", 1, "", "") { }

        public PowerSystem(string measurementType, int node, string energySystem, string unifiedEnergySystem)
        {
            MeasurementType = measurementType;
            Node = node;
            EnergySystem = energySystem;
            UnifiedEnergySystem = unifiedEnergySystem;
        }

        public string GetInfo()
        {
            return $"{MeasurementType}, {Node}, {EnergySystem}, {UnifiedEnergySystem}";
        }
    }
}
