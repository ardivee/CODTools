using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;


[assembly: MPxCommandClass(typeof(CODTools.MarkCosmeticCommand), "CODMarkCosmetic")]
[assembly: MPxCommandClass(typeof(CODTools.UnMarkCosmeticCommand), "CODUnMarkCosmetic")]
[assembly: MPxCommandClass(typeof(CODTools.ClearMarkedCosmeticsCommand), "CODClearMarkedCosmetics")]
[assembly: MPxCommandClass(typeof(CODTools.XCamCommand), "CODXCam")]


namespace CODTools
{
    public static class Cosmetics
    {
        public static HashSet<string> Bones = new HashSet<string>();
    }

    public class MarkCosmeticCommand : MPxCommand, IMPxCommand
    {

        override public void doIt(MArgList argl)
        {
            var SelectionList = new MSelectionList();
            MGlobal.getActiveSelectionList(SelectionList);

            if (SelectionList.DependNodes(MFn.Type.kJoint).Count() == 0)
            {
                MGlobal.displayInfo("[CODTools] Nothing selected!");
                return;
            }

            foreach (var Joint in SelectionList.DependNodes(MFn.Type.kJoint))
            {
                // Grab the controller
                var Path = CODXModel.GetObjectDagPath(Joint);
                var Controller = new MFnIkJoint(Path);

                // Create a new bone
                var TagName = CODXModel.CleanNodeName(Controller.name);

                MGlobal.displayInfo(string.Format("Marked {0} as Cosmetic Joint", TagName));

                // Add Bone to List
                Cosmetics.Bones.Add(TagName);
            }
        }
    }

    public class UnMarkCosmeticCommand : MPxCommand, IMPxCommand
    {

        override public void doIt(MArgList argl)
        {
            var SelectionList = new MSelectionList();
            MGlobal.getActiveSelectionList(SelectionList);

            if (SelectionList.DependNodes(MFn.Type.kJoint).Count() == 0)
            {
                MGlobal.displayInfo("[CODTools] Nothing selected!");
                return;
            }
                
            foreach (var Joint in SelectionList.DependNodes(MFn.Type.kJoint))
            {
                // Grab the controller
                var Path = CODXModel.GetObjectDagPath(Joint);
                var Controller = new MFnIkJoint(Path);

                // Create a new bone
                var TagName = CODXModel.CleanNodeName(Controller.name);

                MGlobal.displayInfo(string.Format("[CODTools] Unmarked {0} as Cosmetic Joint", TagName));

                // Remove Bone from List
                Cosmetics.Bones.Remove(TagName);
            }
        }
    }

    public class ClearMarkedCosmeticsCommand : MPxCommand, IMPxCommand
    {

        override public void doIt(MArgList argl)
        {
            // Clear all Cosmetic Bones
            Cosmetics.Bones.Clear();
            MGlobal.displayInfo("[CODTools] Cleared all Cosmetic Joints\n");
        }
    }

    // CODXCam
    public class XCamCommand : MPxCommand, IMPxCommand
    {

        override public void doIt(MArgList argl)
        {
            // First, get the current selection
            var SelectionList = new MSelectionList();
            MGlobal.getActiveSelectionList(SelectionList);

            uint index = 0;

            foreach (var Camera in SelectionList.DependNodes(MFn.Type.kCamera))
            {
                MDagPath mDagPath = new MDagPath();
                SelectionList.getDagPath(index, mDagPath);

                var Cam = new MFnCamera(mDagPath);
                var Transform = new MFnTransform(mDagPath);


                var WorldPosition = Transform.getTranslation(MSpace.Space.kWorld) * ( 1 / 2.54 );

                MGlobal.displayInfo(string.Format("[CODTools] Pos X: {0}, Pos X: {1}, Pos X: {2}", WorldPosition.x, WorldPosition.y, WorldPosition.z));

                var WorldRotation = new MQuaternion(MQuaternion.identity);
                Transform.getRotation(WorldRotation, MSpace.Space.kWorld);

                // EulerRotation = Radians
                var EulerRotation = WorldRotation.asEulerRotation;
                EulerRotation.reorderIt(MEulerRotation.RotationOrder.kXYZ);

                // Substract 90 degrees from it in radians else we are facing the wrong way!
                EulerRotation.x = EulerRotation.x - ((Math.PI / 180) * 90);
                EulerRotation.z = EulerRotation.z - ((Math.PI / 180) * 90);

                var WorldMatrix = EulerRotation.asMatrix;

                MGlobal.displayInfo(string.Format("[CODTools] Dir {0} {1} {2}", WorldMatrix[1, 0], WorldMatrix[1, 1], WorldMatrix[1, 2]));
                MGlobal.displayInfo(string.Format("[CODTools] Up {0} {1} {2}", WorldMatrix[2, 0], WorldMatrix[2, 1], WorldMatrix[2, 2]));
                MGlobal.displayInfo(string.Format("[CODTools] Right {0} {1} {2}", WorldMatrix[0, 0], WorldMatrix[0, 1], WorldMatrix[0, 2]));

                MGlobal.displayInfo(string.Format("[CODTools] Vertical FOV {0} ", Cam.verticalFieldOfView));
                MGlobal.displayInfo(string.Format("[CODTools] Horizontal FOV {0} ", Cam.horizontalFieldOfView));
                MGlobal.displayInfo(string.Format("[CODTools] Horizontal Film Aperture: {0} ", Cam.horizontalFilmAperture));
                MGlobal.displayInfo(string.Format("[CODTools] Vertical Film Aperture: {0} ", Cam.verticalFilmAperture));
                MGlobal.displayInfo(string.Format("[CODTools] Ratio: {0} ", Cam.aspectRatio));
                MGlobal.displayInfo(string.Format("[CODTools] Ortho: {0} ", Cam.orthoWidth));

                index++;
            }
        }

        private static MDagPath GetMDagPath(MObject Object)
        {
            var SelectionList = new MSelectionList();
            SelectionList.add(new MFnDagNode(Object).dagPath);

            var Result = new MDagPath();
            SelectionList.getDagPath(0, Result);

            return Result;
        }
    }

    

    /*internal static MDagPath GetObjectDagPath(MObject Object)
    {
        var SelectionList = new MSelectionList();
        SelectionList.add(new MFnDagNode(Object).fullPathName);

        var Result = new MDagPath();
        SelectionList.getDagPath(0, Result);

        return Result;
    }*/

}

