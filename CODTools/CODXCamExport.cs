using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CODTools
{
    public class CODXCamExport : MPxFileTranslator
    {
        public override string defaultExtension()
        {
            // Default extension
            return "XCAM_EXPORT";
        }

        public override bool haveReadMethod()
        {
            return false;
        }

        public override bool haveWriteMethod()
        {
            return true;
        }

        public override MPxFileTranslator.MFileKind identifyFile(MFileObject file, string buffer, short bufferLen)
        {
            // It's our file
            if (file.fullName.ToUpper().EndsWith(".XCAM_EXPORT"))
                return MFileKind.kIsMyFileType;

            // Failed
            return MFileKind.kNotMyFileType;
        }

        public override void writer(MFileObject file, string optionsString, MPxFileTranslator.FileAccessMode mode)
        {
            // Prepare to export, pass it off
            if (file.fullName.ToUpper().EndsWith(".XCAM_EXPORT"))
            {
                // Parse settings
                bool GrabNotes = true, EditNotes = false;

                var SplitSettings = optionsString.Trim().Split(';');
                foreach (var Setting in SplitSettings)
                {
                    if (string.IsNullOrWhiteSpace(Setting))
                        continue;

                    var SettingValue = Setting.Split('=');
                    if (SettingValue.Length < 2)
                        continue;

                    if (SettingValue[0] == "grabnotes")
                        GrabNotes = (SettingValue[1] == "1");
                    else if (SettingValue[0] == "editnotes")
                        EditNotes = (SettingValue[1] == "1");
                }

                // Export XCam
                CODXCam.ExportXCam(file.fullName, GrabNotes, EditNotes);
            }
        }

        public override void reader(MFileObject file, string optionsString, MPxFileTranslator.FileAccessMode mode)
        {
            throw new NotImplementedException("We only support support writer not reader");
        }
    }
}
