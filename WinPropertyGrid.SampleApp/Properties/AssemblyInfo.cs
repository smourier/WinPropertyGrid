using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("WinPropertyGrid.SampleApp")]
[assembly: AssemblyDescription("WinUI3 Property Grid Sample App")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WinPropertyGrid")]
[assembly: AssemblyCopyright("Copyright (C) 2023-2026 Simon Mourier. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("1ca7ee95-5d39-467b-a034-a87a64cb3acf")]
[assembly: SupportedOSPlatform("windows10.0.22621.0")]
