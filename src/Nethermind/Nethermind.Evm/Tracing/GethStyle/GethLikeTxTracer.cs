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
    private readonly GethJavascriptCustomTracers _customTracers;
    protected GethLikeTxTracer(GethTraceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        IsTracingFullMemory = options.EnableMemory;
        IsTracingOpLevelStorage = !options.DisableStorage;
        IsTracingStack = !options.DisableStack;
        if (!string.IsNullOrWhiteSpace(options.Tracer))
        {
            // Create the V8ScriptEngine
            using (var engine = new V8ScriptEngine())
            {
                // Create the GethJavascriptCustomTracers instance using the provided JavaScript code from GethTraceOptions
                _customTracers = new GethJavascriptCustomTracers(engine, options.Tracer);
            }
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

        // Use the custom Javascript tracer to record trace entries as per the step js command
        _customTracers?.Step(CurrentTraceEntry, null);
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

        // Use the custom tracer to get the result
        JArray customResult = _customTracers?.Result(null, null);

        // Store the custom javascript tracer result in the trace
        if (customResult != null)
        {
            Trace.CustomTracerResult.Add(customResult);
        }
        return Trace;
    }
    protected abstract void AddTraceEntry(TEntry entry);

    protected abstract TEntry CreateTraceEntry(Instruction opcode);
}
