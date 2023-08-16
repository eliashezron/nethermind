// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using Microsoft.ClearScript.V8;

namespace Nethermind.Evm.Tracing.JavascriptTracer;

public class JavascriptNoOpTracer
{
    public static void RunTracer()
    {
        using (var engine = new V8ScriptEngine())
        {
            string jsCode = @"
                    var noOpTracer = {
                        step: function(log, db) { },
                        fault: function(log, db) { },
                        result: function(ctx, db) { return {}}
                    };
                    noOpTracer;
                ";

            // Execute the JavaScript code
            dynamic noOpTracer = engine.Evaluate(jsCode);
        }
    }
}
