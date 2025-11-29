using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractionWithTheDatabase
{
    public class PowerSystem
    {
        public int Node;

        public string EnergyDistrict;

        public string EnergySystem;

        public string UnifiedEnergySystem;

        public PowerSystem() : this(1, "", "", "") { }

        public PowerSystem(int node, string energyDistrict, string energySystem, string unifiedEnergySystem)
        {
            Node = node;
            EnergyDistrict = energyDistrict;
            EnergySystem = energySystem;
            UnifiedEnergySystem = unifiedEnergySystem;
        }

        public string GetInfo()
        {
            return $"Node: {Node}, EnergyDistrict: {EnergyDistrict}, EnergySystem: {EnergySystem}, UnifiedEnergySystem: {UnifiedEnergySystem}";
        }
    }
}
