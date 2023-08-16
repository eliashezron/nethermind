// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using Microsoft.ClearScript.V8;
namespace Nethermind.Evm.Tracing.JavascriptTracer;

public class JavascriptTrigramTracer
{
    public static void RunTracer()
    {
        using (var engine = new V8ScriptEngine())
        {
            string jsCode = @"
                    var trigramTracer = {
                        hist: {},
                        lastOp: = ['',''],
                        lastDepth: 0,
                        step: function(log, db) {
                                var depth = log.getDepth();
                                if (depth != this.lastDepth){
                                    this.lastOps = ['',''];
                                    this.lastDepth = depth;
                                    return;
                                }
                                var op = log.op.toString();
                                var key = this.lastOps[0]+'-'+this.lastOps[1]+'-'+op;
                                if (this.hist[key]){
                                    this.hist[key]++;
                                }
                                else {
                                    this.hist[key] = 1;
                                }
                                this.lastOps[0] = this.lastOps[1];
                                this.lastOps[1] = op;
                            }
                            this.lastOp = op;
                            this.lastDepth = depth;
                        },
                        fault: function(log, db) {},

                        result: function(ctx) {
                            return this.hist;
                        },
                    };
                    trigramTracer;
                ";
            // Execute the JavaScript code
            dynamic trigramTracer = engine.Evaluate(jsCode);
        }
    }
}
