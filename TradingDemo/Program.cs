using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TradingDemo
{
    static class Program
    {
        public const int MAX_SYMBOLS = 1000;
        public const int MAX_ACCOUNTS = 1000;
        public const int MAX_GROUPS = 100;

        static readonly Symbol[] symbols = new Symbol[MAX_SYMBOLS];
        static readonly Account[] accounts = new Account[MAX_ACCOUNTS];
        static readonly Group[] groups = new Group[MAX_ACCOUNTS];
        static readonly int[] positionMatrix = new int[MAX_SYMBOLS * MAX_ACCOUNTS];

        static void Main()
        {
            Initialize();
            var sw = Stopwatch.StartNew();

            for (var j = 0; j < 1000; j++)
                for (var i = 0; i < MAX_SYMBOLS; i++)
                {
                    ProcessTick(i, i+10);
                }

            Console.WriteLine(sw.Elapsed);
            for (var i = 0; i < 10; i++)
                Console.WriteLine(groups[i].TotalSum);
        }

        static unsafe void Initialize()
        {
            for (var i = 0; i < MAX_SYMBOLS; i++)
            {
                symbols[i] = new Symbol(1);
                for (var j = 0; j < MAX_ACCOUNTS; j++)
                {
                    fixed (int* z = symbols[i].Accounts)
                        z[j] = j;
                }
            }

            for (var i = 0; i < MAX_ACCOUNTS; i++)
            {
                accounts[i] = new Account(i / 10);
            }
            for (var i = 0; i < MAX_GROUPS; i++)
            {
                groups[i] = new Group();
            }

            for (var i = 0; i < MAX_SYMBOLS * MAX_ACCOUNTS; i++)
            {
                positionMatrix[i] = i;
            }
        }

        static unsafe void ProcessTick(int symbolId, int price)
        {
            fixed (Group* group = groups)
            fixed (Account* accountPtr = accounts)
            fixed (Symbol* symbolPtr = symbols)
            fixed (int* posPtr = positionMatrix)
            {
                var symbol = symbolPtr[symbolId];
                var oldPrice = symbol.CurrentPrice;
                symbol.CurrentPrice = price;
                var diff = price - oldPrice;
                int* ptr = symbol.Accounts;
                for (var i = 0; i < Program.MAX_ACCOUNTS; i++)
                {
                    var val = ptr[i];
                    if (val == -1) break;
                    int index = symbolId * MAX_ACCOUNTS + val;
                    var amount = posPtr[index];
                    var change = amount * diff;
                    var account = accountPtr[val];
                    account.TotalSum += change;
                    group[account.GroupId].TotalSum += change;
                }
            }
        }
    }

    unsafe struct Symbol
    {

        public Symbol(int price)
        {
            this.CurrentPrice = price;
            fixed (int* ptr = Accounts)
            {
                for (var i = 0; i < Program.MAX_ACCOUNTS; i++)
                {
                    ptr[i] = -1;
                }
            }
        }

        public int CurrentPrice;
        public fixed int Accounts[Program.MAX_ACCOUNTS];
    }

    struct Account
    {
        public readonly int GroupId;
        public int TotalSum;

        public Account(int groupId)
        {
            this.GroupId = groupId;
            this.TotalSum = 0;
        }
    }

    struct Group
    {
        public int TotalSum;
    }


}
