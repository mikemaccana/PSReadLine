﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.PowerShell;

namespace UnitTestPSReadLine
{
    [TestClass]
    public class KeyInfoConverterTest
    {
        private const ConsoleModifiers NoModifiers = 0;

        [TestMethod]
        public void TestKeyInfoConverterSimpleCharLiteral()
        {
            void TestOne(char keyChar, ConsoleKey key)
            {
                var r = ConsoleKeyChordConverter.Convert(keyChar.ToString());
                Assert.IsNotNull(r);
                Assert.AreEqual(1, r.Length);
                Assert.AreEqual(keyChar, r[0].KeyChar);
                if (key != 0) Assert.AreEqual(key, r[0].Key);
                Assert.AreEqual(NoModifiers, r[0].Modifiers);
            }

            for (char c = 'a'; c <= 'z'; c++)
            {
                TestOne(c, ConsoleKey.A + (c - 'a'));
            }

            for (char c = 'A'; c <= 'Z'; c++)
            {
                TestOne(c, ConsoleKey.A + (c - 'A'));
            }

            for (char c = '0'; c <= '9'; c++)
            {
                TestOne(c, ConsoleKey.D0 + (c - '0'));
            }

            foreach (char c in "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?")
            {
                // Symbols often have different ConsoleKey's depending
                // on the keyboard layout, so we don't verify those
                // (which is fine, because we go out of the way to map
                // those ConsoleKey's to the symbol before looking for
                // key handlers.)
                TestOne(c, 0);
            }
        }

        [TestMethod]
        public void TestKeyInfoConverterSimpleConsoleKey()
        {
            var cases = new [] {
                "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
                "F13", "F14", "F15", "F16", "F17", "F18", "F19", "F20", "F21", "F22", "F23", "F24",
                "Delete", "DownArrow", "End", "Enter", "Home", "LeftArrow", "PageUp", "PageDown",
                "RightArrow", "Tab", "UpArrow",
            };

            var mods = new[]
            {
                Tuple.Create("Control", ConsoleModifiers.Control),
                Tuple.Create("Ctrl", ConsoleModifiers.Control),
                Tuple.Create("Alt", ConsoleModifiers.Alt),
                Tuple.Create("Shift", ConsoleModifiers.Shift),
                Tuple.Create("Ctrl+Shift", ConsoleModifiers.Shift | ConsoleModifiers.Control),
                Tuple.Create("Control+Shift", ConsoleModifiers.Shift | ConsoleModifiers.Control),
                Tuple.Create("Shift+Ctrl", ConsoleModifiers.Shift | ConsoleModifiers.Control),
                Tuple.Create("Shift+Control", ConsoleModifiers.Shift | ConsoleModifiers.Control),
                Tuple.Create("Ctrl+Alt", ConsoleModifiers.Alt | ConsoleModifiers.Control),
                Tuple.Create("Control+Alt", ConsoleModifiers.Alt | ConsoleModifiers.Control),
                Tuple.Create("Alt+Ctrl", ConsoleModifiers.Alt | ConsoleModifiers.Control),
                Tuple.Create("Alt+Control", ConsoleModifiers.Alt | ConsoleModifiers.Control),
                Tuple.Create("Shift+Alt", ConsoleModifiers.Alt | ConsoleModifiers.Shift),
                Tuple.Create("Shift+Alt", ConsoleModifiers.Alt | ConsoleModifiers.Shift),
                Tuple.Create("Alt+Shift", ConsoleModifiers.Alt | ConsoleModifiers.Shift),
                Tuple.Create("Alt+Shift", ConsoleModifiers.Alt | ConsoleModifiers.Shift),
                Tuple.Create("Ctrl+Shift+Alt", ConsoleModifiers.Alt | ConsoleModifiers.Shift | ConsoleModifiers.Control),
                Tuple.Create("Control+Shift+Alt", ConsoleModifiers.Alt | ConsoleModifiers.Shift | ConsoleModifiers.Control),
            };

            void TestOne(string s)
            {
                void VerifyOne(string input, ConsoleModifiers m)
                {
                    var r = ConsoleKeyChordConverter.Convert(input);
                    Assert.IsNotNull(r);
                    Assert.AreEqual(1, r.Length);
                    Assert.AreEqual(Enum.Parse(typeof(ConsoleKey), s), r[0].Key);
                    Assert.AreEqual(m, r[0].Modifiers);
                }

                VerifyOne(s, NoModifiers);
                VerifyOne(s.ToLowerInvariant(), NoModifiers);
                VerifyOne(s.ToUpperInvariant(), NoModifiers);

                foreach (var c in mods)
                {
                    VerifyOne(c.Item1 + "+" + s, c.Item2);
                    VerifyOne(c.Item1.ToLowerInvariant() + "+" + s, c.Item2);
                    VerifyOne(c.Item1.ToUpperInvariant() + "+" + s, c.Item2);
                    VerifyOne(c.Item1.Replace('+', '-') + "-" + s, c.Item2);
                }
            }

            foreach (var c in cases)
            {
                TestOne(c);
            }
        }

        [TestMethod]
        public void TestKeyInfoConverterErrors()
        {
            void TestOne(string s)
            {
                Exception ex = null;
                try
                {
                    ConsoleKeyChordConverter.Convert(s);
                }
                catch (Exception e)
                {
                    ex = e;
                }

                if (!(ex is ArgumentException))
                {
                    Assert.Fail($"Input `${s}` did not throw an ArgumentException");
                }
            }

            var cases = new [] {
                "escrape",
                "alt+shft+x",
                "alt+ctrl+sift+x",
                "alt+control+shifr+x",
                "alt+alt+x",
                "shift+shift+x",
                "ctrl+ctrl+x",
                "control+control+x",
                "control+alt+control+x",
                "control+shift+alt+control+x",
                "shift+",
                "+x",
                "x+",
                "x,",
                ",x",
                "Ctrl+10",
                "Ctrl+1a",
                "Ctrl+ab",
            };

            foreach (var c in cases)
            {
                TestOne(c);
            }
        }

    }
}
