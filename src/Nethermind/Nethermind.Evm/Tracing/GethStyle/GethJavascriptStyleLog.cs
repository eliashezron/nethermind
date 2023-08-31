// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
namespace Nethermind.Evm.Tracing.GethStyle
{
    public class GethJavascriptStyleLog
    {
        public long? pc { get; set; }
        public string op { get; set; }
        public long gas { get; set; }
        public long gasCost { get; set; }
        public int depth { get; set; }
        public long? getPC()
        {
            return pc;
        }
    }
}

