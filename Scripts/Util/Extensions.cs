using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDB_Parser.Blocks;

namespace TQDBEditor
{
    public static class StringExtensions
    {
        public static bool ValidateEqnValue(this VariableBlock self, string value, IReadOnlyList<string> validVariables, out IReadOnlyList<int> invalidIndices)
        {
            if (self.Type != TQDB_Parser.VariableType.equation)
                throw new ArgumentException("This variable is not an equation", nameof(self));

            var invalidIndices_internal = new List<int>();
            var ret = self.ValidateValue(value, out var originalInvalid);
            invalidIndices_internal.AddRange(originalInvalid);

            var addingLetters = false;
            StringBuilder currentVariable = new();
            var variableIndices = new List<int>();

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!char.IsLetter(c))
                {
                    if (addingLetters)
                        CheckVariable();
                    continue;
                }
                addingLetters = true;
                currentVariable.Append(c);
                variableIndices.Add(i);
            }
            if (addingLetters)
                CheckVariable();

            invalidIndices = invalidIndices_internal;
            return ret;

            void CheckVariable()
            {
                addingLetters = false;
                if (!validVariables.Contains(currentVariable.ToString()))
                {
                    invalidIndices_internal.AddRange(variableIndices);
                    ret = false;
                }
                currentVariable.Clear();
                variableIndices.Clear();
            }
        }
    }
}
