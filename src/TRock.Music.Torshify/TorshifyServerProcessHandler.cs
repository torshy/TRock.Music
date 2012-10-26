using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TRock.Music.Torshify
{
    public class TorshifyServerProcessHandler
    {
        #region Fields

        private Job _job;

        #endregion Fields

        #region Constructors

        public TorshifyServerProcessHandler()
        {
            Port = 8081;
            Hidden = true;
        }

        #endregion Constructors

        #region Enumerations

        public enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11
        }

        #endregion Enumerations

        #region Properties

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string TorshifyServerLocation
        {
            get;
            set;
        }

        public bool CloseServerTogetherWithClient
        {
            get;
            set;
        }

        public bool Hidden
        {
            get; 
            set;
        }

        public bool AutostartIfCrashed
        {
            get; 
            set;
        }

        #endregion Properties

        #region Methods

        public void Start()
        {
            var torshify = Process.GetProcessesByName("TRock.Music.Torshify.Server").FirstOrDefault();

            if (torshify == null)
            {
                torshify = StartTorshifyServer();
            }

            if (torshify != null && CloseServerTogetherWithClient)
            {
                _job = new Job();
                _job.AddProcess(torshify.Handle);
            }
        }

        private Process StartTorshifyServer()
        {
            if (!File.Exists(TorshifyServerLocation))
            {
                throw new FileNotFoundException("Torshify Server is not located at " + TorshifyServerLocation);
            }

            if (string.IsNullOrEmpty(UserName))
            {
                throw new ArgumentException("Please specify your Spotify username");
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentException("Please specify your Spotify password");
            }

            var credentials = string.Format("/username={0} /password={1} /port={2}", UserName, Password, Port);

            if (Hidden)
            {
                credentials += " /hidden";
            }

            Process torshify = Process.Start(TorshifyServerLocation, credentials);

            if (torshify != null && AutostartIfCrashed)
            {
                torshify.Exited += (sender, args) =>
                {
                    if (!AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        torshify = StartTorshifyServer();
                    }
                };

                torshify.EnableRaisingEvents = true;
            }

            return torshify;
        }

        #endregion Methods

        #region Nested Types

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public UInt32 nLength;
            public IntPtr lpSecurityDescriptor;
            public Int32 bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IO_COUNTERS
        {
            public UInt64 ReadOperationCount;
            public UInt64 WriteOperationCount;
            public UInt64 OtherOperationCount;
            public UInt64 ReadTransferCount;
            public UInt64 WriteTransferCount;
            public UInt64 OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public Int64 PerProcessUserTimeLimit;
            public Int64 PerJobUserTimeLimit;
            public UInt32 LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public UInt32 ActiveProcessLimit;
            public UIntPtr Affinity;
            public UInt32 PriorityClass;
            public UInt32 SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        public class Job : IDisposable
        {
            #region Fields

            private bool _disposed;
            private IntPtr _handle;

            #endregion Fields

            #region Constructors

            public Job()
            {
                _handle = CreateJobObject(IntPtr.Zero, null);

                var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = 0x2000
                };

                var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
                {
                    BasicLimitInformation = info
                };

                int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!SetInformationJobObject(_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                    throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
            }

            #endregion Constructors

            #region Methods

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Close()
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }

            public bool AddProcess(IntPtr processHandle)
            {
                return AssignProcessToJobObject(_handle, processHandle);
            }

            public bool AddProcess(int processId)
            {
                return AddProcess(Process.GetProcessById(processId).Handle);
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern IntPtr CreateJobObject(IntPtr a, string lpName);

            [DllImport("kernel32.dll")]
            static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, UInt32 cbJobObjectInfoLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

            private void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing) { }

                Close();
                _disposed = true;
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}