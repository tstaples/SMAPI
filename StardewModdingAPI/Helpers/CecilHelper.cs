using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Helpers
{
    public static class CecilHelper
    {
        //System.Void StardewValley.Game1::.ctor()
        
        private static void InjectMethod(ILProcessor ilProcessor, Instruction target, MethodReference method)
        {
            Instruction callEnterInstruction = ilProcessor.Create(OpCodes.Call, method);
            ilProcessor.InsertBefore(target, callEnterInstruction);
        }

        private static void InjectMethod(ILProcessor ilProcessor, IEnumerable<Instruction> targets, MethodReference method)
        {
            foreach(var target in targets)
            {
                InjectMethod(ilProcessor, target, method);
            }
        }


       // public void ReplaceInstruction(ILProcessor processor, OpCode opcode, string oldOperand, string newOperand)
        //{
            //var instructions = processor.Body.Instructions.Where(i => i.OpCode == opcode && i.Operand == oldOperand);
           // processor.Create()
        //}

        public static void InjectEntryMethod(CecilContext stardewContext, CecilContext smapiContext, string injecteeType, string injecteeMethod, 
            string injectedType, string injectedMethod)
        {
            var methodInfo = smapiContext.GetSMAPIMethodReference(injectedType, injectedMethod);
            var reference = stardewContext.ImportSMAPIMethodInStardew(methodInfo);
            var ilProcessor = stardewContext.GetMethodILProcessor(injecteeType, injecteeMethod);
            InjectMethod(ilProcessor, ilProcessor.Body.Instructions.First(), reference);
        }

        public static void InjectExitMethod(CecilContext stardewContext, CecilContext smapiContext, string injecteeType, string injecteeMethod,
            string injectedType, string injectedMethod)
        {
            var methodInfo = smapiContext.GetSMAPIMethodReference(injectedType, injectedMethod);
            var reference = stardewContext.ImportSMAPIMethodInStardew(methodInfo);
            var ilProcessor = stardewContext.GetMethodILProcessor(injecteeType, injecteeMethod);
            InjectMethod(ilProcessor, ilProcessor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret), reference);
        }
    }
}
