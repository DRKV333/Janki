using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DLRPrecompFixer
{
    public static class Program
    {
        private const string OldClosureName = "Closure";
        private const string TypeToPatchName = "DLRCachedCode";
        private const string ConstantsFieldName = "Constants";
        private const string LocalsFieldName = "Locals";

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Required arguments: [AssemblySourcePath] [AssemblySavePath]", nameof(args));

            AssemblyDefinition def = AssemblyDefinition.ReadAssembly(args[0]);

            TypeDefinition cachedCodeType = def.MainModule.GetType(TypeToPatchName);

            Type closure = typeof(FakeClosure.FakeClosure);

            TypeReference newClosure = def.MainModule.ImportReference(closure);
            MethodReference newClosureCtor = def.MainModule.ImportReference(closure.GetConstructor(new[] { typeof(object[]), typeof(object[]) }));
            FieldReference newConstants = def.MainModule.ImportReference(closure.GetField(ConstantsFieldName));
            FieldReference newLocals = def.MainModule.ImportReference(closure.GetField(LocalsFieldName));

            foreach (var method in cachedCodeType.Methods)
            {
                foreach (var param in method.Parameters.Where(x => x.ParameterType.Name == OldClosureName))
                {
                    param.ParameterType = newClosure;
                }

                if (method.HasBody)
                {
                    foreach (var inst in method.Body.Instructions)
                    {
                        if (inst.OpCode == OpCodes.Newobj)
                        {
                            MethodReference operand = (MethodReference)inst.Operand;
                            if (operand.DeclaringType.Name == OldClosureName)
                                inst.Operand = newClosureCtor;
                        }
                        else if (inst.OpCode == OpCodes.Ldfld)
                        {
                            FieldReference operand = (FieldReference)inst.Operand;
                            if (operand.Name == ConstantsFieldName)
                                inst.Operand = newConstants;
                            else if (operand.Name == LocalsFieldName)
                                inst.Operand = newLocals;
                        }
                    }
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(args[1]));
            def.Write(args[1]);
        }
    }
}
