using System;
using System.IO;

namespace IPB.LogicApp.Standard.Testing.Local.Host
{
	public class FuncHelper
    {

        public static string GetFuncPath()
        {
            var funcPath = GetFuncPathOnLocalMachine();
            if (File.Exists(funcPath))
                return funcPath;
            else
            {
                funcPath = GetFuncPathOnBuildAgent();
                if (File.Exists(funcPath))
                    return funcPath;
                else
				{
					funcPath = GetFuncPathOnBuildAgentWithFuncCoreToolInstaller();
					if (File.Exists(funcPath))
						return funcPath;
					else
					{
						throw new Exception("The func.exe does not exist at the expected paths");
					}
				}
            }
        }

        ///When func is installed by npm on your dev machine it does into this path by default
        public static string GetFuncPathOnLocalMachine()
        {
            var funcPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\npm\\node_modules\\azure-functions-core-tools\\bin\\func.exe";
            return funcPath;
        }

        ///When npm installs func.exe on the build agent it does into this path by default
        public static string GetFuncPathOnBuildAgent()
        {
            return "C:\\npm\\prefix\\node_modules\\azure-functions-core-tools\\bin\\func.exe";
		}

		///When using Azure DevOps Pipeline FuncToolsInstaller@0
		public static string GetFuncPathOnBuildAgentWithFuncCoreToolInstaller()
		{
			return $"C:\\hostedtoolcache\\windows\\func\\{Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_CORE_TOOLS_VERSION")}\\x64\\func.exe";
		}
	}
}
