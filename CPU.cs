using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace ZCPU
{
    public class CPU
    {
        ///<summary>
        ///System initted
        //</summary>
        public bool init = false;
        /// <summary>
        /// Locator to network
        /// </summary>
        public Network Network;
        /// <summary>
        /// RAM array
        /// </summary>
        public long[] RAM { get; private set; }
        ///<summary>
        ///Size of RAM in KB.
        ///</summary>
        public long bitsize { get; private set; }
        /// <summary>
        /// The List of reserved indexes
        /// </summary>
        public long?[] RIL;

        ///<summary>
        ///Program counter
        ///</summary>
        UInt64 pc = 0;

        ///<summary>
        ///Halt the system.
        ///</summary>
        public void hlt()
        {
            memclean(0, bitsize);
            RAM = null;
            while (true) {}
        }

        ///<summary>
        ///CPU clock cycle
        ///</summary>
        public void ClockCycle() {
            if(!init){
                Console.WriteLine("Tried to cycle while CPU isn't running");
                Environment.Exit(1);
            }
            UInt64 CurrentInstruction = RAM[pc];
            switch(CurrentInstruction) {
                case 0x00:
                    //Console.WriteLine("nop");
                    pc++;
                case 0x01:
                    //Console.WriteLine("hlt");
                    hlt();
                case 0x02:
                    add(RAM[pc + 1], RAM[pc + 2], RAM[pc + 3]);
                    //Console.WriteLine("add");
                    pc += 6;
                case 0x03:
                    sub(RAM[pc +1], RAM[pc + 2], RAM[pc + 3]);
                    //Console.WriteLine("sub");
                    pc += 6;
                case 0x04:
                    mul(RAM[pc + 1], RAM[pc + 2], RAM[pc + 3]);
                    //Console.WriteLine("mul");
                    pc += 6;
                case 0x05:
                    div(RAM[pc + 1], RAM[pc + 2], RAM[pc + 3]);
                    //Console.WriteLine("div");
                    pc += 6;
                case 0x06:
                    pow(RAM[pc + 1], RAM[pc + 2], RAM[pc + 3]);
                    //Console.WriteLine("pow");
                    pc += 6;
                case 0x07:
                    sqrt(RAM[pc + 1], RAM[pc + 2]);
                    //Console.WriteLine("sqrt");
                    pc += 3;
                default:
                    Console.WriteLine("Skipping uknown instruction " + pc);
                    pc++;
            }
        }
        ///<summary>
        ///Clean memory from startIndex to endIndex
        ///<paramref name="startIndex">Start of index to clean</paramref>
        ///<paramref name="endIndex">End index to clean</paramref>
        ///</summary>
        public void memclean(int startIndex, long endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                setMemLoc(i, 0);
            }
        }

        /// <summary>
        /// Read index.
        /// </summary>
        /// <param name="index">Index to read</param>
        /// <returns>Value of index (int)</returns>
        public long rdI(long index)
        {
            if (index < 0 || index > bitsize) { return 0; }
            return RAM[index];
        }
        /// <summary>
        /// Reserves a certain index
        /// </summary>
        /// <param name="i">The index to reserve</param>
        public void ReserveIndex(long i)
        {
            if (i >= bitsize) { return; }
            RIL[i] = RAM[i];
            RAM[i] = 0;
        }
        /// <summary>
        /// Unreserves an index
        /// </summary>
        /// <param name="i">The index to unreserve</param>
        public void UnreserveIndex(long i)
        {
            long? a = RIL[i];
            RIL[i] = null;
            RAM[i] = (long)a;
        }
        /// <summary>
        /// Checks if a certain index is reserved
        /// </summary>
        /// <param name="i">the index to check</param>
        /// <returns></returns>
        public bool IsReserved(long i)
        {
            return RIL[i] != null;
        }
        /// <summary>
        /// Reads an array of indexes
        /// </summary>
        public long[] rd(long sindex, long eindex)
        {
            long[] tmp = new long[eindex];
            for (long i = sindex; i < eindex; i++)
            {
                tmp[i] = RAM[i];
            }
            return tmp;
        }
        /// <summary>
        /// Pointer to the display class
        /// </summary>
        public Display Display;
        /// <summary>
        /// Reads from starting index untill the end of the RAM (unless its 0, use at your own risk)
        /// </summary>
        /// <param name="sindex">Starting index, Default: 0</param>
        /// <returns></returns>
        public long[] rd(long sindex = 0)
        {
            var tmp = new List<long>();
            for (long i = sindex; i < bitsize; i++)
            {
                if (RAM[i] == 0)
                {
                    break;
                }
                tmp.Add(RAM[i]);
            }
            return tmp.ToArray();
        }
        ///<summary>
        ///Copy memory.
        ///</summary>
        ///<param name="from">Index to copy</param>
        ///<param name="to">Index to copy to</param>
        public void memcpy(long from, long to)
        {
            if (from >= bitsize || to >= bitsize)
            {
                panic(PanicType.gp);
                return;
            }
            RAM[to] = RAM[from];
        }
        ///<summary>
        ///Writes system information.
        ///</summary>
        public void sysinfo()
        {
            string mem = "KB";
            long memtemp = bitsize;
            memtemp /= 1024;
            if (memtemp / 1024 >= 1)
            {
                mem = "MB";
                memtemp /= 1024;
            }
            if (memtemp / 1024 >= 1)
            {
                mem = "GB";
                memtemp /= 1024;
            }
            Console.WriteLine("PC = {0}", RAM[pc]);
            Console.WriteLine("Memory: {0}{1}", memtemp, mem);

        }
        ///<summary>
        ///Sets a memory location
        ///</summary>
        ///<param name="index">Index to set</param>
        ///<param name="val">Value</param>
        public void setMemLoc(long index, int val)
        {
            if (index >= bitsize || IsReserved(index))
            {
                Console.WriteLine("Tried to write to index beyond available RAM.");
                return;
            }

            this.RAM[index] = val;
        }
        private void setReservedBit(long index, int val)
        {
            if (index >= bitsize)
            {
                Console.WriteLine("Tried to write to index beyond available RAM.");
                return;
            }
            this.RAM[index] = val;
        }
        ///<summary>
        ///Increment index by 1
        ///</summary>
        ///<param name="index">Index to increment</param>
        public void inc(int index)
        {
            if (index >= bitsize)
            {
                Console.WriteLine("Tried to write to index beyond available RAM.");
                return;
            }
            RAM[index]++;
        }
        ///<summary>
        ///Dump memory to screen.
        ///</summary>
        public void memdmp()
        {
            for (int i = 0; i < bitsize; i++)
            {
                Console.Write((char)RAM[i]);
            }
            Console.Write('\n');
        }
        ///<summary>
        ///Dump memory as int
        ///</summary>
        ///<param name="e">Whether or not to print 0's (default: true)</param>
        public void intdmp(bool e = true)
        {
            for (int i = 0; i < RAM.Length; i++)
            {
                if (RAM[i] == 0 && !e) { return; }
                Console.Write(RAM[i].ToString() + " ");
            }
            Console.Write('\n');
        }
        /// <summary>
        /// Compare indexes
        /// </summary>
        /// <param name="l1">n1</param>
        /// <param name="l2">n2</param>
        /// <param name="wh">Where to save (1 or 0)</param>
        public void memcmp(int l1, int l2, int wh)
        {
            setMemLoc(wh, l1 == l2 ? 1 : 0);
        }
        ///<summary>
        ///Swap indexes.
        ///</summary>
        ///<param name="i1">Index 1</param>
        ///<param name="i2">Index 2</param>
        public void swp(int i1, int i2)
        {
            RAM[i1] = RAM[i1] + RAM[i2];
            RAM[i2] = RAM[i1] - RAM[i2];
            RAM[i1] = RAM[i1] - RAM[i2];
        }
        ///<summary>
        ///Read to memory
        ///</summary>
        ///<param name="startIndex">Index to start saving to (default:0)</param>
        public void rde(long startIndex = 0)
        {
            string a = Console.ReadLine();
            for (int i = 0; i < a.Length; i++)
            {
                setMemLoc(i + startIndex, a[i]);
            }
        }
        /// <summary>
        /// Print memory locations 
        /// </summary>
        /// <param name="from">From (default:0)</param>
        /// <param name="to">To (default:0)</param>
        public void prnt(int from = 0, int to = 0)
        {
            // function use
            {
                if (to == 0)
                {
                    for (int i = from; i < bitsize; i++)
                    {
                        if (RAM[i] == 0)
                        {
                            to = i;
                            break;
                        }
                    }
                }
                for (int i = from; i < to; i++)
                {
                    if (RAM[i] > 255)
                    {
                        Console.Write("?");
                    }
                    else
                        Console.Write((char)RAM[i]);
                }
            }

            // EOF
            Console.Write('\n');
        }
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="l1">first number</param>
        /// <param name="l2">second number</param>
        /// <param name="wh">Index to store</param>
        public void add(int l1, int l2, int wh)
        {
            setMemLoc(wh, l1 + l2);
        }
        /// <summary>
        /// Sub
        /// </summary>
        /// <param name="l1">n1</param>
        /// <param name="l2">n2</param>
        /// <param name="wh">Index to store</param>
        public void sub(int l1, int l2, int wh)
        {
            setMemLoc(wh, l1 - l2);
        }
        /// <summary>
        /// Mul
        /// </summary>
        /// <param name="l1">n1</param>
        /// <param name="l2">n2</param>
        /// <param name="wh">Index to store</param>
        public void mul(int l1, int l2, int wh)
        {
            setMemLoc(wh, l1 * l2);
        }
        /// <summary>
        /// Div
        /// </summary>
        /// <param name="l1">n1</param>
        /// <param name="l2">n2</param>
        /// <param name="wh">Index to store</param>
        public void div(int l1, int l2, int wh)
        {
            setMemLoc(wh, Convert.ToInt32(Math.Round((double)(l1 / l2))));
        }
        public void rand(int l1, int l2, int wh)
        {
            setMemLoc(wh, new Random().Next(l1, l2));
        }
        public void xor(int l1, int l2, int wh)
        {
            setMemLoc(wh, l1^l2);
        }
        /// <summary>
        /// Square root
        /// </summary>
        /// <param name="l1">n1</param>
        /// <param name="l2">n2</param>
        /// <param name="wh">Index to store</param>
        public void sqrt(int l, int wh)
        {
            setMemLoc(wh, Convert.ToInt32(Math.Round(Math.Sqrt(l))));
        }
        /// <summary>
        /// Power of
        /// </summary>
        /// <param name="l1">n1</param>
        /// <param name="l2">n2</param>
        /// <param name="wh">Index to store</param>
        public void pow(int l1, int l2, int wh)
        {
            setMemLoc(wh, Convert.ToInt32(Math.Round(Math.Pow(l1, l2))));
        }
        /// <summary>
        /// Init system
        /// </summary>
        /// <param name="bitSystem">Size of RAM in KB</param>


        public CPU(long bitSystem = 2)
        {
            initd(bitSystem * 1024);
        }
        /// <summary>
        /// Init function, Can only be run once. USE AT YOUR OWN RISK!
        /// </summary>
        /// <param name="memsize">Amount of memory to allocate</param>
        public void initd(long memsize)
        {
            if (init)
            {
                Console.WriteLine("Attempted to run init while emulator is already initialized.");
                Environment.Exit(1);
            }
            // memtemp init
            {
                long memtemp = memsize;
                memtemp /= 1024;
                {
                    if (memtemp / 1024 >= 1)
                    {
                        memtemp /= 1024;
                    }
                    if (memtemp / 1024 >= 1)
                    {
                        memtemp /= 1024;
                    }
                    if (memtemp <= 0)
                    {
                        Console.WriteLine("Not enough memory to start emulator.");
                        Environment.Exit(1);
                    }
                }

            }
            // array init
            {
                RAM = new long[memsize];
                bitsize = memsize;
                {
                    RIL = new long?[memsize];
                    for (int i = 0; i < memsize; i++)
                    {
                        RIL[i] = null;
                    }
                    memclean(0, bitsize);
                }
            }
            init = true;
            ClockCycle();
        }
    }
}