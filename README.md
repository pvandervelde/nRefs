# nRefs

nRefs is an application that creates an XML file containing the assembly names for all referenced assemblies of a given input assembly or executable. To run nRefs use the following command line.

    nRefs.Console.exe -a <Assembly_Path> -o <Output_File>

The output will be a file containing the full assembly name of all the referenced assemblies. An example section as taken from the output file obtained by running nRefs on it's own executable:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<references>
  <reference>Lokad.Shared, Version=1.5.181.0, Culture=neutral, PublicKeyToken=43f0664b2b4db1fc</reference>
  <reference>mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>Nuclei, Version=0.6.4.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9</reference>
  <reference>Nuclei.Build, Version=0.6.4.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9</reference>
  <reference>SMDiagnostics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</reference>
  <reference>System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Data.SqlXml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Runtime.Serialization, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</reference>
  <reference>System.ServiceModel.Internals, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</reference>
  <reference>System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
  <reference>System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</reference>
</references>
```

# Installation instructions
The application is available from NuGet or as ZIP archive from the [releases page](https://github.com/pvandervelde/nRefs/releases).

# How to build
The solution files are created in Visual Studio 2012 (using .NET 4.5) and the entire project can be build by invoking MsBuild on the nrefs.integration.msbuild script. This will build the binaries, the NuGet package and the ZIP archive. The binaries will be placed in the `build\bin\AnyCpu\Release` directory and the NuGet package and the ZIP archive will be placed in the `build\deploy` directory.

Note that the build scripts assume that:

* The binaries should be signed, however the SNK key file is not included in the repository so a new key file has to be [created][snkfile_msdn]. The key file is referenced through an environment variable called `SOFTWARE_SIGNING_KEY_PATH` that has as value the full path of the key file. 
* GIT can be found on the PATH somewhere so that it can be called to get the hash of the last commit in the current repository. This hash is embedded in the nRefs executable together with information about the build configuration and build time and date.

[snkfile_msdn]: http://msdn.microsoft.com/en-us/library/6f05ezxy(v=vs.110).aspx
