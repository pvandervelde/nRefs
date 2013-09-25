//-----------------------------------------------------------------------
// <copyright company="nRefs">
//     Copyright 2013 nRefs. Licensed under the Apache License, Version 2.0.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml.Linq;
using Mono.Options;
using nRefs.Console.Nuclei.Fusion;
using nRefs.Console.Properties;
using Nuclei;

namespace nRefs.Console
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1400:AccessModifierMustBeDeclared",
            Justification = "Access modifiers should not be declared on the entry point for a command line application. See FxCop.")]
    static class Program
    {
        /// <summary>
        /// Defines the error code for a normal application exit (i.e without errors).
        /// </summary>
        private const int NormalApplicationExitCode = 0;

        /// <summary>
        /// Defines the error code for an application exit with an unhandled exception.
        /// </summary>
        private const int UnhandledExceptionApplicationExitCode = 1;

        /// <summary>
        /// Defines the exit code for an application exit due to input errors.
        /// </summary>
        private const int InputErrorsOccuredApplicationExitCode = 2;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// The array containing the start-up arguments for the application.
        /// </param>
        /// <returns>A value indicating if the process exited normally (0) or abnormally (&gt; 0).</returns>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1400:AccessModifierMustBeDeclared",
            Justification = "Access modifiers should not be declared on the entry point for a command line application. See FxCop.")]
        static int Main(string[] args)
        {
            ShowHeader();

            bool showHelp = false;
            string assemblyPath = null;
            string outputFile = null;

            var options = new OptionSet 
                {
                    {
                        Resources.CommandLine_Param_AssemblyPath_Key,
                        Resources.CommandLine_Param_AssemblyPath_Description,
                        v => assemblyPath = Path.GetFullPath(v)
                    },
                    { 
                        Resources.CommandLine_Param_Output_Key, 
                        Resources.CommandLine_Param_Output_Description, 
                        v => outputFile = Path.GetFullPath(v)
                    },
                    {
                        Resources.CommandLine_Param_Help_Key,
                        Resources.CommandLine_Param_Help_Description,
                        v => showHelp = v != null 
                    }
                };

            try
            {
                options.Parse(args);
            }
            catch (OptionException)
            {
                WriteErrorToConsole(Resources.Output_Error_InvalidInput);
                return UnhandledExceptionApplicationExitCode;
            }

            if (showHelp)
            {
                ShowHelp(options);
                return NormalApplicationExitCode;
            }

            if (string.IsNullOrWhiteSpace(outputFile) ||
                string.IsNullOrWhiteSpace(assemblyPath) ||
                !File.Exists(assemblyPath))
            {
                WriteErrorToConsole(Resources.Output_Error_MissingValues);
                ShowHelp(options);
                return InputErrorsOccuredApplicationExitCode;
            }

            try
            {
                WriteReferenceInformationToOutputFile(assemblyPath, outputFile);
                return NormalApplicationExitCode;
            }
            catch (Exception)
            {
                return UnhandledExceptionApplicationExitCode;
            }
        }

        private static void ShowHeader()
        {
            System.Console.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Header_ApplicationAndVersion,
                    GetVersion()));
            System.Console.WriteLine(GetCopyright());
            System.Console.WriteLine(GetLibraryLicenses());
        }

        private static void ShowHelp(OptionSet argProcessor)
        {
            System.Console.WriteLine(Resources.Help_Usage_Intro);
            System.Console.WriteLine();
            argProcessor.WriteOptionDescriptions(System.Console.Out);
        }

        private static void WriteErrorToConsole(string errorText)
        {
            try
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(errorText);
            }
            finally
            {
                System.Console.ResetColor();
            }
        }

        private static void WriteToConsole(string text)
        {
            System.Console.WriteLine(text);
        }

        private static string GetVersion()
        {
            var attribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            Debug.Assert(attribute.Length == 1, "There should be a copyright attribute.");

            return (attribute[0] as AssemblyFileVersionAttribute).Version;
        }

        private static string GetCopyright()
        {
            var attribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Debug.Assert(attribute.Length == 1, "There should be a copyright attribute.");

            return (attribute[0] as AssemblyCopyrightAttribute).Copyright;
        }

        private static string GetLibraryLicenses()
        {
            var licenseXml = EmbeddedResourceExtracter.LoadEmbeddedStream(
                Assembly.GetExecutingAssembly(),
                @"nRefs.Console.Properties.licenses.xml");
            var doc = XDocument.Load(licenseXml);
            var licenses = from element in doc.Descendants("package")
                           select new
                           {
                               Id = element.Element("id").Value,
                               Version = element.Element("version").Value,
                               Source = element.Element("url").Value,
                               License = element.Element("licenseurl").Value,
                           };

            var builder = new StringBuilder();
            builder.AppendLine(Resources.Header_OtherPackages_Intro);
            foreach (var license in licenses)
            {
                builder.AppendLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Header_OtherPackages_IdAndLicense,
                        license.Id,
                        license.Version,
                        license.Source));
            }

            return builder.ToString();
        }

        private static void WriteReferenceInformationToOutputFile(string assemblyPath, string outputFile)
        {
            var fusionHelper = new FusionHelper(
                () => Directory.EnumerateFiles(
                    Path.GetDirectoryName(assemblyPath), 
                    "*.dll", 
                    SearchOption.AllDirectories));
            AppDomain.CurrentDomain.AssemblyResolve += fusionHelper.LocateAssemblyOnAssemblyLoadFailure;

            // Load the assembly
            // Get all the references, then get all the references of the references ..
            var knownReferences = new Dictionary<string, AssemblyName>();
            
            var assemblies = new Queue<Assembly>();
            var baseAssembly = LoadAssembly(assemblyPath);
            if (baseAssembly == null)
            {
                return;
            }

            assemblies.Enqueue(baseAssembly);
            while (assemblies.Count > 0)
            {
                var assembly = assemblies.Dequeue();
                var references = assembly.GetReferencedAssemblies();
                foreach (var reference in references)
                {
                    if (!knownReferences.ContainsKey(reference.FullName))
                    {
                        var dependencyAssembly = LoadAssembly(reference);
                        if (dependencyAssembly == null)
                        {
                            continue;
                        }

                        assemblies.Enqueue(dependencyAssembly);
                        knownReferences.Add(reference.FullName, reference);
                    }
                }
            }

            var list = knownReferences.Values.ToList();
            list.Sort((a, b) => a.FullName.CompareTo(b.FullName));

            var doc = new XDocument();
            var root = new XElement("references");
            doc.Add(root);

            foreach (var reference in list)
            {
                var element = new XElement("reference")
                    {
                        Value = reference.FullName
                    };
                root.Add(element);
            }

            doc.Save(outputFile);
        }

        private static Assembly LoadAssembly(string assemblyPath)
        {
            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (FileLoadException)
            {
                return null;
            }
            catch (PathTooLongException)
            {
                return null;
            }
            catch (BadImageFormatException)
            {
                return null;
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (FileLoadException)
            {
                return null;
            }
            catch (PathTooLongException)
            {
                return null;
            }
            catch (BadImageFormatException)
            {
                return null;
            }
            catch (SecurityException)
            {
                return null;
            }
        }
    }
}
