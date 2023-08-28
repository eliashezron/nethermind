// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Microsoft.ClearScript.V8; // For the V8ScriptEngine
using Newtonsoft.Json.Linq; // For JArray

namespace Nethermind.Evm.Tracing.GethStyle;

public abstract class GethLikeTxTracer<TEntry> : TxTracer where TEntry : GethTxTraceEntry
{
    private static GethJavascriptCustomTracer? _customTracers;
    string customTracerCode = @"
    var tracer = {
        retVal: [],
        step: function(log, db) { this.retVal.push(log.gas + ':' + log.op) },
        fault: function(log, db) { this.retVal.push('FAULT: ' + JSON.stringify(log)) },
        result: function(ctx, db) { return this.retVal }
    };
";
    protected GethLikeTxTracer(GethTraceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        IsTracingFullMemory = options.EnableMemory;
        IsTracingOpLevelStorage = !options.DisableStorage;
        IsTracingStack = !options.DisableStack;
        if (!string.IsNullOrWhiteSpace(customTracerCode))
        {
            // Create the GethJavascriptCustomTracers instance using the provided JavaScript code from GethTraceOptions
            _customTracers = new GethJavascriptCustomTracer(customTracerCode);

        }
        IsTracing = IsTracing || IsTracingFullMemory;
    }

    public sealed override bool IsTracingOpLevelStorage { get; protected set; }
    public override bool IsTracingReceipt => true;
    public sealed override bool IsTracingMemory { get; protected set; }
    public override bool IsTracingInstructions => true;
    public sealed override bool IsTracingStack { get; protected set; }
    protected bool IsTracingFullMemory { get; }



    protected TEntry? CurrentTraceEntry { get; set; }
    protected GethLikeTxTrace Trace { get; } = new();

    public override void MarkAsSuccess(Address recipient, long gasSpent, byte[] output, LogEntry[] logs, Keccak? stateRoot = null)
    {
        Trace.ReturnValue = output;
    }

    public override void MarkAsFailed(Address recipient, long gasSpent, byte[]? output, string error, Keccak? stateRoot = null)
    {
        Trace.Failed = true;
        Trace.ReturnValue = output ?? Array.Empty<byte>();
    }

    public override void StartOperation(int depth, long gas, Instruction opcode, int pc, bool isPostMerge = false)
    {
        if (CurrentTraceEntry is not null)
            AddTraceEntry(CurrentTraceEntry);

        CurrentTraceEntry = CreateTraceEntry(opcode);
        CurrentTraceEntry.Depth = depth;
        CurrentTraceEntry.Gas = gas;
        CurrentTraceEntry.Opcode = opcode.GetName(isPostMerge);
        CurrentTraceEntry.ProgramCounter = pc;

        //Console.WriteLine(CurrentTraceEntry);

        var gethStyleLog = new GethJavascriptStyleLog
        {
            pc = CurrentTraceEntry.ProgramCounter,
            op = CurrentTraceEntry.Opcode,
            gas = CurrentTraceEntry.Gas,
            gasCost = CurrentTraceEntry.GasCost,
            depth = CurrentTraceEntry.Depth
        };

        //Console.WriteLine("this is the opcode {0}",gethStyleLog.op);

        if (_customTracers != null)
        {
            Console.WriteLine("_customTracers is not null, calling Step");
            _customTracers.Step(gethStyleLog, null);
        }
        else
        {
            Console.WriteLine("_customTracers is null, skipping Step");
        }

    }

    public override void ReportOperationError(EvmExceptionType error) => CurrentTraceEntry.Error = GetErrorDescription(error);

    private string? GetErrorDescription(EvmExceptionType evmExceptionType)
    {
        return evmExceptionType switch
        {
            EvmExceptionType.None => null,
            EvmExceptionType.BadInstruction => "BadInstruction",
            EvmExceptionType.StackOverflow => "StackOverflow",
            EvmExceptionType.StackUnderflow => "StackUnderflow",
            EvmExceptionType.OutOfGas => "OutOfGas",
            EvmExceptionType.InvalidSubroutineEntry => "InvalidSubroutineEntry",
            EvmExceptionType.InvalidSubroutineReturn => "InvalidSubroutineReturn",
            EvmExceptionType.InvalidJumpDestination => "BadJumpDestination",
            EvmExceptionType.AccessViolation => "AccessViolation",
            EvmExceptionType.StaticCallViolation => "StaticCallViolation",
            _ => "Error"
        };
    }

    public override void ReportOperationRemainingGas(long gas) => CurrentTraceEntry.GasCost = CurrentTraceEntry.Gas - gas;

    public override void SetOperationMemorySize(ulong newSize) => CurrentTraceEntry.UpdateMemorySize(newSize);

    public override void SetOperationStack(List<string> stackTrace) => CurrentTraceEntry.Stack = stackTrace;

    public override void SetOperationMemory(IEnumerable<string> memoryTrace)
    {
        if (IsTracingFullMemory)
            CurrentTraceEntry.Memory = memoryTrace.ToList();
    }

    public virtual GethLikeTxTrace BuildResult()
    {
        if (CurrentTraceEntry is not null)
            AddTraceEntry(CurrentTraceEntry);
        return Trace;
    }
    protected abstract void AddTraceEntry(TEntry entry);

    protected abstract TEntry CreateTraceEntry(Instruction opcode);
}
