using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PixelPerfect.Cutscene
{
    class Serializer
    {
        public static CutsceneState Deserialize(string xmlfile)
        {
            CutsceneState cutsceneState;

            using (Stream stream = File.Open(xmlfile, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CutsceneState));
                cutsceneState = (CutsceneState)xmlSerializer.Deserialize(stream);
            }
            return cutsceneState;
        }

        public static void Serialize(CutsceneState cutscene, string xmlfile)
        {
            using (Stream stream = File.Open(xmlfile, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CutsceneState));

                xmlSerializer.Serialize(stream, cutscene);
            }
        }
    }
}
