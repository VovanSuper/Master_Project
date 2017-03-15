/// <summary>
///Cloned from https://ovsyukov.visualstudio.com/DefaultCollection/_git/WAV%20sound%20files%20to%20Transducer%20Markup%20Language%20demonstrator/branches
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Master_Project.Core
{
    
    [StructLayout(LayoutKind.Sequential)]
    public class Structs
    {
        public interface IChunk 
        {
            Type getSelfType();
        }

        public class chunkWav : IChunk
        {
            public string riffID { get; set; }          // 4 bytes = "riff"
            public uint riffSize { get; set; }          // size of chunk
            public string wavID { get; set; }           // "wave"
            public Type getSelfType()
            {
                return this.GetType();
            }
        }
        public class chunkFmt : IChunk
        {
            public string fmtID { get; set; }            // 4 bytes! = "fmt_"
            public uint fmtSize { get; set; }            // should be  16 for MS_PCM
            public ushort audioFormat { get; set; }      // should be 1 for MS_PCM
            public ushort numChannels { get; set; }      // Mono = 1, Stereo = 2, etc
            public uint sampleRate { get; set; }         // samples per second, e.g 8000, 44100
            public uint byteRate { get; set; }           // bytes per second.  == SampleRate * NumChannels * BitsPerSample/8
            public ushort blockAlign { get; set; }       // amount of bytes for one sample  == NumChannels * BitsPerSample/8; The number of bytes for one sample including all channels.
            public ushort bitsPerSample { get; set; }    // 8 bits = 8, 16 bits = 16, etc
            public Type getSelfType()
            {
                return this.GetType();
            }
        }
        public class chunkFact : IChunk
        {
            public string factID;    	//Four bytes: "fact"
            public uint factSize;    	//Length of header
            public uint numSamples;    	//Number of audio frames;
            public Type getSelfType()
            {
                return this.GetType();
            }
        }
        public class chunkList : IChunk
        {
            public string listID;    // "LIST"
            public uint listSize;
            public string typeID;   // 4 bytes "adtl" or "INFO"
            public Type getSelfType()
            {
                return this.GetType();
            }
        }
        public class chunkData : IChunk
        {
            public string dataID { get; set; }            // "data"
            public uint dataSize { get; set; }
            public long dataInFilePos { get; set; }	      // Position of data chunk in file
            public double dSecLength { get; set; }	      // Length of audio in seconds
            public uint dwNumSamples { get; set; }        // Number of audio frames
           // public List<string> leftBase64Channel { get; set; }
           // public List<string> rightBase64Channel { get; set; }
            public List<string> dataFramesInBAse64 { get; set; } // All Audio Frames Base64-Encoded
            public Type getSelfType()
            {
                return this.GetType();
            }
        }        
    }

}
