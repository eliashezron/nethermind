// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System;
using System.Diagnostics;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json.Linq;

namespace Nethermind.Evm.Tracing.GethStyle
{
    public class GethJavascriptCustomTracer : GethLikeTxTrace
    {
        private readonly V8ScriptEngine _engine = new V8ScriptEngine();
        private readonly dynamic _tracer;
        

        public GethJavascriptCustomTracer(string jsTracerCode)
        {
      
            _engine.Execute(jsTracerCode);
            _tracer = _engine.Script.tracer;
        }

        public void Step(dynamic log, dynamic db)
        {
            _tracer.step(log, db);
            dynamic? result = _tracer.result(null, null);
            Console.WriteLine("this is the result {0}", JArray.FromObject(result));
            CustomTracerResult?.Add(result);
        }

        public void Fault(dynamic log, dynamic db)
        {
            _tracer.fault(log, db);
        }

        public JArray Result(dynamic ctx, dynamic db)
        {
            dynamic? result = _tracer.result(ctx, db);
            return result;
        }
    }
}
