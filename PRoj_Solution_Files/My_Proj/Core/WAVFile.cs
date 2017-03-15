/// <summary>
///Cloned from https://ovsyukov.visualstudio.com/DefaultCollection/_git/WAV%20sound%20files%20to%20Transducer%20Markup%20Language%20demonstrator/branches
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Reflection;

namespace Master_Project.Core
{
    public class WAVFile : IDisposable
    {
        private static WAVFile _tmlInstance;
        public static WAVFile getTmledInstance(string file)
        {
            _tmlInstance = _tmlInstance ?? new WAVFile(file);
            return _tmlInstance;
        }

        private WAVFileReader wavReader;
        private Structs.chunkWav riffSec;
        private Structs.chunkFmt fmtSec = null;
        private Structs.chunkFact factSec = null;
        private Structs.chunkList listSec = null;
        private Structs.chunkData dataSec = null;
        private enum PhoneMode { Mono, Stereo };
        private PhoneMode phoneMode;
        public string fileName { get; private set; }
        private int intervalMilisecs = 10;
        private int franesPer10Milisecs;

        private WAVFile(string _fileName)
        {
            this.fileName = _fileName;
            wavReader = new WAVFileReader(fileName);
            riffSec = wavReader.wavChunk;
            fmtSec = wavReader.fmtChunk;
            factSec = wavReader.factChunk;
            listSec = wavReader.listChunk;
            dataSec = wavReader.dataChunk;
            franesPer10Milisecs = (int)fmtSec.sampleRate / 1000 * intervalMilisecs;
        }

        public XComment createWavInfo()
        {
            switch (wavReader.fmtChunk.numChannels)
            {
                case 1:
                    phoneMode = PhoneMode.Mono;
                    break;
                case 2:
                    phoneMode = PhoneMode.Stereo;
                    break;
                default:
                    throw new ArgumentException(@"audioPhone mode is not supported (should be mono/stereo PCM file)");
            }
            return new XComment(string.Format("\n/********************************************************" +
                                              "\n/********************************************************" +
                                              "\n/********************************************************\n" +
                                              "***    Created by:       Vladimir Ovsyukov\n" +
                                              "***    date:             {6}\n" +
                                              "***    version:          0.0.0.1 \n\n" +
                
                                              "***Cloned from https://ovsyukov.visualstudio.com/DefaultCollection/_git/WAV%20sound%20files%20to%20Transducer%20Markup%20Language%20demonstrator/branches " +
                
                                              "/**********************************************************\n" +
                                              "file: {0}\n" +
                                              "phoneMode: {1}\n" +
                                              "avarage number of bytes per second: {2}\n" +
                                              "number of bytes for one sample including all channels: {3}\n" +
                                              "number of bytes per sample slice: {4}\n" +
                                              "number of bits used to define each sample: {5}" +
                                              "\n/********************************************************" +
                                              "\n/********************************************************" +
                                              "\n/********************************************************\n",
                                                          fileName, phoneMode, wavReader.fmtChunk.sampleRate,
                                                          wavReader.fmtChunk.byteRate, wavReader.fmtChunk.blockAlign,
                                                          wavReader.fmtChunk.bitsPerSample, System.DateTime.Now));
        }

        public void createProcessesXML(out List<XElement> processes, out List<XElement> clusterDescriptions, out List<XElement> dataPart)  //Returns list<XElement> of processes, ClusterDescriptions and dataSections chunks
        {
            XElement riffSecProcess = new XElement("process",
                new XAttribute("name", "PROCESS_1"),
                new XAttribute("uid", "SYS01_PCS"),
                new XElement("input",
                    new XElement("procDataSet",
                        new XAttribute("name", "WAV_FILE_RIFF_SECTION"),
                        this.createProcessDataSet(riffSec)
                                )
                            )
            );
            XElement fmtSecProcess = (fmtSec != null) ? (new XElement("process",
                new XAttribute("name", "PROCESS_2"),
                new XAttribute("uid", "SYS02_PCS"),
                new XElement("input",
                    new XElement("procDataSet",
                        new XAttribute("name", "WAV_FILE_FMT_SECTION"),
                        this.createProcessDataSet(fmtSec)
                                )
                            )
                )) : null;
            XElement factSecProcess = (factSec != null) ? (new XElement("process",
                new XAttribute("name", "PROCESS_3"),
                new XAttribute("uid", "SYS03_PCS"),
                new XElement("input",
                    new XElement("procDataSet",
                        new XAttribute("name", "WAV_FILE_FACT_SECTION"),
                        this.createProcessDataSet(factSec)
                                )
                            )
                )) : null;
            XElement listSecProcess = (listSec != null) ? (new XElement("process",
                new XAttribute("name", "PROCESS_4"),
                new XAttribute("uid", "SYS04_PCS"),
                new XElement("input",
                    new XElement("procDataSet",
                        new XAttribute("name", "WAV_FILE_LIST_SECTION"),
                        this.createProcessDataSet(listSec)
                                )
                            )
                )) : null;
            XElement dataSecProcess = (dataSec != null) ? (new XElement("process",
                new XAttribute("name", "PROCESS_5"),
                new XAttribute("uid", "SYS05_PCS"),
                new XElement("input",
                    new XElement("procDataSet",
                        new XAttribute("name", "WAV_FILE_DATA_SECTION"),
                        this.createProcessDataSet(dataSec)
                                 )
                            ))
                ) : null;

            List<XElement> clustDescs = new List<XElement>();
            List<XElement> riffSecClusterDesc = this.createClusterDescSec(riffSecProcess, riffSec);
            if (riffSecClusterDesc != null)
                riffSecClusterDesc.ForEach(e => clustDescs.Add(e));
            List<XElement> fmtSecClusterDesc = this.createClusterDescSec(fmtSecProcess, fmtSec);
            if (fmtSecClusterDesc != null)
                fmtSecClusterDesc.ForEach(e => clustDescs.Add(e));
            List<XElement> factSecClusterDesc = this.createClusterDescSec(factSecProcess, factSec);
            if (factSecClusterDesc != null)
                factSecClusterDesc.ForEach(e => clustDescs.Add(e));
            List<XElement> listSecClusterDesc = this.createClusterDescSec(listSecProcess, listSec);
            if (listSecClusterDesc != null)
                listSecClusterDesc.ForEach(e => clustDescs.Add(e));
            List<XElement> dataSecClusterDesc = this.createClusterDescSec(dataSecProcess, dataSec, true);
            if (dataSecClusterDesc != null)
                dataSecClusterDesc.ForEach(e => clustDescs.Add(e));
                        
            processes = new List<XElement>() { riffSecProcess, fmtSecProcess, factSecProcess, listSecProcess, dataSecProcess };
            clusterDescriptions = clustDescs;

            
            dataPart =  this.createAllDataParts(dataSec).ToList<XElement>() ;
            
        }

        private IEnumerable<XElement> createAllDataParts(Master_Project.Core.Structs.IChunk dataSec)
        {
            if (dataSec is Structs.chunkData)
            {
                int time = 10;
                int inter = 0;
                var curTimeStamp = DateTime.Now;
                List<XElement> res = new List<XElement>(capacity: (dataSec as Structs.chunkData).dataFramesInBAse64.ToList().Count + 1);
                res.Add(new XElement("data",
                           new XAttribute("clk", time),
                           new XAttribute("ref", "SYS01_TIMESTAMP"),
                           new XText(curTimeStamp.ToString())));
                //var localTime = DateTime.Now.Millisecond;
                //var intervalToAdd = (dataSec as Structs.chunkData).
                (dataSec as Structs.chunkData).dataFramesInBAse64.ToList<string>().ForEach(f =>
                    {
                        if (inter++ > franesPer10Milisecs)
                        {
                            time++;
                            inter = 0;
                        }
                        res.Add(new XElement("data",
                                   new XAttribute("clk", time),
                                   new XAttribute("ref", dataSec.getSelfType().GetProperty("dataID").GetValue(dataSec, null).ToString()),
                                   new XText(f)
                              ));
                    });
                res.Add(new XElement("data",
                           new XAttribute("clk", time),
                           new XAttribute("ref", "SYS01_TIMESTAMP"),
                           new XText(curTimeStamp.AddMilliseconds(time).ToString())));
                return res;
            }
            return null;
        }


        private XElement createProcessDataSet(object unitName)
        {
            XElement elem = new XElement("dataSet");
            unitName.GetType().GetProperties().ToList().ForEach(new Action<PropertyInfo>(
                p => elem.Add(new XElement("dataUnit",
                                      new XElement("name", p.Name),
                                      (p.PropertyType == typeof(System.String)) ? new XElement("dataType", new XText("text")) :
                                                                                   new XElement("dataType", new XText("number"))
                                           )
                )));
            return elem;
        }


        private List<XElement> createClusterDescSec(XElement someWavFileSec, object unitName, bool isDataSection = false)
        {
            if (someWavFileSec == null)
                return null;

            List<XElement> clusterDescsOfProcess = new List<XElement>();
            someWavFileSec.Descendants("dataSet").Single().Descendants("dataUnit")
                .ToList().ForEach(new Action<XElement>(e =>
                {
                    XElement clusterDesc = new XElement("clusterDesc",
                        new XAttribute("name", someWavFileSec.Attribute("name").Value),
                        new XAttribute("uidRef", e.Element("name").Value),
                        new XElement("description", string.Format("Cluster description of {0} dataUnit in {1} dataChunk", e.Element("name")
                                                                                         .Value,
                                                                                         someWavFileSec.Attribute("name").Value)),
                        new XElement("idMapping",
                            isDataSection ? new XElement("tapPointUidRef") :
                                            new XElement("tapPointUidRef", e.Element("name").Value),
                            isDataSection ? new XElement("localId", e.Element("name").Value) :
                                            new XElement("localId")
                            ),

                        new XElement("clusterProperties",
                            new XElement("clusterSize", unitName.GetType().GetProperties()[1].GetValue(unitName, null))
                            ),
                        new XElement("dataUnitEncoding",
                            isDataSection ? null :
                                            new XElement("beginTextDelimeter"),
                            isDataSection ? new XElement("dataType", "binBlob") :
                                            new XElement("dataType", "text"),

                            isDataSection ? new XElement("handleAsType", "unsignedByte") :
                                            new XElement("handleAsType", "string"),
                            isDataSection ? null :
                                            new XElement("endTextDelimeter")
                            ),
                        new XElement("numCfInCluster", someWavFileSec.Descendants("dataSet").Single().Descendants("dataUnit").ToList().Count),
                        isDataSection ? new XElement("endian", "little") :
                                        new XElement("endian", "big"),

                            isDataSection ? new XElement("encode", "unsignInt") :
                                            new XElement("encode", "utf-8"),
                        new XElement("transSeq",
                            isDataSection ? new XElement("seqOfThisDataStruct", "seqOfBitsInUnit") :
                                            new XElement("seqOfThisDataStruct", "seqOfUnitsInSecs"),
                            isDataSection ? new XElement("sequence", "256...244800") :
                                            null
                            )
                                                       );
                    clusterDescsOfProcess.Add(clusterDesc);
                })
                );


            return clusterDescsOfProcess;
        }

        //private List<XElement> createRawDataSecs()
        //{



        //    return null;
        //}
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                wavReader.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
