# Parcs.NET
Parallel distributed system for .NET

<b>Prerequisites</b>

You must have .NET Framework 4.5 or higher installed on your machine to use Parcs.NET. 


<b>Installation</b>

To run Parcs.NET locally, you need to do the following steps:

1. Clone the repository 
2. Open Powershell
3. Go to Installation folder (using cd command)
4. Run .\Setup.ps1 -IpAdresses $your_local_IP_address$ -LocalIp $your_local_IP_address$

The script should install NuGet packages, build the solution using MSBuild, put your IP address in HostServer/bin/Release/hosts.txt file and run both HostServer and Daemon locally. For NuGet install you need nuget command line installed. (https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference).

Alternatively, you can do those steps manually using Visual Studio.

<b>How to create your own module</b>
1. Create Visual Studio Console Application project.
2. Install Parcs and Parcs.Module.CommandLine (optional) NuGet packages.
3. ...
4. PROFIT

<b>Example</b>

The example of a module can be found in NewMatrixModule folder.
