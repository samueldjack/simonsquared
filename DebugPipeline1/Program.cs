#region License
// This file is part of Simon Squared
// 
// Simon Squared is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Simon Squared is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
// along with Simon Squared. If not, see <http://www.gnu.org/licenses/>.
#endregion
#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace DebugPipeline1
{
    /// <summary>
    /// To debug the XNA Framework Content Pipeline:
    ///     1. Modify the constants below to match your project
    ///     2. Set this project to be the Startup Project
    ///     3. Start debugging
    /// </summary>
    class Program
    {
        /// <summary>
        /// TODO: Change this to the full path of the content project whose pipeline you want to debug.
        ///       Example:
        ///               private const string ProjectToDebug = @"E:\Projects\BitBucket\Flatlings\Flatlings\FlatlingsContent\FlatlingsContent.contentproj";
        /// </summary>
        private const string ProjectToDebug = @"E:\Projects\BitBucket\Flatlings\Flatlings\FlatlingsContent\FlatlingsContent.contentproj";

        /// <summary>
        /// TODO: Change this to the content item you want to debug. The content pipeline will only
        ///       build this one item and no others. Leave SingleItem null or empty to build the entire
        ///       content project while debugging.
        ///       Example:
        ///               private const string SingleItem = @"dude.fbx";
        /// </summary>
        private const string SingleItem = @"Levels\Levels.csv";

        /// <summary>
        /// TODO: Set the XnaProfile to HiDef or Reach, depending on your target graphics profile.
        /// NOTE: Windows Phone projects only support content built for the Reach profile.
        /// </summary>
        private const GraphicsProfile XnaProfile = GraphicsProfile.Reach;

        /// <summary>
        /// TODO: You generally don't need to change this unless your custom importer or processor uses
        ///       the TargetPlatform property of its context object.
        /// </summary>
        private const TargetPlatform XnaPlatform = TargetPlatform.WindowsPhone;

        /// <summary>
        /// TODO: Change this if you want to see more output from MSBuild.
        /// NOTE: Detailed and Diagnostic output makes builds noticeably slower.
        /// </summary>
        private const LoggerVerbosity LoggingVerbosity = LoggerVerbosity.Normal;

        #region MSBuild hosting and execution

        /// <summary>
        /// This program hosts the MSBuild engine and builds the content project with parameters based
        /// on the constant values specified above.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists(ProjectToDebug))
            {
                throw new FileNotFoundException(String.Format("The project file '{0}' does not exist. Set the ProjectToDebug field to the full path of the project you want to debug.", ProjectToDebug), ProjectToDebug);
            }
            if (!String.IsNullOrEmpty(SingleItem) && !File.Exists(Path.Combine(WorkingDirectory, SingleItem)))
            {
                throw new FileNotFoundException(String.Format("The project item '{0}' does not exist. Set the SingleItem field to the relative path of the content item you want to debug, or leave it empty to debug the whole project.", SingleItem), SingleItem);
            }
            Environment.CurrentDirectory = WorkingDirectory;

            var globalProperties = new Dictionary<string, string>();

            globalProperties.Add("Configuration", Configuration);
            globalProperties.Add("XnaProfile", XnaProfile.ToString());
            globalProperties.Add("XNAContentPipelineTargetPlatform", XnaContentPipelineTargetPlatform);
            globalProperties.Add("SingleItem", SingleItem);
            globalProperties.Add("CustomAfterMicrosoftCommonTargets", DebuggingTargets);

            var project = ProjectCollection.GlobalProjectCollection.LoadProject(ProjectName, globalProperties, MSBuildVersion);
            bool succeeded = project.Build("rebuild", Loggers);

            // To read the build output in the console window, place a breakpoint on the
            // Debug.WriteLine statement below.
            Debug.WriteLine("Build " + (succeeded ? "Succeeded." : "Failed."));
        }

        #region Additional, rarely-changing property values

        private const string Configuration = "Debug";
        private const string MSBuildVersion = "4.0";

        private static IEnumerable<ILogger> Loggers
        {
            get
            {
                return new ILogger[] { new ConsoleLogger(LoggingVerbosity) };
            }
        }

        private static string WorkingDirectory
        {
            get { return Path.GetDirectoryName(Path.GetFullPath(ProjectToDebug)); }
        }

        private static string BuildToolDirectory
        {
            get
            {
                string startupExe = System.Reflection.Assembly.GetEntryAssembly().Location;
                return Path.GetDirectoryName(startupExe);
            }
        }

        private static string ProjectName
        {
            get { return Path.GetFileName(Path.GetFullPath(ProjectToDebug)); }
        }

        private static string XnaContentPipelineTargetPlatform
        {
            get
            {
                return XnaPlatform.ToString();
            }
        }

        public static string DebuggingTargets
        {
            get
            {
                if (String.IsNullOrEmpty(SingleItem))
                {
                    return String.Empty;
                }

                string targetsPath = @"Targets\Debugging.targets";
                return Path.Combine(BuildToolDirectory, targetsPath);
            }
        }

        #endregion

        #endregion
    }
}
