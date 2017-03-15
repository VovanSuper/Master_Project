using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Master_Project.Core
{
    class WAVFileReader : IDisposable
    {
        public BinaryReader reader;
        public Structs.chunkWav wavChunk { get; private set; }
        public Structs.chunkFmt fmtChunk { get; private set; }
        public Structs.chunkFact factChunk { get; private set; }
        public Structs.chunkList listChunk { get; private set; }
        public Structs.chunkData dataChunk { get; private set; }
        long fileLength;

        public WAVFileReader(string wavFileName)
        {
            reader = new BinaryReader(new FileStream(wavFileName, FileMode.Open, FileAccess.Read));
            fileLength = (long)reader.BaseStream.Length;
            this.fillStructs();
            if (fmtChunk.fmtSize != 16 || fmtChunk.audioFormat != 1)
                throw new ArgumentException("File format is not supported (non Microsoft PCM WAV format)");
        }


        #region Chunks_Filler
        private Structs.chunkWav readWavChunk()
        {
            return wavChunk = new Structs.chunkWav
                {
                    riffID = new string(reader.ReadChars(4)),
                    riffSize = reader.ReadUInt32() + 8,
                    wavID = new string(reader.ReadChars(4))
                };
        }
        private Structs.chunkFmt readFmtChunk()
        {
            return fmtChunk = new Structs.chunkFmt()
            {
                fmtID = "fmt ",
                fmtSize = reader.ReadUInt32(),
                audioFormat = reader.ReadUInt16(),
                numChannels = reader.ReadUInt16(),
                sampleRate = reader.ReadUInt32(),
                byteRate = reader.ReadUInt32(),
                blockAlign = reader.ReadUInt16(),
                bitsPerSample = reader.ReadUInt16()
            };
        }
        private Structs.chunkFact readFactChunk()
        {
            return factChunk = new Structs.chunkFact()
            {
                factID = "fact",
                factSize = reader.ReadUInt32(),
                numSamples = reader.ReadUInt32()
            };

        }
        private Structs.chunkList readListChunk()
        {
            return listChunk = new Structs.chunkList
            {
                listID = "LIST",
                listSize = reader.ReadUInt32(),
                typeID = new string(reader.ReadChars(4))
            };
        }
        private Structs.chunkData readDataChunk()
        {
            dataChunk = new Structs.chunkData();
            dataChunk.dataID = "data";
            dataChunk.dataSize = reader.ReadUInt32();
            dataChunk.dataInFilePos = reader.BaseStream.Position;
            dataChunk.dwNumSamples = (factChunk != null) ? factChunk.numSamples :
                                                Convert.ToUInt32(dataChunk.dataSize / (fmtChunk.bitsPerSample / 8 * fmtChunk.numChannels));
            dataChunk.dSecLength = ((double)dataChunk.dataSize / (double)fmtChunk.byteRate);            
            dataChunk.dataFramesInBAse64 = this.ReadAllAudioFrames().ToList<string>();
            return dataChunk;
        }

        private IEnumerable<string> ReadAllAudioFrames()
        {
            for (int i = 0; i < dataChunk.dataSize; i += fmtChunk.blockAlign)
            {
                yield return readBlockAlignedBase64(fmtChunk.blockAlign);
            }
        }
        #endregion
        
        //private void FillChannelsArrays(ref List<string> LeftChannel, ref List<string> RightChannel)
        //{
        //    var numChann = fmtChunk.numChannels;
        //    var dataSize = dataChunk.dataSize;
        //    var blockAlign = fmtChunk.blockAlign;
        //    var BitsPerSample = fmtChunk.bitsPerSample;
        //    var numSamples = (dataSize * 8) / (numChann * BitsPerSample);
        //    if (numChann == 1)
        //    {
        //        RightChannel = null;
        //        LeftChannel = new List<string>();
        //        for (int i = 0; (i < dataSize) && (i < fileLength); i = i + blockAlign)
        //        {
        //            string curVal = readBlockAlignedBase64(blockAlign);
        //            LeftChannel.Add(curVal);
        //        }
        //    }
        //    else
        //    {
        //        var a = this.GetPosition();
        //        RightChannel = new List<string>();
        //        LeftChannel = new List<string>();
        //        int j = 0;
        //        for (int i = 0; i < dataSize; i = i + blockAlign)
        //        {
        //            j++; var b = this.GetPosition();
        //            string curVal = readBlockAlignedBase64(blockAlign);
        //            if (j % 2 == 0)
        //                RightChannel.Add(curVal);
        //            else
        //                LeftChannel.Add(curVal);
        //        }
        //        var b1 = this.GetPosition();
        //    }

        //}

        #region Auxiliary
        private string readBlockAlignedBase64(int numBytes)
        {
            return Convert.ToBase64String(reader.ReadBytes(numBytes));
        }

        private long GetPosition()
        {
            return reader.BaseStream.Position;
        }

        private string GetChunkName()
        {
            return new string(reader.ReadChars(4));
        }
        private void AdvanceToNext()
        {
            long NextOffset = (long)reader.ReadUInt32();             //Get next chunk offset                                                         
            reader.BaseStream.Seek(NextOffset, SeekOrigin.Current);  //Seek to the next offset from current position
        }

        public void Dispose()
        {
            if (reader != null)
                reader.Close();
        }


        private void fillStructs()
        {
            wavChunk = this.readWavChunk();
            string temp = String.Empty;
            try
            {
                while (this.GetPosition() < fileLength)
                {
                    temp = this.GetChunkName();
                    if (temp == "fmt ")
                    {
                        fmtChunk = this.readFmtChunk();
                        if (this.GetPosition() + fmtChunk.fmtSize == fileLength)
                            break;
                    }
                    else if (temp == "fact")
                    {
                        factChunk = this.readFactChunk();
                        if (this.GetPosition() + factChunk.factSize == fileLength)
                            break;
                    }
                    else if (temp.ToLower() == "list")
                    {
                        listChunk = this.readListChunk();
                        if (this.GetPosition() + listChunk.listSize == fileLength)
                            break;
                    }
                    else if (temp == "data")
                    {
                        dataChunk = this.readDataChunk();
                        if (this.GetPosition() + dataChunk.dataSize == fileLength)
                            break;
                    }
                    else
                    {
                        this.AdvanceToNext();  // Provides the required skipping of unsupported chunks.
                    }
                }
            }
            catch (EndOfStreamException)
            {
            }
        }




        #endregion
    }
   
}
