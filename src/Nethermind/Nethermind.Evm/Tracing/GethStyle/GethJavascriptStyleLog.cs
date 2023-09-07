// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using Nethermind.Evm;

namespace Nethermind.Evm.Tracing.GethStyle
{
    public class GethJavascriptStyleLog
    {

        public long? pc { get; set; }
        public OpcodeString? op { get; set; }
        public long? gas { get; set; }
        public long? gasCost { get; set; }
        public int? depth { get; set; }

        public long? getPC()
        {
            return pc;
        }
        public Stack? stack { get; set; } = new Stack();
        public class Stack
        {
            private readonly List<string> _items = new();

            public void push(List<string> items)
            {
                _items.AddRange(items);
            }
            public string? peek(int index)
            {
                int topIndex = _items.Count - 1 - index;
                if (topIndex >= 0 && topIndex < _items.Count)
                {
                    return _items[topIndex];
                }
                return null;
            }
        }
        public class OpcodeString
        {
            private readonly string _value;

            public OpcodeString(string value)
            {
                _value = value;
            }

            // Method to get the opcode number
            public string? toNumber()
            {
                if (!string.IsNullOrWhiteSpace(_value))
                {
                    if (Enum.TryParse<Instruction>(_value, out Instruction opcode))
                    {
                        return $"0x{(byte)opcode:X2}"; // Formats the value as a hexadecimal string
                    }
                    else
                    {
                        // Handle unknown opcodes
                        return null;
                    }
                }
                return null;
            }

            // return js .toString() method
            public string toString()
            {
                return _value;
            }
        }

    }
}
