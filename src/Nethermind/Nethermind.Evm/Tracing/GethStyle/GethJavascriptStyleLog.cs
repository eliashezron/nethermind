// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Nethermind.Evm;
using Nethermind.Core;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;


namespace Nethermind.Evm.Tracing.GethStyle
{

    public class GethJavascriptStyleLog : GethTxTraceEntry
    {
        public long? pc { get; set; }
        public OpcodeString? op { get; set; }
        public long? gas { get; set; }
        public long? gasCost { get; set; }
        public int? depth { get; set; }

        public Contract? contract { get; set; }
        public long? getPC()
        {
            return pc;
        }
        public Stack? stack { get; set; } = new Stack();
        public new class Stack
        {
            private readonly List<string> _items = new();

            public void push(List<string> items) => _items.AddRange(items);

            public string? length() => _items.Count.ToString();

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
            private readonly Instruction _value;

            public OpcodeString(Instruction value)
            {
                _value = value;
            }

            public string? toNumber() => _value.GetHex();

            public string? toString() => _value.GetName();

        }

        public class Contract
        {
            // private readonly ScriptEngine _engine;
            private readonly Address _address;
            private readonly V8ScriptEngine _engine = new V8ScriptEngine();


            public Contract( Address address)
            {

                _address = address;
            }

            public dynamic getAddress()
            {
                dynamic byteAdrress = _engine.Script.Array.from(_address.Bytes);
                return byteAdrress;
            }
        }
    }
}
