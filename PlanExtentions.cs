using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Optimization
{
    static class PlanExtentions
    {
        public static void AddUpperObjective(this ExternalPlanSetup plan, Structure structure, double dose, double volume, double priority)
        {
            plan.OptimizationSetup.AddPointObjective(
                        structure, OptimizationObjectiveOperator.Upper, new DoseValue(dose, DoseValue.DoseUnit.Gy), volume, priority);
        }

        public static void AddLowerObjective(this ExternalPlanSetup plan, Structure structure, double dose, double volume, double priority)
        {
            plan.OptimizationSetup.AddPointObjective(
                        structure, OptimizationObjectiveOperator.Lower, new DoseValue(dose, DoseValue.DoseUnit.Gy), volume, priority);
        }

        public static void AddMeanObjective(this ExternalPlanSetup plan, Structure structure, double dose, double priority)
        {
            plan.OptimizationSetup.AddMeanDoseObjective(
                structure, new DoseValue(dose, DoseValue.DoseUnit.Gy), priority);
        }

        public static void AddEUDExactObjective(this ExternalPlanSetup plan, Structure structure, double dose, double paramertA, double priority)
        {
            plan.OptimizationSetup.AddEUDObjective(
                structure, OptimizationObjectiveOperator.Exact, new DoseValue(dose, DoseValue.DoseUnit.Gy), paramertA, priority);
        }
    }
}
