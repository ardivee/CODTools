using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CODTools
{
    internal class Axis
    {
        public List<double> x { get; set; }
        public List<double> y { get; set; }
        public List<double> z { get; set; }
    }

    internal class Align
    {
        public string tag { get; set; }
        public List<double> offset { get; set; }
        public Axis axis { get; set; }
    }

    internal class XCamNoteTrack
    {
        public string name { get; set; }
        public int frame { get; set; }
    }

    internal class CameraAnimation
    {
        public int frame { get; set; }
        public List<double> origin { get; set; }
        public List<double> dir { get; set; }
        public List<double> up { get; set; }
        public List<double> right { get; set; }
        public double flen { get; set; }
        public double fov { get; set; }
        public double fdist { get; set; }
        public double fstop { get; set; }
        public int lense { get; set; }
    }

    internal class Camera
    {
        public string name { get; set; }
        public int index { get; set; }
        public string type { get; set; }
        public string aperture { get; set; }
        public List<double> origin { get; set; }
        public List<double> dir { get; set; }
        public List<double> up { get; set; }
        public List<double> right { get; set; }
        public double flen { get; set; }
        public double fov { get; set; }
        public double fdist { get; set; }
        public double fstop { get; set; }
        public int lense { get; set; }
        public double aspectratio { get; set; }
        public double nearz { get; set; }
        public double farz { get; set; }
        public List<CameraAnimation> animation { get; set; }
    }


    internal class XCam
    {
        public int version { get; set; }
        public string scene { get; set; }
        public Align align { get; set; }
        public int framerate { get; set; }
        public int numframes { get; set; }
        public List<string> targetModelBoneRoots { get; set; }
        public List<Camera> cameras { get; set; }
        public List<string> cameraSwitch { get; set; }
        public List<XCamNoteTrack> notetracks { get; set; }

        public void WriteExport(string FilePath)
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public XCam()
        {
            this.version = 1;

            // Not sure how this works yet so we set default values
            this.align = new Align
            {
                tag = "tag_align",
                offset = new List<double> { 0.0, 0.0, 0.0 },
                axis = new Axis
                {
                    x = new List<double> { 0.0, -1.0, 0.0 },
                    y = new List<double> { 1.0, 0.0, 0.0 },
                    z = new List<double> { 0.0, 0.0, 1.0 }
                }
            };

            // Not sure about this either, so we initialize an empty list
            this.targetModelBoneRoots = new List<string>();

            // Hardcoded for now
            this.framerate = 30;

            this.cameras = new List<Camera>();
            this.cameraSwitch = new List<string>();
            this.notetracks = new List<XCamNoteTrack>();
        }
    }
}
