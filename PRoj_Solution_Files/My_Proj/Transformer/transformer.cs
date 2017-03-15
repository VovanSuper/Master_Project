using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;
using Master_Project.Core;

namespace M_P_4.Transformer
{
    class Transformer
    {
        private XmlDocument TMLDocument;
        private string WAVFile { get; set; }
        
        public XmlDocument transform()
        {
            //TODO: Transform the incoming WAV file into a compliant TML structure
            this.TMLDocument = null;
            return TMLDocument;
        
        }
        
        public Transformer(string WAVFile) 
        {
            this.WAVFile = WAVFile;
        }
        public Transformer()
        {
            this.WAVFile = null;
        }

        

        public string WAVintoASCIIstringOfBytes(string WAVFile)   //--To return string of bytes encoded to UTF-8
        {
            byte[] buff = this.openStream(WAVFile);
            Encoding encoding = Encoding.ASCII;
            string symbolickbuf = encoding.GetString(buff);
            return symbolickbuf;
        }

        
        public string WAVintoBaseHex(string WAVFile)   //--To return string of bytes encoded to UTF-8
        {
            byte[] buff = this.openStream(WAVFile);
            string symbolickbuf = BitConverter.ToString(buff).Replace("-","");
            return symbolickbuf;
        }

        

        private byte[] openStream(string someWAVFile)    //--opens file and ReadWAVFully() returns byte[]
        {
            Stream streamfile = File.OpenRead(someWAVFile);
            byte[] buf = ReadWAVFully(streamfile, streamfile.Length);
            return buf;
        }

        private static byte[] ReadWAVFully(Stream stream, long initialLength)   //--reads entire WAV file and returns byte[]
        {
            // if not WAV fully read  then put some customsize for buffer first            
            if (initialLength < 1)
            {
                initialLength = 49152;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        
        public string[] in2Dimarray(string source, int subStrsLength)   //--transforming string into array of substrings
        {
            string[] result = CustomSplit(source, subStrsLength).ToArray<string>();
            return result;
        }
        private static IEnumerable<string> CustomSplit(string source, int byLenght)   //--splits the String into a string[]
        {
            while (source.Length > byLenght)
            {
                yield return source.Substring(0, byLenght);
                source = source.Remove(0, byLenght);
            }
            if (source.Length > 0)
                yield return source;
        }
        



        //// ----------------alt in2Dimarray----------------------------------
        //private T[,] in2Dimarray<T>(T[] array, int byLenght)
        //{

        //    int n = array.Length % byLenght == 0 ? array.Length / byLenght : array.Length / byLenght + 1;
        //    var newArr = new T[byLenght, n];
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        int k = i % byLenght;
        //        int l = i / byLenght;
        //        newArr[k, l] = array[i];
        //    }

        //    return newArr;
        //}
        ////-----------------------------------------------------------------------------
        ////-----------------------------------------------------------------------------
    }
}
