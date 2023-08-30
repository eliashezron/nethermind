using System;
namespace Nethermind.Evm.Tracing.GethStyle
{
    public class GethJavascriptStyleLog : GethTxTraceEntry
    {
        public double pc { get; set; }
        public string op { get; set; }
        public double gas { get; set; }
        public double gasCost { get; set; }
        public double depth { get; set; }
        public string getOP()
        {
            return Opcode;
        }
    }
}

