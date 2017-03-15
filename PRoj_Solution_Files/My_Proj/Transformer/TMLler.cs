using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using Master_Project.Core;
using System.Windows;
using System.Reflection;
using System.Timers;

namespace Master_Project.Transformer
{
    public static class TMLler
    {
        private static WAVFile wav;

        public static List<XElement> clusterDescriptions;

        public static XDocument createDoc(string _fileName)
        {
            try
            {
                wav = WAVFile.getTmledInstance(_fileName);
                XDocument document = new XDocument(new XDeclaration("1.0", "ASCII", String.Empty),
                new XComment(wav.createWavInfo()),
                   new XElement("tml",
                       new XElement("system",
                           new XAttribute("name", "SYS01"),
                           new XElement("sysClk",
                                new XElement("period",
                                    new XElement("value", new XText("0.001"))
                                            )
                                       ),
                           new XElement("processes"),
                           new XElement("clusterDescriptions")
                                   ),
                       new XComment("Strart of data stream")
                               )
               );
                List<XElement> processes, clusterDescriptions, dataPart;
                wav.createProcessesXML(out processes, out clusterDescriptions, out dataPart);
                processes.ForEach( e => document.Root.Descendants("processes").Single().Add(e) );
                if (clusterDescriptions != null)
                    clusterDescriptions.ForEach( e => document.Root.Descendants("clusterDescriptions").Single().Add(e) );

                if (dataPart != null)
                    document.Root.Add(dataPart);
                                
                return document;
            }

            catch
            {
                return null;
                throw;
            }
            
        }

        
    }

    
}
