namespace Optimization
{
    public static class Config
    {
        public static double CtvUpperDoseModifier { get; private set; } = 1;
        public static double CtvUpperPriority { get; private set; } = 100;

        public static double PtvUpperDoseModifier { get; private set; } = 1.04;
        public static double PtvUpperPriority { get; private set; } = 100;

        public static double PtvLowerDoseModifier { get; private set; } = 1;
        public static double PtvLowerPriority { get; private set; } = 100;

        public static double MainTargetLowerDoseModifier { get; private set; } = 0.95;
        public static double MainTargetLowerVolume { get; private set; } = 98;
        public static double MainTargetUpperDoseModifier { get; private set; } = 1.07;
        public static double MainTargetUpperVolume { get; private set; } = 2;
        public static double MainTargetEud { get; private set; } = -1;
        public static double MainTargetEudPriority { get; private set; } = 50;

        public static double BodyLowerDoseModifier { get; private set; } = 1.04;
        public static double BodyVmatPriority { get; private set; } = 130;
        public static double BodyImrtPriority { get; private set; } = 320;

        public static double OrganUpperPriority { get; private set; } = 60;

        public static double DefaultSmoothX = 40;
        public static double DefaultSmoothY = 30;

        public static double NtoPriority = 60;
        public static double NtoDistanceFromTarget = 3;
        public static double NtoStartDose = 100;
        public static double NtoEndDose = 30;
        public static double NtoFallOff = 0.1;

        public static string HalcyonName = "HAL1335";
        public static string AaaAlgorithmName = "AAA_15.6.06";
        public static string CbctName = "CBCT";

        public static string ObjectivesFileName = "Objectives.txt";

        public static string CtvPrefix = "CTV";
        public static string PtvPrefix = "PTV";
    }
}
