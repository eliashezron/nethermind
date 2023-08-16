// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using Microsoft.ClearScript.V8;
namespace Nethermind.Evm.Tracing.JavascriptTracer;

public class JavascriptBigramTracer
{
    public static void RunTracer()
    {
        using (var engine = new V8ScriptEngine())
        {
            string jsCode = @"
                    var bigramTracer = {
                        hist: {},
                        lastOp: '',
                        lastDepth: 0,
                        step: function(log, db) {
                            var op = log.op.toString();
                            var depth = log.getDepth();
                            if (depth == this.lastDepth){
                                var key = this.lastOp+'-'+op;
                                if (this.hist[key]){
                                    this.hist[key]++;
                                }
                                else {
                                    this.hist[key] = 1;
                                }
                            }
                            this.lastOp = op;
                            this.lastDepth = depth;
                        },
                        fault: function(log, db) {},

                        result: function(ctx) {
                            return this.hist;
                        },
                    };
                    bigramTracer;
                ";
            // Execute the JavaScript code
            dynamic bigramTracer = engine.Evaluate(jsCode);
        }
    }
}
