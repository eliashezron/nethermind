// function trace(x) {
//     let y = [];
//     y.push(debug.traceTransaction(x, { disableMemory: false, disableStack: false, disableStorage: false }))
//     y.push(debug.traceTransaction("0x8a9d1762f88474ea31273a0d3aaf107f45ee24c0c3adc42a03d8db92302c1130", { disableMemory: false, disableStack: false, disableStorage: false,tracer:'{' +  'retVal: [],' + 'step: function(log,db) {this.retVal.push(log.getPC() + ":" + log.op.toString())},' + 'fault: function(log,db) {this.retVal.push("FAULT: " + JSON.stringify(log))},' + 'result: function(ctx,db) {return this.retVal}' +  '}'  }))
//     return y;
// }
//
// // function trace(x) {
// //     const originalTraceResults = debug.traceTransaction(x, { disableMemory: false, disableStack: false, disableStorage: false });
// //     const filteredTraceResults = [];
// //
// //     originalTraceResults.forEach(p => {
// //         filteredTraceResults.push({ op: p.op, pc: p.pc });
// //     });
// //
// //     return filteredTraceResults;
// // }
//
// // function main() {
// //     const numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
// //     return numbers.map(num => ({ op: num * 2, pc: num * 2 }));
// // }
// //
// // const result = main();
// // console.log(result);
//
//
// { tracer:'{' +  'retVal: [],' + 'step: function(log,db) {this.retVal.push(log.getPC() + ":" + log.op.toString())},' + 'fault: function(log,db) {this.retVal.push("FAULT: " + JSON.stringify(log))},' + 'result: function(ctx,db) {return this.retVal}' +  '}'}
//
//
//
// y.push(debug.traceTransaction("0x4132ab601e17fc3b0b0aad162d2655a1bde1fa08feda4176a16005d2c013b139", { disableMemory: false, disableStack: false, disableStorage: false}))
//
//
//
//
