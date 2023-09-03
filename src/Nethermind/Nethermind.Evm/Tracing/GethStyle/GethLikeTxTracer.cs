// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Microsoft.ClearScript.V8; // For the V8ScriptEngine
using Newtonsoft.Json.Linq; // For JArray
using Microsoft.Extensions.Options;

namespace Nethermind.Evm.Tracing.GethStyle;

public abstract class GethLikeTxTracer<TEntry> : TxTracer where TEntry : GethTxTraceEntry
{
    private static GethJavascriptCustomTracer? _customTracers;

    protected GethLikeTxTracer(GethTraceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        IsTracingFullMemory = options.EnableMemory;
        IsTracingOpLevelStorage = !options.DisableStorage;
        IsTracingStack = !options.DisableStack;
        if (!string.IsNullOrWhiteSpace(options.Tracer))
        {
            // Create the GethJavascriptCustomTracers instance using the provided JavaScript code from GethTraceOptions
            _customTracers = new GethJavascriptCustomTracer(options.Tracer);

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
    protected GethJavascriptStyleLog? CustomTraceEntry { get; set; } = new();
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

        //if (_customTracers is not null)
        //{
        //    var gethStyleLog = new GethJavascriptStyleLog
        //    {
        //        pc = CurrentTraceEntry.ProgramCounter,
        //        op = CurrentTraceEntry.Opcode,
        //        gas = CurrentTraceEntry.Gas,
        //        gasCost = CurrentTraceEntry.GasCost,
        //        depth = CurrentTraceEntry.Depth,

        //    };
        //    _customTracers.Step(gethStyleLog, null);
        //}
        if (_customTracers is not null)
        {
            CustomTraceEntry.pc = CurrentTraceEntry.ProgramCounter;
            CustomTraceEntry.op = CurrentTraceEntry.Opcode;
            CustomTraceEntry.gas = CurrentTraceEntry.Gas;
            CustomTraceEntry.gasCost = CurrentTraceEntry.GasCost;
            CustomTraceEntry.depth = CurrentTraceEntry.Depth;

            _customTracers.Step(CustomTraceEntry, null);
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

    public override void SetOperationStack(List<string> stackTrace)
    {
        CurrentTraceEntry.Stack = stackTrace;
        //if (_customTracers is not null)
        //{
        //    var gethStyleLog = new GethJavascriptStyleLog();
        //    gethStyleLog.stack.push(stackTrace);
            
        //}
        if (_customTracers is not null)
        {
            CustomTraceEntry.stack.push(stackTrace);
            
        }

    }

    public override void SetOperationMemory(IEnumerable<string> memoryTrace)
    {
        if (IsTracingFullMemory)
            CurrentTraceEntry.Memory = memoryTrace.ToList();
    }

    public virtual GethLikeTxTrace BuildResult()
    {
        if (CurrentTraceEntry is not null)
            AddTraceEntry(CurrentTraceEntry);
        if (_customTracers is not null)
        {
            Trace.CustomTracerResult.AddRange(_customTracers.CustomTracerResult);
        }
        return Trace;
    }
    protected abstract void AddTraceEntry(TEntry entry);

    protected abstract TEntry CreateTraceEntry(Instruction opcode);
}
