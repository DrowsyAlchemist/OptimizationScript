using Optimization;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;

[assembly: AssemblyVersion("1.0.1.8")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {

        }

        public void Execute(ScriptContext context, Window window)
        {
            Logger.SetWindow(window);
            ConfigParser.SetConfig(typeof(Config));
            ConfigParser.SetConfig(typeof(StructureNames));
            Program.Execute(context);
            MessageBox.Show("Complete!");
        }
    }
}
