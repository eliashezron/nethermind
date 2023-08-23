// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System.Collections.Generic;

namespace Nethermind.Evm.Tracing.GethStyle;

public class GethCustomTxTraceEntry
{
    public double? Pc { get; set; }

    public string? Op { get; set; }

    public double? Gas { get; set; }

    public double? GasCost { get; set; }

    public double? Depth { get; set; }

    public string? Error { get; set; }

    public List<string>? Stack { get; set; }

    public List<string>? Memory { get; set; }

    public Dictionary<string, string>? Storage { get; set; }
}
