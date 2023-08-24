// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json.Linq;

namespace Nethermind.Evm.Tracing.GethStyle
{
    public class GethJavascriptCustomTracer
    {
        private static readonly V8ScriptEngine _engine = new V8ScriptEngine();
        private readonly dynamic _tracer;

        public GethJavascriptCustomTracer( string jsTracerCode)
        {
            _engine.Execute(jsTracerCode);
            _tracer = _engine.Script.tracer;
        }

        public void Step(dynamic log, dynamic db)
        {
            _tracer.step(log, db);
        }

        public void Fault(dynamic log, dynamic db)
        {
            _tracer.fault(log, db);
        }

        public JArray Result(dynamic ctx, dynamic db)
        {
            var result = _tracer.result(ctx, db);
            return JArray.FromObject(result);
        }
    }
}
