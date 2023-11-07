// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only


using System;
using System.Collections.Generic;
using System.Threading;
using Nethermind.Core;
using Nethermind.Core.Collections;
using Nethermind.Core.Specs;
using Nethermind.State;

namespace Nethermind.Evm.Tracing.GethStyle.Javascript;

public class GethLikeBlockJavascriptTracer : BlockTracerBase<GethLikeTxTrace, GethLikeJavascriptTxTracer>, IDisposable
{
    private readonly IReleaseSpec _spec;
    private readonly GethTraceOptions _options;
    private readonly Context _ctx;
    private readonly Db _db;
    private int _index;
    private ArrayPoolList<IDisposable>? _engines;

    public GethLikeBlockJavascriptTracer(IWorldState worldState, IReleaseSpec spec, GethTraceOptions options) : base(options.TxHash)
    {
        _spec = spec;
        _options = options;
        _ctx = new Context();
        _db = new Db(worldState);
    }

    public override void StartNewBlockTrace(Block block)
    {
        _engines = new ArrayPoolList<IDisposable>(block.Transactions.Length + 1);
        _ctx.block = block.Number;
        _ctx.BlockHash = block.Hash;
        base.StartNewBlockTrace(block);
    }

    protected override GethLikeJavascriptTxTracer OnStart(Transaction? tx)
    {
        _ctx.gasPrice = (ulong)tx!.GasPrice;
        _ctx.TxHash = tx.Hash;
        _ctx.txIndex = tx.Hash is not null ? _index++ : null;
        Engine engine = new(_spec);
        _engines?.Add(engine);
        return new GethLikeJavascriptTxTracer(this, engine, _db, _ctx, _options);
    }

    public override void EndBlockTrace()
    {
        base.EndBlockTrace();
        Engine.CurrentEngine = null;
        ArrayPoolList<IDisposable>? list = Interlocked.Exchange(ref _engines, null);
        list?.Dispose();
    }

    protected override bool ShouldTraceTx(Transaction? tx) => base.ShouldTraceTx(tx) && tx is not null;

    protected override GethLikeTxTrace OnEnd(GethLikeJavascriptTxTracer txTracer) => txTracer.BuildResult();
    public void Dispose()
    {
        ArrayPoolList<IDisposable>? list = Interlocked.Exchange(ref _engines, null);
        if (list is not null)
        {
            list.ForEach(e => e.Dispose());
            list.Dispose();
        }
    }
}
