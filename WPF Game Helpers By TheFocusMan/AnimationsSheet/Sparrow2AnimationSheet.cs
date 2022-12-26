using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web;
using System.Windows;
using System.Xml;

namespace WpfGame.AnimationsSheet
{
    public sealed class Sparrow2AnimationSheet : AnimationSheet
    {
        private string _xmlpath;

        public Sparrow2AnimationSheet(string texture, string xml, double framerate = 30)
            : base(texture, framerate)
        {
            _xmlpath = xml;
            BeginInit();
            EndInit();
        }

        public static string EscapeXMLValue(string xmlString)
        {

            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            if (!Sparrow2XmlCache.xmlfiles.Contains(xmlString))
            {
                Sparrow2XmlCache.xmlfiles.Add(xmlString);
                var first = xmlString.IndexOf('"');
                while (first != -1)
                {
                    first++;
                    var last = xmlString.IndexOf('"', first);
                    var str = xmlString[first..last];
                    var str1 = str.Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;");
                    if (str != str1)
                    {
                        xmlString = xmlString.Remove(first, last - first).Insert(first, str1);
                        last = xmlString.IndexOf('"', first); // bug fix
                    }

                    first = xmlString.IndexOf('"', last + 1);
                }
                Sparrow2XmlCache.xmlescapedfiles.Add(xmlString);
                return xmlString;
            }
            else
            {
                return Sparrow2XmlCache.xmlescapedfiles[Sparrow2XmlCache.xmlfiles.IndexOf(xmlString)];
            }
        }

        public static string UnescapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");


            return xmlString.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
        }

        internal protected override void ReadAmimation()
        {
            using var reader1 = new StringReader(EscapeXMLValue(File.ReadAllText(_xmlpath)));
            using var reader = XmlReader.Create(reader1, new XmlReaderSettings());
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "SubTexture")
                    {
                        string name = reader.GetAttribute("name");
                        int num = int.Parse(name[^4..]);
                        name = name.Remove(name.Length - 4).Trim();
                        if (!_rect.ContainsKey(name))
                            _rect.Add(name, new AnimationSubTextureDataCollection());

                        double x = Mathf.TryParse(reader.GetAttribute("x"));
                        double y = Mathf.TryParse(reader.GetAttribute("y"));
                        double width = Mathf.TryParse(reader.GetAttribute("width"));
                        double height = Mathf.TryParse(reader.GetAttribute("height"));
                        double frameX = Mathf.TryParse(reader.GetAttribute("frameX"));
                        double frameY = Mathf.TryParse(reader.GetAttribute("frameY"));
                        double frameWidth = Mathf.TryParse(reader.GetAttribute("frameWidth"));
                        double frameHeight = Mathf.TryParse(reader.GetAttribute("frameHeight"));
                        var rect = new Rect(x, y, width, height);
                        _rect[name].Add(new AnimationSubTextureData(rect, new Rect(frameX, frameY, frameWidth, frameHeight), num));
                        ResetAnimationSort(name);
                    }
                }
            }
        }
    }

    internal static class Sparrow2XmlCache
    {
        public static List<string> xmlfiles = new List<string>();
        public static List<string> xmlescapedfiles = new List<string>();
    }

}
