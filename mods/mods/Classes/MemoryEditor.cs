using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace mods
{
    class MemoryEditor
    {
        // REQUIRED CONSTS
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int MEM_COMMIT = 0x00001000;
        const int PAGE_READWRITE = 0x04;
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;

        // REQUIRED METHODS
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess
             (int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory
        (int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress,
          byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess,
        IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);


        // REQUIRED STRUCTS
        public struct MEMORY_BASIC_INFORMATION
        {
            public int BaseAddress;
            public int AllocationBase;
            public int AllocationProtect;
            public int RegionSize;
            public int State;
            public int Protect;
            public int lType;
        }

        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        List<int> address = new List<int>();

        SYSTEM_INFO sys_info;

        IntPtr proc_min_address;
        IntPtr proc_max_address;

        long proc_min_address_l;
        long proc_max_address_l;

        Process process;
        IntPtr processHandle;
        MEMORY_BASIC_INFORMATION mem_basic_info;

        int charMultiplier = 1;
        string encoding;

        public MemoryEditor(string processName, string encoding)
        {
            // getting minimum & maximum address
            sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            proc_min_address = sys_info.minimumApplicationAddress;
            proc_max_address = sys_info.maximumApplicationAddress;

            // saving the values as long ints so I won't have to do a lot of casts later
            proc_min_address_l = (long)proc_min_address;
            proc_max_address_l = (long)proc_max_address;


            // notepad better be runnin'
            process = Process.GetProcessesByName(processName)[0];

            // opening the process with desired access level
            processHandle =
            OpenProcess(0x1F0FFF, false, process.Id);

            // this will store any information we get from VirtualQueryEx()
            mem_basic_info = new MEMORY_BASIC_INFORMATION();

            this.encoding = encoding;

            if(encoding == "ASCII")
            {
                charMultiplier = 1;
            }
            else if(encoding == "Unicode")
            {
                charMultiplier = 2;
            }
        }

        public void Scan_Range(int start, int end)
        {
            StreamWriter sw = new StreamWriter(@"K:\Jake Ball\dump.txt");

            int size = 50;

            int bytesRead = 0;  // number of bytes read with ReadProcessMemory
            byte[] buffer = new byte[size];

            // read everything in the buffer above
            ReadProcessMemory((int)processHandle,
            start - 25, buffer, size, ref bytesRead);

            // then output this in the file
            for (int i = 0; i < size; i++)
                sw.WriteLine("0x{0} : {1}",
                (start + i - 25).ToString("X"), (char)buffer[i]);

            sw.Close();
        }

        private void Scan(string scan)
        {
            int bytesRead = 0;  // number of bytes read with ReadProcessMemory

            proc_min_address = sys_info.minimumApplicationAddress;
            proc_max_address = sys_info.maximumApplicationAddress;

            // saving the values as long ints so I won't have to do a lot of casts later
            proc_min_address_l = (long)proc_min_address;
            proc_max_address_l = (long)proc_max_address;


            while (proc_min_address_l < proc_max_address_l)
            {
                // 28 = sizeof(MEMORY_BASIC_INFORMATION)
                VirtualQueryEx(processHandle, proc_min_address, out mem_basic_info, 28);

                // if this memory chunk is accessible
                if (mem_basic_info.Protect ==
                PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
                {
                    byte[] buffer = new byte[mem_basic_info.RegionSize];

                    // read everything in the buffer above
                    ReadProcessMemory((int)processHandle,
                    mem_basic_info.BaseAddress, buffer, mem_basic_info.RegionSize, ref bytesRead);

                    // then output this in the file
                    for (int i = 0; i < mem_basic_info.RegionSize; i++)
                    {
                        if (Check_String(buffer, i, scan))
                        {
                            //Read_Address(mem_basic_info.BaseAddress + i, scan.Length);
                            //Scan_Range(mem_basic_info.BaseAddress + i, 0);
                            Console.WriteLine("0x{0}: {1}", (mem_basic_info.BaseAddress + i).ToString("X"), (char)buffer[i]);
                            //address.Add(mem_basic_info.BaseAddress + i);
                        }
                    }
                }

                // move to the next memory chunk
                proc_min_address_l += mem_basic_info.RegionSize;
                proc_min_address = new IntPtr(proc_min_address_l);
            }
        }

        public void Read_Address(int address, int length)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[length]; 


            // 0x0046A3B8 is the address where I found the string, replace it with what you found
            ReadProcessMemory((int)processHandle, address, buffer, buffer.Length, ref bytesRead);

            Console.Write(Encoding.ASCII.GetString(buffer) +
               " (" + bytesRead.ToString() + "bytes) ");
        }

        public void Replace_String(string old, string write)
        {
            address.Clear();
            Scan(old);
            OverWrite(old, write);
        }

        private void OverWrite(string old, string write)
        {
            int bytesWritten = 0;
            byte[] bufferWrite;
            if (encoding == "ASCII")
            {
                write = write.PadRight(old.Length, ' ');
                bufferWrite = Encoding.ASCII.GetBytes(write);
            }
            else if(encoding == "Unicode")
            {
                write = write.PadRight(old.Length, '\0');
                bufferWrite = Encoding.Unicode.GetBytes(write);
            }
            else
            {
                bufferWrite = new byte[0];
            }

            foreach (int addr in address)
            {
                WriteProcessMemory((int)processHandle, addr, bufferWrite, bufferWrite.Length, ref bytesWritten);
            }
        }

        private bool Check_String(byte[] buffer, int offset, string test)
        {
            bool found = true;

            for (int i = 0; i < test.Length * charMultiplier; i += charMultiplier)
            {
                char b = (char)buffer[i + offset];
                char t = test[i / charMultiplier];
                if (b != t)
                {
                    found = false;
                    break;
                }
            }

            return found;
        }
    }
}
