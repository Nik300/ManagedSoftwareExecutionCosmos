using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Emulation
{
    public class Stack<T>
    {
        public List<T> Data { get; private set; }
        public Stack()
        {
            Data = new List<T>();
        }

        /// <summary>
        /// Push one single element to the stack
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public void Push(T element)
        {
            Data.Add(element);
        }

        /// <summary>
        /// Gather the last element in the stack
        /// </summary>
        /// <returns></returns>
        public bool Try_Pop(ref T val)
        {
            if (Data.Count == 0) return false;
            val = Data[Data.Count - 1];
            List<T> dat = new List<T>();
            if (Data.Count - 1 != -1)
                for (int i = 0; i < Data.Count - 1; i++)
                    dat.Add(Data[i]);

            Data = dat;

            dat = null;

            return true;
        }

        /// <summary>
        /// Clears entire stack
        /// </summary>
        public void Clear() { Data.Clear(); }
    }
    public class VirtualMemory
    {
        //public static readonly int Size; 
        public List<int> Data { get; private set; }
        public Stack<int> Stack { get; private set; }

        public VirtualMemory()
        {
            Data = new List<int>();
            Stack = new Stack<int>();
        }

        /// <summary>
        /// Clears entire memory
        /// </summary>
        public void Clear() { Data.Clear(); }

        /// <summary>
        /// Add single value to ram
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddInt(int data = 0)
        {
            Data.Add(data);
            return true;
        }

        /// <summary>
        /// Add single 8-bit value to ram
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddChar(byte data = 0)
        {
            List<byte> toAdd = new List<byte>();

            toAdd.Add(data);
            toAdd.Add(0x00);
            toAdd.Add(0x00);
            toAdd.Add(0x00);

            Data.Add(BitConverter.ToInt32(toAdd.ToArray(), 0));
            return true;
        }

        /// <summary>
        /// Add single 8-bit value to ram
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddChar(char data = '\0')
        {
            List<byte> toAdd = new List<byte>();

            toAdd.Add((byte)data);
            toAdd.Add(0x00);
            toAdd.Add(0x00);
            toAdd.Add(0x00);

            Data.Add(BitConverter.ToInt32(toAdd.ToArray(), 0));
            return true;
        }

        /// <summary>
        /// Add array to memory
        /// </summary>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        public bool AddArray(List<int> dataArray)
        {
            foreach (int b in dataArray)
            {
                Data.Add(b);
            }
            return true;
        }

        /// <summary>
        /// Add specified amount of integers to memory
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public bool AddAmount(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Data.Add(0x00);
            }
            return true;
        }

        /// <summary>
        /// Read single 8-bit value from memory
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public byte ReadChar(int addr)
        {
            List<byte> data = new List<byte>();
            foreach (byte c in BitConverter.GetBytes(Data[addr]))
            {
                data.Add(c);
            }
            return data[0];
        }
        
        public short ReadShort(int addr)
        {
            List<byte> data = new List<byte>();
            foreach (byte b in BitConverter.GetBytes(Data[addr]))
            {
                data.Add(b);
            }
            return (short)((data[0] << 8) | data[1]);
        }

        /// <summary>
        /// Read single value from memory
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public int ReadInt32(int addr)
        {
            List<byte> bdata = new List<byte>();
            foreach(byte b in BitConverter.GetBytes(Data[addr]))
            {
                bdata.Add(b);
            }
            bdata.Reverse();
            int data = BitConverter.ToInt32(bdata.ToArray(), 0);
            return data;
        }

        /// <summary>
        /// Read a range of bytes from memory
        /// </summary>
        /// <param name="addr"></param> <param name="len"></param>
        /// <returns></returns>
        public List<int> ReadRange(int addr, int len)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < len; i++) { output.Add(Data[addr + i]); }
            return output;
        }

        /// <summary>
        /// Insert a single value into a location in memory
        /// </summary>
        /// <param name="addr"></param> <param name="data"></param>
        /// <returns></returns>
        public bool InsertInt32(int addr, int data)
        {
            if (addr > Data.Count) { return false; }
            Data.Insert(addr, data);
            return true;
        }

        /// <summary>
        /// Write a single value into a location in memory
        /// </summary>
        /// <param name="addr"></param> <param name="data"></param>
        /// <returns></returns>
        public bool WriteInt32(int addr, int data)
        {
            List<byte> dat = new List<byte>();
            foreach (byte b in BitConverter.GetBytes(data))
            {
                dat.Add(b);
            }
            dat.Reverse();
            data = BitConverter.ToInt32(dat.ToArray(), 0);
            Data[addr] = data;
            return true;
        }

        /// <summary>
        /// Write a range of data into memory
        /// </summary>
        /// <param name="addr"></param> <param name="length"></param> <param name="data"></param>
        /// <returns></returns>
        public bool WriteRange(int addr, int length, List<int> data)
        {
            for (int i = 0; i < length; i++)
            {
                WriteInt32(addr + i, data[i]);
            }
            return true;
        }

        /// <summary>
        /// Write character value to memory
        /// </summary>
        /// <param name="addr"></param> <param name="character"></param>
        /// <returns></returns>
        public bool WriteChar(int addr, char c)
        {
            Data[addr] = (byte)c;
            return true;
        }

        /// <summary>
        /// Write string(character array) to memory
        /// </summary>
        /// <param name="addr"></param> <param name="text"></param>
        /// <returns></returns>
        public bool WriteString(int addr, string txt)
        {
            for (int i = 0; i < txt.Length; i++)
            {
                Data[addr + i] = txt[i];
            }
            return true;
        }
        /// <summary>
        /// Clears The specified address in memory
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public bool ClearInt32(int addr)
        {
            Data[addr] = 0;
            return true;
        }
    }
}
