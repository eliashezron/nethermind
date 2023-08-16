// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System;
using System.Collections.Generic;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Nethermind.Evm.Tracing.GethStyle;

namespace Nethermind.Evm.Tracing.JavascriptTracer
{
    // public class JavascriptOpCounterTracer
    // {
    //     public static void RunTracer()
    //     {
    //         using (var engine = new V8ScriptEngine())
    //         {
    //             string jsCode = @"
    //                 var tracer = {
    //                     count: 0,
    //                     step: function(log, db) { this.count++; },
    //                     fault: function(log, db) { },
    //                     result: function(ctx, db) { return this.count; }
    //                 };
    //                 tracer;
    //             ";
    //
    //             // Execute the JavaScript code
    //             dynamic tracer = engine.Evaluate(jsCode);
    //         }
    //     }
    //
    // }
    public class JavascriptOpCounterTracer : GethCustomTracers
    {
        private int _count = 0;

        public new void Step(dynamic log, dynamic db)
        {

            _count++;
        }

        public new void Fault(dynamic log, dynamic db)
        {
        }

        public new List<string> Result(dynamic ctx, dynamic db)
        {
            List<string> result = new List<string>();
            result.Add($"Total operation count: {_count}");
            return result;
        }
    }
}
