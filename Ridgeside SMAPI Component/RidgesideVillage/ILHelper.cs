using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Harmony;



namespace RidgesideVillage
{
    /*
        * So much of this is based off of, or almost directly ripped from,
        * the theLion's IL code helper, I'm sticking a special thanks
        * in the code itself. They suggested I write my own helper, so I'm doing my best to do
        * that here, but re-inventing the wheel is pretty difficult. I'll be updating this helper
        * as I figure out more about IL code and Harmony transpilers, so that I feel less like
        * a giant plagarist. I can't just subsitute my own variable names and call it a day!
        */

    //Version 0.0.1

    public class ILCodeHelper
    {
        private IMonitor _Monitor { get; }
        private List<CodeInstruction> _instructionList;
        private List<CodeInstruction> _instructionBackup;
        private readonly Stack<int> _indexStack;

        private int currIndex
        {
            get
            {
                if (_indexStack == null || _indexStack.Count() < 1)
                    throw new IndexOutOfRangeException("Instruciton stack is null or empty.");
                return _indexStack.Peek();
            }
        }

        public ILCodeHelper(IMonitor monitor)
        {
            _indexStack = new Stack<int>();
            _Monitor = monitor;
        }

        public ILCodeHelper(IEnumerable<CodeInstruction> instructions, IMonitor monitor)
        {
            _instructionList = instructions.ToList();
            _instructionBackup = _instructionList.Clone();
            _indexStack = new Stack<int>();
            _indexStack.Push(0);
            _Monitor = monitor;

        }

        public ILCodeHelper FindPatternLast(CodeInstruction[] pattern)
        {
            var revInstructions = _instructionList.Clone();
            revInstructions.Reverse();

            var index = _instructionList.Count() - revInstructions.GetPatternIndex(pattern) - 1;
            if (index < 0)
                throw new IndexOutOfRangeException("Pattern not found.");

            _indexStack.Push(index);
            return this;
        }

        public ILCodeHelper InsertOPCode(IEnumerable<CodeInstruction> instructions)
        {
            _instructionList = instructions.ToList();
            _instructionBackup = _instructionList.Clone();

            if (_indexStack.Count > 0)
            {
                _indexStack.Clear();
            }
            _indexStack.Push(0);

            return this;
        }
    }

    public static class CodeInstructionExtentions
    {
        public static List<CodeInstruction> Clone(this IList<CodeInstruction> list)
        {
            return list.Select(instr => new CodeInstruction(instr) { blocks = instr.blocks.ToList() }).ToList();
        }

        public static int GetPatternIndex(this IList<CodeInstruction> list, CodeInstruction[] pattern)
        {
            int start = 0;
            var count = list.Count() - pattern.Count() + 1;

            for (var i = start; i < count; i++)
            {
                var j = 0;
                while (j < pattern.Count() && list[(i + j)].opcode.Equals(pattern[j].opcode) && (pattern[j].operand == null || list[(i + j)].operand.ToString().Equals(pattern[j].operand.ToString())))
                {
                    ++j;
                }
                if (j == pattern.Count())
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
