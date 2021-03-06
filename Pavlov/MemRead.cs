﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pavlov
{
    class MemRead
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr process, IntPtr baseAddress, [Out] byte[] buffer, int size, out IntPtr bytesRead);

        private int processNum;
        private Process curProcess;
        private IntPtr petIntimacyAddress;
        private IntPtr petFoodAddress;
        private IntPtr petNameAddress;
        private IntPtr petIdAddress;
        private IntPtr homonIntimacyAddress;
        private IntPtr homonFoodAddress;
        private IntPtr homonNameAddress;
        private IntPtr homonIdAddress;
        private IntPtr charNameAddress;

        public void GetProcess()
        {
            try
            {
                var prosesses = Process.GetProcessesByName("Ragexe");
                if (processNum >= prosesses.Count()) processNum = 0;
                curProcess = prosesses[processNum++];
            }
            catch
            {
                processNum = 0;
                curProcess = null;
            }
            if (curProcess != null)
            {
                SigScan sigScan = new SigScan { Process = curProcess, DumpSize = 0x5B8D80 };
                petIntimacyAddress = sigScan.FindAddress(new byte[] { 0x8B, 0x43, 0x07, 0xA3, 0x00, 0x00, 0x00, 0x00, 0x85, 0xFF }, "xxxx????xx", 4);
                charNameAddress = sigScan.FindAddress(new byte[] { 0x0F, 0xB6, 0x84, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x30, 0x81 }, "xxxx????xx", 4);
                petFoodAddress = petIntimacyAddress - 0x4;
                petNameAddress = petIntimacyAddress - 0x30;
                petIdAddress = petIntimacyAddress - 0xC;
                IntPtr homonAtk = sigScan.FindAddress(new byte[] { 0x23, 0xA3, 0x00, 0x00, 0x00, 0x00, 0x0F, 0xBF, 0x46, 0x25 }, "xx????xxxx", 2);
                homonNameAddress = homonAtk - 0x20;
                homonFoodAddress = homonAtk + 0x48;
                homonIntimacyAddress = homonAtk + 0x38;
                homonIdAddress = homonAtk + 0x6C;
            }
        }

        public GameInfo GetPetValues()
        {
            try
            {
                return new GameInfo
                {
                    Name = ReadString(curProcess.Handle, petNameAddress),
                    Food = ReadInt(curProcess.Handle, petFoodAddress),
                    Intimacy = ReadInt(curProcess.Handle, petIntimacyAddress),
                    Id = ReadUInt(curProcess.Handle, petIdAddress),
                    CharName = ReadString(curProcess.Handle, charNameAddress)
                };
            }
            catch
            {
                return null;
            }
        }

        public GameInfo GetHomonValues()
        {
            try
            {
                return new GameInfo
                {
                    Name = ReadString(curProcess.Handle, homonNameAddress),
                    Food = ReadInt(curProcess.Handle, homonFoodAddress),
                    Intimacy = ReadInt(curProcess.Handle, homonIntimacyAddress),
                    Id = ReadUInt(curProcess.Handle, homonIdAddress),
                    CharName = ReadString(curProcess.Handle, charNameAddress)
                };
            }
            catch
            {
                return null;
            }
        }

        private static string ReadString(IntPtr process, IntPtr baseAddress)
        {
            var buffer = new byte[8];
            ReadProcessMemory(process, baseAddress, buffer, 8, out IntPtr bytesRead);
            return System.Text.Encoding.Default.GetString(buffer).Replace("\0", string.Empty);
        }

        private static int ReadInt(IntPtr process, IntPtr baseAddress)
        {
            var buffer = new byte[4];
            ReadProcessMemory(process, baseAddress, buffer, 4, out IntPtr bytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        private static uint ReadUInt(IntPtr process, IntPtr baseAddress)
        {
            var buffer = new byte[4];
            ReadProcessMemory(process, baseAddress, buffer, 4, out IntPtr bytesRead);
            return BitConverter.ToUInt32(buffer, 0);
        }
    }
}
