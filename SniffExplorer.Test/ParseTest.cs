using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;

namespace SniffExplorer.Test
{
    [TestClass]
    public class ParseTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                #region PKT bullshit
                writer.Write(new byte[3 + 2 + 1]);
                writer.Write(20772); // Build
                writer.Write(System.Text.Encoding.UTF8.GetBytes("enUS")); // Locale
                writer.Write(new byte[40 + 4 + 4 + 4]);

                // Write a single dummy opcode
                writer.Write(0x47534D43u); // CMSG
                writer.Write(0); // Connection ID
                writer.Write(1489646141); // Timestamp
                writer.Write(0); // Opt. Data Length
                var sizeOffset = ms.Position;
                writer.Write(0); // placeholder
                writer.Write((int) OpcodeClient.CMSG_AUTH_SESSION);

                var ofsStart = ms.Position;
                #endregion

                writer.Write(1000ul); // DosResponse
                writer.Write((ushort) 15595); // Build
                writer.Write((byte) 1); // BuildType
                writer.Write((uint) 2); // RegionID
                writer.Write((uint) 3); // BattlegroundID
                writer.Write((uint) 4); // RealmID

                // LocalChallenge
                foreach (var i in Enumerable.Range(0, 16))
                    writer.Write((byte) i); // 0, 1, ..., 15

                // Digest
                foreach (var i in Enumerable.Range(100, 24))
                    writer.Write((byte) i); // 100, 101, ..., 123

                // UsesIPv6
                writer.Write((byte) 0xFF); // all bits set, cheap, but just to make it work

                // RealmJoinTicket
                const string realmJoinTicket = "!Warpten:nowyouseeme@blizzturd.com";
                writer.Write(realmJoinTicket.Length);
                writer.Write(System.Text.Encoding.UTF8.GetBytes(realmJoinTicket));

                #region PKT stuff
                // Get actual data size, skip back to position, write size
                ofsStart = ms.Position - ofsStart;
                ms.Position = sizeOffset;
                writer.Write((int) ofsStart + 4);

                // Aaaand back to the start we go
                ms.Position = 0;
                #endregion
                BinaryProcessor.Process(ms);

                Console.WriteLine("{0}", Store.Opcodes.Count);
            }

        }
    }
}
