// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using Microsoft.ClearScript.V8;

namespace Nethermind.Evm.Tracing.JavascriptTracer;

public class JavascriptPrestateTracerLegacy
{
    public static void RunTracer()
    {
        using (var engine = new V8ScriptEngine())
        {
            string jsCode = @"
                    var preStateTracer = {
                        prestate: null,
	                    lookupAccount: function(addr, db){
		                    var acc = toHex(addr);
		                    if (this.prestate[acc] === undefined) {
			                    this.prestate[acc] = {
				                    balance: '0x' + db.getBalance(addr).toString(16),
				                    nonce:   db.getNonce(addr),
				                    code:    toHex(db.getCode(addr)),
				                    storage: {}
			                    };
		                    }
	                    },

	                    lookupStorage: function(addr, key, db){
		                    var acc = toHex(addr);
		                    var idx = toHex(key);

		                    if (this.prestate[acc].storage[idx] === undefined) {
			                    this.prestate[acc].storage[idx] = toHex(db.getState(addr, key));
		                    }
	                    },

	                    result: function(ctx, db) {
		                    if (this.prestate === null) {
			                    this.prestate = {};
			                    this.lookupAccount(ctx.to, db);
		                    }
		                    this.lookupAccount(ctx.from, db);

		                    var fromBal = bigInt(this.prestate[toHex(ctx.from)].balance.slice(2), 16);
		                    var toBal   = bigInt(this.prestate[toHex(ctx.to)].balance.slice(2), 16);

		                    this.prestate[toHex(ctx.to)].balance   = '0x'+toBal.subtract(ctx.value).toString(16);
		                    this.prestate[toHex(ctx.from)].balance = '0x'+fromBal.add(ctx.value).add(ctx.gasUsed * ctx.gasPrice).toString(16);

		                    this.prestate[toHex(ctx.from)].nonce--;
		                    if (ctx.type == 'CREATE') {
			                    delete this.prestate[toHex(ctx.to)];
		                    }
		                    return this.prestate;
	                    },
	                    step: function(log, db) {

		                    if (this.prestate === null){
			                    this.prestate = {};
			                    this.lookupAccount(log.contract.getAddress(), db);
		                    }

		                    switch (log.op.toString()) {
			                    case 'EXTCODECOPY': case 'EXTCODESIZE': case 'EXTCODEHASH': case 'BALANCE':
				                    this.lookupAccount(toAddress(log.stack.peek(0).toString(16)), db);
				                    break;
			                    case 'CREATE':
				                    var from = log.contract.getAddress();
				                    this.lookupAccount(toContract(from, db.getNonce(from)), db);
				                    break;
			                    case 'CREATE2':
				                    var from = log.contract.getAddress();

				                    var offset = log.stack.peek(1).valueOf()
				                    var size = log.stack.peek(2).valueOf()
				                    var end = offset + size
				                    this.lookupAccount(toContract2(from, log.stack.peek(3).toString(16), log.memory.slice(offset, end)), db);
				                    break;
			                    case 'CALL': case 'CALLCODE': case 'DELEGATECALL': case 'STATICCALL':
				                    this.lookupAccount(toAddress(log.stack.peek(1).toString(16)), db);
				                    break;
			                    case 'SSTORE':case 'SLOAD':
				                    this.lookupStorage(log.contract.getAddress(), toWord(log.stack.peek(0).toString(16)), db);
				                    break;
		                    }
	                    },
	                    fault: function(log, db) {}
                    };
                    preStateTracer;
                ";

            // Execute the JavaScript code
            dynamic preStateTracer = engine.Evaluate(jsCode);
        }
    }
}
