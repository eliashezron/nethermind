// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json.Linq;

namespace Nethermind.Evm.Tracing.GethStyle
{
    public class GethJavascriptCustomTracers
    {
        private readonly V8ScriptEngine _engine;
        private readonly dynamic _tracer;

        public GethJavascriptCustomTracers(V8ScriptEngine engine, string jsTracerCode)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
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
