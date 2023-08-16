// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using Microsoft.ClearScript.V8;
namespace Nethermind.Evm.Tracing.JavascriptTracer;

public class JavascriptUnigramTracer
{
    public static void RunTracer()
    {
        using (var engine = new V8ScriptEngine())
        {
            string jsCode = @"
                    var unigramTracer = {
                        hist: {},
                        nops: 0,
                        step: function(log, db) {
                            var op = log.op.toString();
                            if (this.hist[op]){
                                this.hist[op]++;
                            }
                            else {
                                this.hist[op] = 1;
                            }
                            this.nops++;
                        },
                        fault: function(log, db) {},

                        result: function(ctx) {
                            return this.hist;
                        },
                    };
                    unigramTracer;
                ";
            // Execute the JavaScript code
            dynamic unigramTracer = engine.Evaluate(jsCode);
        }
    }
}
