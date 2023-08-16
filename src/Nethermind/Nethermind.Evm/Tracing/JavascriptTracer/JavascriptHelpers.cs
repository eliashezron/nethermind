// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only
using System;

namespace Nethermind.Evm.Tracing.JavascriptTracer
{
    public class JavascriptHelpers
    {
        public static string ToHex(string input)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string ToAddress(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static object BigInt(string value)
        {
            // Simulate the bigInt behavior here
            return value;
        }
        public static bool IsPrecompiled(string address)
        {
            // Implement your logic to determine if the address belongs to a precompiled contract
            // For example, you can check if the address falls within a specific range or list of addresses
            // Return true if it's a precompiled contract, false otherwise
            return false;
        }

    }
}
