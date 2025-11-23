

namespace ClassLibrary
{
    public class PowerSystem
    {
        public string MeasurementType;

        public int Node;

        public string EnergyDistrict;

        public string EnergySystem;

        public string UnifiedEnergySystem;

        public PowerSystem() : this("", 1, "", "", "") { }

        public PowerSystem(string measurementType, int node, string energyDistrict, string energySystem, string unifiedEnergySystem)
        {
            MeasurementType = measurementType;
            Node = node;
            EnergyDistrict = energyDistrict;
            EnergySystem = energySystem;
            UnifiedEnergySystem = unifiedEnergySystem;
        }

        public string GetInfo()
        {
            return $"MeasurementType: {MeasurementType}, Node: {Node}, EnergyDistrict: {EnergyDistrict}, EnergySystem: {EnergySystem}, UnifiedEnergySystem: {UnifiedEnergySystem}";
        }
    }
}
