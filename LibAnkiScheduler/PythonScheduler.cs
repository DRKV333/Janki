using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LibAnkiScheduler
{
    public class PythonScheduler : IScheduler
    {
        private readonly dynamic scheduler;

        public PythonScheduler()
        {
            ScriptEngine pythonEngine = Python.CreateEngine();
            LanguageContext pythonContext = HostingHelpers.GetLanguageContext(pythonEngine);
            ScriptScope scope = pythonEngine.CreateScope();

            pythonContext.DomainManager.LoadAssembly(Assembly.GetAssembly(typeof(DLRCachedCode)));

            scope.ImportModule("schedv2");

            dynamic schedv2 = scope.GetVariable("schedv2");
            scheduler = pythonEngine.Operations.CreateInstance(schedv2.Scheduler);
        }

        public string Name => scheduler.name;
    }
}
