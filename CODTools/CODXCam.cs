using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CODTools
{
    internal class CODXCam
    {
        public static void ExportXCam(string FilePath, bool Grab = true, bool Edit = false)
        {
            // Configure scene
            using (var MayaCfg = new MayaSceneConfigure())
            {
                // First, get the current selection
                var ExportObjectList = new MSelectionList();
                MGlobal.getActiveSelectionList(ExportObjectList);

                if (ExportObjectList.DependNodes(MFn.Type.kCamera).Count() == 0)
                {
                    MGlobal.displayError("[CODTools] There are no camera's selected...");
                    return;
                }

                // Progress
                MayaCfg.StartProgress("Exporting XCam...", ((int)ExportObjectList.length + Math.Max((MayaCfg.SceneEnd - MayaCfg.SceneStart) + 1, 1)));

                // Create new XCam
                //var Result = new XAnim(System.IO.Path.GetFileNameWithoutExtension(FilePath));
                var Result = new XCam();

                // Metadata
                var SceneName = string.Empty;
                MGlobal.executeCommand("file -q -sceneName", out SceneName);

                // Set the Scene Name
                Result.scene = SceneName;

                // Set the number of frames
                Result.numframes = (MayaCfg.SceneEnd - MayaCfg.SceneStart) + 1;

                // Set the First Frame
                MayaCfg.SetTime(MayaCfg.SceneStart);

                // We may only need transforms lol
                var SceneCameras = new List<MFnCamera>();
                var SceneCameraTransforms = new List<MFnTransform>();

                var CameraCount = ExportObjectList.DependNodes(MFn.Type.kCamera).ToList().Count;

                for (int i = 0; i < CameraCount; i++)
                {
                    // Step
                    MayaCfg.StepProgress();

                    MDagPath mDagPath = new MDagPath();
                    ExportObjectList.getDagPath((uint)i, mDagPath);

                    var SceneCamera = new MFnCamera(mDagPath);
                    var SceneCameraTransform = new MFnTransform(mDagPath);

                    SceneCameras.Add(SceneCamera);
                    SceneCameraTransforms.Add(SceneCameraTransform);

                    var WorldPosition = SceneCameraTransform.getTranslation(MSpace.Space.kWorld);

                    var WorldRotation = new MQuaternion(MQuaternion.identity);
                    SceneCameraTransform.getRotation(WorldRotation, MSpace.Space.kWorld);

                    // EulerRotation = Radians
                    var EulerRotation = WorldRotation.asEulerRotation;
                    EulerRotation.reorderIt(MEulerRotation.RotationOrder.kXYZ);

                    // Substract 90 degrees from it in radians else we are facing the wrong way!
                    // This happens because we are on the Z Up Axis ( I Think )
                    EulerRotation.x = EulerRotation.x - ((Math.PI / 180) * 90);
                    EulerRotation.z = EulerRotation.z - ((Math.PI / 180) * 90);

                    var WorldMatrix = EulerRotation.asMatrix;

                    var Cam = new CODTools.Camera
                    {
                        name = SceneCameraTransform.name,
                        index = i,
                        type = "Perspective",
                        aperture = "FOCAL_LENGTH",
                        origin = new List<double> { WorldPosition.y * (1 / 2.54), WorldPosition.x * -(1 / 2.54), WorldPosition.z * (1 / 2.54) },
                        dir = new List<double> { WorldMatrix[1, 0], WorldMatrix[1, 1], WorldMatrix[1, 2] },
                        up = new List<double> { WorldMatrix[2, 0], WorldMatrix[2, 1], WorldMatrix[2, 2] },
                        right = new List<double> { WorldMatrix[0, 0], WorldMatrix[0, 1], WorldMatrix[0, 2] },
                        flen = SceneCamera.focalLength,
                        fov = SceneCamera.verticalFieldOfView,
                        fdist = SceneCamera.focusDistance,
                        fstop = SceneCamera.fStop,
                        lense = 10,
                        aspectratio = SceneCamera.aspectRatio,
                        nearz = SceneCamera.nearClippingPlane,
                        farz = SceneCamera.farClippingPlane,
                        animation = new List<CameraAnimation>()
                    };

                    Result.cameras.Add(Cam);
                }

                // Iterate over the frame range, then generate part frames
                for (int i = MayaCfg.SceneStart; i < (MayaCfg.SceneEnd + 1); i++)
                {
                    // Step and set time
                    MayaCfg.StepProgress();
                    MayaCfg.SetTime(i);

                    for (int c = 0; c < SceneCameras.Count; c++)
                    {
                        var WorldPosition = SceneCameraTransforms[c].getTranslation(MSpace.Space.kWorld);

                        var WorldRotation = new MQuaternion(MQuaternion.identity);
                        SceneCameraTransforms[c].getRotation(WorldRotation, MSpace.Space.kWorld);

                        // EulerRotation = Radians
                        var EulerRotation = WorldRotation.asEulerRotation;
                        EulerRotation.reorderIt(MEulerRotation.RotationOrder.kXYZ);

                        // Substract 90 degrees from it in radians else we are facing the wrong way!
                        // This happens because we are on the Z Up Axis ( I Think )
                        EulerRotation.x = EulerRotation.x - ((Math.PI / 180) * 90);
                        EulerRotation.z = EulerRotation.z - ((Math.PI / 180) * 90);

                        var WorldMatrix = EulerRotation.asMatrix;

                        var anim = new CameraAnimation
                        {
                            frame = i,
                            origin = new List<double> { WorldPosition.y * (1 / 2.54), WorldPosition.x * -(1 / 2.54), WorldPosition.z * (1 / 2.54) },
                            dir = new List<double> { WorldMatrix[1, 0], WorldMatrix[1, 1], WorldMatrix[1, 2] },
                            up = new List<double> { WorldMatrix[2, 0], WorldMatrix[2, 1], WorldMatrix[2, 2] },
                            right = new List<double> { WorldMatrix[0, 0], WorldMatrix[0, 1], WorldMatrix[0, 2] },
                            flen = SceneCameras[c].focalLength,
                            fov = SceneCameras[c].verticalFieldOfView,
                            fdist = SceneCameras[c].focusDistance,
                            fstop = SceneCameras[c].fStop,
                            lense = 10
                        };

                        Result.cameras[c].animation.Add(anim);
                    }
                }

                // Reset time
                MayaCfg.SetTime(MayaCfg.SceneStart);

                // Grab XAnim notetracks
                // TODO: Implement this lol
                if (Grab)
                    LoadNotetracks(ref Result);

                // Write
                Result.WriteExport(FilePath);
            }

            // Log complete
            MGlobal.displayInfo(string.Format("[CODTools] Exported {0}", System.IO.Path.GetFileName(FilePath)));
        }

        private static void LoadNotetracks(ref XCam Cam)
        {
            try
            {
                var SelectList = new MSelectionList();
                SelectList.add("SENotes");

                if (SelectList.length == 0)
                    return;

                // Get path
                var NotePath = new MDagPath();
                SelectList.getDagPath(0, NotePath);

                // Get node
                var Dep = new MFnDependencyNode(NotePath.node);
                var NotePlug = Dep.findPlug("Notetracks");

                var ResultJson = "{}";
                NotePlug.getValue(out ResultJson);

                // Deserialize
                var ResultNotes = new JavaScriptSerializer().Deserialize<Dictionary<string, List<int>>>(ResultJson);

                // Append
                foreach (var Note in ResultNotes)
                {
                    foreach (var Frame in Note.Value)
                        Cam.notetracks.Add(new XCamNoteTrack {name = Note.Key, frame = Frame });
                }
            }
            catch
            {
                // Nothing..
            }
        }

    }
}
