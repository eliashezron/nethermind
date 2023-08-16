// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System.Collections.Generic;

namespace Nethermind.Evm.Tracing.GethStyle;

public class GethCustomTracers
{
    private readonly List<string> _retVal = new List<string>();

    public void Step(dynamic log, dynamic db)
    {
        _retVal.Add($"{log.getPC()}:{log.op.toString()}");
    }

    public void Fault(dynamic log, dynamic db)
    {
        _retVal.Add($"FAULT: {Newtonsoft.Json.JsonConvert.SerializeObject(log)}");
    }

    public List<string> Result(dynamic ctx, dynamic db)
    {
        return _retVal;
    }
}
