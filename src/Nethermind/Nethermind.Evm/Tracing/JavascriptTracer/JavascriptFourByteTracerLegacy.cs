// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace Nethermind.Evm.Tracing.JavascriptTracer;

public class JavascriptFourByteTracerLegacy
{
    public static void RunTracer()
    {
        using (var engine = new V8ScriptEngine())
        {
            engine.AddHostType("ToHex", typeof(JavascriptHelpers));
            engine.AddHostType("ToAddress", typeof(JavascriptHelpers));
            engine.AddHostType("IsPrecompiled", typeof(JavascriptHelpers));

            string jsCode = @"
                    var fourByteTracer = {
                        ids: {},

                        callType: function(opstr) {
                            switch(opstr){
		                    case 'CALL': case 'CALLCODE':
                                return 3;
                                case 'DELEGATECALL': case 'STATICCALL':
                                return 2; // stack ptr to memin
                            }
                            return false;
                        },

                        store: function(id, size) {
                            var key = "" + toHex(id) + '-' + size;
                            this.ids[key] = this.ids[key] + 1 || 1;
                        },

                        step: function(log, db) {
                            var ct = this.callType(log.op.toString());
		                    if (!ct) {
			                    return;
		                    }
		                    if (isPrecompiled(toAddress(log.stack.peek(1).toString(16)))) {
			                    return;
		                    }
		                    var inSz = log.stack.peek(ct + 1).valueOf();
		                    if (inSz >= 4) {
			                    var inOff = log.stack.peek(ct).valueOf();
			                    this.store(log.memory.slice(inOff, inOff + 4), inSz-4);
		                    }
                        },

                        fault: function(log, db) {},

                        result: function(ctx) {
                            if (ctx.input.length >= 4) {
			                    this.store(slice(ctx.input, 0, 4), ctx.input.length-4)
		                    }
		                    return this.ids;
                        }
                    };
                    fourByteTracer;
                ";

            // Execute the JavaScript code
            dynamic fourByteTracer = engine.Evaluate(jsCode);

        }
    }
}
