using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: ExtensionPlugin(typeof(CODTools.PluginInfo))]
[assembly: MPxFileTranslatorClass(typeof(CODTools.CODXModelExport), "CoD XMODEL_EXPORT", null, CODTools.PluginInfo.modelExportOptionScript, CODTools.PluginInfo.modelExportDefaultOptions)]
[assembly: MPxFileTranslatorClass(typeof(CODTools.CODXModelBin), "CoD XMODEL_BIN", null, CODTools.PluginInfo.modelExportOptionScript, CODTools.PluginInfo.modelExportDefaultOptions)]
[assembly: MPxFileTranslatorClass(typeof(CODTools.CODXAnimExport), "CoD XANIM_EXPORT", null, CODTools.PluginInfo.animExportOptionScript, CODTools.PluginInfo.animExportDefaultOptions)]
[assembly: MPxFileTranslatorClass(typeof(CODTools.CODXAnimBin), "CoD XANIM_BIN", null, CODTools.PluginInfo.animExportOptionScript, CODTools.PluginInfo.animExportDefaultOptions)]
[assembly: MPxFileTranslatorClass(typeof(CODTools.CODXAnimSiege), "CoD SIEGE_ANIM_SOURCE", null, null, null)]
[assembly: MPxFileTranslatorClass(typeof(CODTools.CODXCamExport), "CoD XCAM_EXPORT", null, null, null)]

namespace CODTools
{
    public class PluginInfo : IExtensionPlugin
    {
        public const string modelExportOptionScript = "xmodelExportOptions";
        public const string modelExportDefaultOptions = "exportsiege=0;";

        public const string animExportOptionScript = "xanimExportOptions";
        public const string animExportDefaultOptions = "grabnotes=1;exportTagAlign=0;";

        public bool InitializePlugin()
        {
            MSceneMessage.AfterNew += MSceneMessage_AfterNew;

            try
            {
                // Create UI
                MGlobal.executeCommand("catchQuiet(deleteUI(\"CODTools\"));");
                // Create it
                MGlobal.executeCommand(CODTools.Properties.Resources.CODToolsMenu);
            }
            catch { }

            return true;
        }

        private void MSceneMessage_AfterNew(object sender, MBasicFunctionArgs e)
        {
            // Clear List on new Scene
            Cosmetics.Bones.Clear();
        }

        public bool UninitializePlugin()
        {
            try
            {
                // Clear UI if available
                MGlobal.executeCommand("catchQuiet(deleteUI(\"CODTools\"));");
            }
            catch { }

            return true;
        }

        public string GetMayaDotNetSdkBuildVersion()
        {
            return string.Empty;
        }
    }
}
