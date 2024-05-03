using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Optimization
{
    public class Program
    {
        public static Patient Patient { get; private set; }
        public static StructureSet StructureSet { get; private set; }
        public static ExternalPlanSetup Plan { get; private set; }

        public static bool IsImrt { get; private set; }

        public static void Execute(ScriptContext context)
        {
            try
            {
                Patient = context.Patient;
                StructureSet = context.StructureSet;
                Plan = context.ExternalPlanSetup;
                Logger.WriteInfo($"Patient: {Patient.LastName} {Patient.FirstName}");
                Logger.WriteInfo($"StructureSet: {StructureSet.Id}");
                Logger.WriteInfo($"Plan: {Plan.Id}");
                Optimize();
            }
            catch (Exception error)
            {
                Logger.WriteError(error.ToString());
                MessageBox.Show(error.ToString(), "Exception");
            }
        }

        private static void Optimize()
        {
            if (Patient.CanModifyData() == false)
                throw new Exception("The program can not modify data.");

            Patient.BeginModifications();

            var treatmentBeams = Plan.Beams.Where(b => b.IsSetupField == false && b.Id.Contains(Config.CbctName) == false);
            var treatmentBeam = treatmentBeams.First();

            IsImrt = treatmentBeam.GantryDirection == GantryDirection.None;

            string treatmentUnitId = treatmentBeam.TreatmentUnit.Id;
            Logger.WriteInfo("TreatmentUnit: " + treatmentUnitId);
            bool isHalcyon = treatmentUnitId.Contains(Config.HalcyonName);

            if (isHalcyon)
                SwitchCalculationModelToAaa();

            if (IsImrt && (isHalcyon == false))
                FixJaws(treatmentBeams);

            SetNto();
            List<Objective> objectives = GetObjectivesFromFile();
            double plannedDose = Plan.TotalDose.Dose;

            foreach (var structure in StructureSet.Structures)
                AddObjective(structure, objectives, plannedDose);
        }

        private static void SetNto()
        {
            Plan.OptimizationSetup.AddNormalTissueObjective(
                Config.NtoPriority,
                Config.NtoDistanceFromTarget,
                Config.NtoStartDose,
                Config.NtoEndDose,
                Config.NtoFallOff);
        }

        private static void AddObjective(Structure structure, IEnumerable<Objective> objectives, double plannedDose)
        {
            if (structure.Id.Equals(StructureNames.Body))
                AddBodyObjective(Plan, structure, plannedDose);
            else if (structure.Id.StartsWith(Config.CtvPrefix))
                AddCtvObjective(Plan, structure, plannedDose);
            else if (structure.Id.StartsWith(Config.PtvPrefix) && (structure.Id.Equals(StructureNames.PtvOptMinus) == false))
                AddPtvObjectives(Plan, structure, plannedDose);
            else
                AddOrgansObjective(Plan, structure, objectives, plannedDose);
        }

        private static void SwitchCalculationModelToAaa()
        {
            Plan.SetCalculationModel(CalculationType.PhotonVolumeDose, Config.AaaAlgorithmName);
            MessageBox.Show($"Calculation model switched to {Config.AaaAlgorithmName}.");
        }

        private static void FixJaws(IEnumerable<Beam> treatmentBeams)
        {
            foreach (var beam in treatmentBeams)
                Plan.OptimizationSetup.AddBeamSpecificParameter(beam, Config.DefaultSmoothX, Config.DefaultSmoothY, true);

            MessageBox.Show("Jaws fixed.");
        }

        private static void AddBodyObjective(ExternalPlanSetup plan, Structure body, double dose)
        {
            double priority = IsImrt ? Config.BodyImrtPriority : Config.BodyVmatPriority;
            plan.AddUpperObjective(body, dose * Config.BodyLowerDoseModifier, 0, priority);
        }

        private static void AddCtvObjective(ExternalPlanSetup plan, Structure structure, double dose)
        {
            plan.AddLowerObjective(structure, dose * Config.CtvUpperDoseModifier, 100, Config.CtvUpperPriority);
        }

        private static void AddOrgansObjective(ExternalPlanSetup plan, Structure structure, IEnumerable<Objective> objectives, double plannedDose)
        {
            bool hasObjective = false;

            foreach (var objective in objectives)
            {
                if (objective.StructureName.Equals(structure.Id))
                {
                    if (HasCroppedStructure(structure))
                        if (objective.Type == ObjectiveType.Mean)
                            continue;

                    SetObjective(plan, structure, objective);
                    hasObjective = true;
                }
                else if (IsCroppedOrganObjective(structure, objective))
                {
                    if (HasStructureFor(objective))
                        plan.AddUpperObjective(structure, plannedDose, 0, Config.OrganUpperPriority);
                    else if (HasMeanObjective(structure, objectives) == false)
                        SetObjective(plan, structure, objective);

                    hasObjective = true;
                }
            }
            if (hasObjective == false)
                Logger.WriteWarning("Can not find objective for " + structure.Id);
        }

        private static bool IsCroppedOrganObjective(Structure structure, Objective objective)
        {
            return objective.StructureName.Contains(structure.Id + StructureNames.CropPostfix);
        }

        private static bool HasCroppedStructure(Structure structure)
        {
            return StructureSet.Structures.Any(s => s.Id.Contains(structure.Id + StructureNames.CropPostfix));
        }

        private static bool HasStructureFor(Objective objective)
        {
            return StructureSet.Structures.Any(s => s.Id.Contains(objective.StructureName));
        }

        private static bool HasMeanObjective(Structure structure, IEnumerable<Objective> objectives)
        {
            return objectives.Any(o => o.StructureName.Equals(structure.Id) && o.Type == ObjectiveType.Mean);
        }

        private static void AddPtvObjectives(ExternalPlanSetup plan, Structure structure, double dose)
        {
            plan.AddLowerObjective(structure, dose * Config.PtvLowerDoseModifier, 100, Config.PtvLowerPriority);
            plan.AddUpperObjective(structure, dose * Config.PtvUpperDoseModifier, 0, Config.PtvUpperPriority);

            if (IsMainTarget(structure))
                AddMainTargetObjectives(plan, structure, dose);
        }

        private static bool IsMainTarget(Structure structure)
        {
            if (structure.Id.Equals(StructureNames.PtvOpt))
                return true;

            if (structure.Id.Equals(StructureNames.PtvAll))
                if (StructureSet.Structures.Any(s => s.Id.Equals(StructureNames.PtvOpt)) == false)
                    return true;

            return false;
        }

        private static void AddMainTargetObjectives(ExternalPlanSetup plan, Structure structure, double dose)
        {
            plan.AddLowerObjective(structure, dose * Config.MainTargetLowerDoseModifier, Config.MainTargetLowerVolume, 0);
            plan.AddUpperObjective(structure, dose * Config.MainTargetUpperDoseModifier, Config.MainTargetUpperVolume, 0);
            plan.AddEUDExactObjective(structure, dose, Config.MainTargetEud, Config.MainTargetEudPriority);
        }

        private static List<Objective> GetObjectivesFromFile()
        {
            string fileName = Config.ObjectivesFileName;
            string objectivesFile = GetObjectivesFile(fileName);
            string[] lines = File.ReadAllLines(objectivesFile);
            List<Objective> objectives = new List<Objective>(lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                Objective objective = new Objective(lines[i]);

                if (objective != null)
                    objectives.Add(objective);
            }
            return objectives;
        }

        private static string GetObjectivesFile(string fileName)
        {
            string pathToFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(pathToFile);
            string fileFullName = null;

            foreach (var file in files)
                if (file.Contains(fileName))
                    fileFullName = file;

            if (fileFullName == null)
                throw new NotImplementedException();

            return fileFullName;
        }

        private static void SetObjective(ExternalPlanSetup plan, Structure structure, Objective objective)
        {
            double plannedDose = plan.TotalDose.Dose;
            double dose = objective.DoseType == DoseType.Absolute ? objective.Dose : plannedDose * (objective.Dose / 100);
            double priority = objective.Priority;

            switch (objective.Type)
            {
                case ObjectiveType.Upper:
                    plan.AddUpperObjective(structure, dose, objective.Volume, priority);
                    break;
                case ObjectiveType.Lower:
                    plan.AddLowerObjective(structure, dose, objective.Volume, priority);
                    break;
                case ObjectiveType.Mean:
                    plan.AddMeanObjective(structure, dose, priority);
                    break;
                case ObjectiveType.EUDExact:
                    plan.AddEUDExactObjective(structure, dose, objective.GEudParameter, priority);
                    break;
            }
            Logger.WriteInfo($"Objective for {structure.Id} is set.");
        }
    }
}
