using System;

namespace Optimization
{
    class Objective
    {
        public readonly string StructureName;
        public readonly ObjectiveType Type;
        public readonly DoseType DoseType;
        public readonly double Dose;
        public readonly double Priority;
        public readonly double Volume;
        public readonly double GEudParameter;

        public Objective(string objectiveString)
        {
            try
            {
                string[] objective = objectiveString.Split(' ');
                StructureName = objective[0].Replace('=', ' ');
                Type = (ObjectiveType)Enum.Parse(typeof(ObjectiveType), objective[1]);
                DoseType = (DoseType)Enum.Parse(typeof(DoseType), objective[2]);
                Dose = double.Parse(objective[3]);
                Priority = double.Parse(objective[4]);

                if (objective.Length > 5)
                    Volume = double.Parse(objective[5]);

                if (objective.Length > 6)
                    GEudParameter = double.Parse(objective[6]);
            }
            catch (Exception e)
            {
                Logger.WriteWarning("Can not parse objective " + objectiveString + ": " + e.Message);
            }
        }
    }
}
