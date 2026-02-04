namespace Stroke.Tests.Contrib.Telnet;

using Stroke.Contrib.Telnet;
using Xunit;

/// <summary>
/// Unit tests for <see cref="TelnetProtocolParser"/>.
/// </summary>
public class TelnetProtocolParserTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataReceived_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TelnetProtocolParser(
            null!,
            (rows, cols) => { },
            ttype => { }));
    }

    [Fact]
    public void Constructor_WithNullSizeReceived_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TelnetProtocolParser(
            data => { },
            null!,
            ttype => { }));
    }

    [Fact]
    public void Constructor_WithNullTtypeReceived_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TelnetProtocolParser(
            data => { },
            (rows, cols) => { },
            null!));
    }

    [Fact]
    public void Constructor_WithValidCallbacks_CreatesParser()
    {
        var parser = new TelnetProtocolParser(
            data => { },
            (rows, cols) => { },
            ttype => { });

        Assert.NotNull(parser);
    }

    #endregion

    #region Normal Data Passthrough

    [Fact]
    public void Feed_WithPlainData_InvokesDataReceived()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        parser.Feed("Hello"u8);

        Assert.Equal("Hello"u8.ToArray(), receivedData.ToArray());
    }

    [Fact]
    public void Feed_WithEmptyData_DoesNotInvokeCallback()
    {
        var callCount = 0;
        var parser = CreateParser(data => callCount++);

        parser.Feed([]);

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Feed_WithNopBytes_PassesThroughAsData()
    {
        // FR-003c: NOP (0x00) bytes should pass through as data
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        parser.Feed([0x00, 0x41, 0x00]); // NOP, 'A', NOP

        Assert.Equal([0x00, 0x41, 0x00], receivedData.ToArray());
    }

    #endregion

    #region Double-IAC Escape (FR-016)

    [Fact]
    public void Feed_WithDoubleIac_EmitsSingle0xFF()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // IAC IAC should produce single 0xFF
        parser.Feed([TelnetConstants.IAC, TelnetConstants.IAC]);

        Assert.Single(receivedData);
        Assert.Equal(0xFF, receivedData[0]);
    }

    [Fact]
    public void Feed_WithDataSurroundingDoubleIac_PreservesData()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // "A" IAC IAC "B"
        parser.Feed([0x41, TelnetConstants.IAC, TelnetConstants.IAC, 0x42]);

        Assert.Equal([0x41, 0xFF, 0x42], receivedData.ToArray());
    }

    #endregion

    #region IAC Command Handling (FR-003a)

    [Fact]
    public void Feed_WithIacDo_DoesNotEmitData()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // IAC DO ECHO - should be consumed, not emitted as data
        parser.Feed([TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.ECHO]);

        Assert.Empty(receivedData);
    }

    [Fact]
    public void Feed_WithIacDont_DoesNotEmitData()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        parser.Feed([TelnetConstants.IAC, TelnetConstants.DONT, TelnetConstants.ECHO]);

        Assert.Empty(receivedData);
    }

    [Fact]
    public void Feed_WithIacWill_DoesNotEmitData()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        parser.Feed([TelnetConstants.IAC, TelnetConstants.WILL, TelnetConstants.SGA]);

        Assert.Empty(receivedData);
    }

    [Fact]
    public void Feed_WithIacWont_DoesNotEmitData()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        parser.Feed([TelnetConstants.IAC, TelnetConstants.WONT, TelnetConstants.SGA]);

        Assert.Empty(receivedData);
    }

    [Fact]
    public void Feed_WithDataAfterIacCommand_ContinuesNormally()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // IAC DO ECHO then "Hello"
        parser.Feed([TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.ECHO, 0x48, 0x69]);

        Assert.Equal([0x48, 0x69], receivedData.ToArray());
    }

    #endregion

    #region Simple Commands (FR-003b)

    [Theory]
    [InlineData(TelnetConstants.DM)]
    [InlineData(TelnetConstants.BRK)]
    [InlineData(TelnetConstants.IP)]
    [InlineData(TelnetConstants.AO)]
    [InlineData(TelnetConstants.AYT)]
    [InlineData(TelnetConstants.EC)]
    [InlineData(TelnetConstants.EL)]
    [InlineData(TelnetConstants.GA)]
    public void Feed_WithSimpleCommand_DoesNotEmitData(byte command)
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        parser.Feed([TelnetConstants.IAC, command]);

        Assert.Empty(receivedData);
    }

    [Fact]
    public void Feed_WithSimpleCommandThenData_ContinuesNormally()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // IAC BRK then "X"
        parser.Feed([TelnetConstants.IAC, TelnetConstants.BRK, 0x58]);

        Assert.Equal([0x58], receivedData.ToArray());
    }

    #endregion

    #region NAWS Parsing (FR-004)

    [Fact]
    public void Feed_WithValidNaws_InvokesSizeReceived()
    {
        var (rows, columns) = (0, 0);
        var parser = CreateParser(sizeReceived: (r, c) => (rows, columns) = (r, c));

        // IAC SB NAWS <width_hi> <width_lo> <height_hi> <height_lo> IAC SE
        // Width: 80 = 0x0050, Height: 24 = 0x0018
        parser.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x50, // Width: 80
            0x00, 0x18, // Height: 24
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(24, rows);
        Assert.Equal(80, columns);
    }

    [Fact]
    public void Feed_WithLargeNawsValues_ReportsCorrectSize()
    {
        var (rows, columns) = (0, 0);
        var parser = CreateParser(sizeReceived: (r, c) => (rows, columns) = (r, c));

        // Width: 1920 = 0x0780, Height: 1080 = 0x0438
        parser.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x07, 0x80, // Width: 1920
            0x04, 0x38, // Height: 1080
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(1080, rows);
        Assert.Equal(1920, columns);
    }

    [Fact]
    public void Feed_WithInvalidNawsLength_DoesNotInvokeSizeReceived()
    {
        var callCount = 0;
        var parser = CreateParser(sizeReceived: (r, c) => callCount++);

        // Only 3 bytes of NAWS data instead of 4
        parser.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x50, 0x00, // Missing 4th byte
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(0, callCount);
    }

    #endregion

    #region TTYPE Parsing (FR-005)

    [Fact]
    public void Feed_WithValidTtype_InvokesTtypeReceived()
    {
        var receivedTtype = string.Empty;
        var parser = CreateParser(ttypeReceived: ttype => receivedTtype = ttype);

        // IAC SB TTYPE IS <terminal-type> IAC SE
        var ttypeData = "xterm-256color"u8.ToArray();
        var sequence = new List<byte>
        {
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS
        };
        sequence.AddRange(ttypeData);
        sequence.AddRange([TelnetConstants.IAC, TelnetConstants.SE]);

        parser.Feed(sequence.ToArray());

        Assert.Equal("xterm-256color", receivedTtype);
    }

    [Fact]
    public void Feed_WithTtypeVT100_ReportsVT100()
    {
        var receivedTtype = string.Empty;
        var parser = CreateParser(ttypeReceived: ttype => receivedTtype = ttype);

        var ttypeData = "VT100"u8.ToArray();
        var sequence = new List<byte>
        {
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS
        };
        sequence.AddRange(ttypeData);
        sequence.AddRange([TelnetConstants.IAC, TelnetConstants.SE]);

        parser.Feed(sequence.ToArray());

        Assert.Equal("VT100", receivedTtype);
    }

    [Fact]
    public void Feed_WithTtypeWithoutIs_DoesNotInvokeCallback()
    {
        var callCount = 0;
        var parser = CreateParser(ttypeReceived: ttype => callCount++);

        // TTYPE without IS marker (using SEND instead)
        parser.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.SEND, // Wrong marker
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(0, callCount);
    }

    #endregion

    #region Partial IAC at Buffer End (EC-001)

    [Fact]
    public void Feed_WithPartialIacAtEnd_RetainsState()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // First feed ends with IAC
        parser.Feed([0x41, TelnetConstants.IAC]);

        // Second feed completes the double-IAC
        parser.Feed([TelnetConstants.IAC, 0x42]);

        Assert.Equal([0x41, 0xFF, 0x42], receivedData.ToArray());
    }

    [Fact]
    public void Feed_WithPartialSubnegotiation_RetainsState()
    {
        var (rows, columns) = (0, 0);
        var parser = CreateParser(sizeReceived: (r, c) => (rows, columns) = (r, c));

        // Feed NAWS in multiple chunks
        parser.Feed([TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS]);
        parser.Feed([0x00, 0x50]); // Width: 80
        parser.Feed([0x00, 0x18]); // Height: 24
        parser.Feed([TelnetConstants.IAC, TelnetConstants.SE]);

        Assert.Equal(24, rows);
        Assert.Equal(80, columns);
    }

    [Fact]
    public void Feed_WithPartialIacCommand_RetainsState()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // First feed: "A" + IAC + DO
        parser.Feed([0x41, TelnetConstants.IAC, TelnetConstants.DO]);

        // Second feed: ECHO + "B"
        parser.Feed([TelnetConstants.ECHO, 0x42]);

        Assert.Equal([0x41, 0x42], receivedData.ToArray());
    }

    #endregion

    #region Subnegotiation Buffer Overflow (EC-007)

    [Fact]
    public void Feed_WithOversizedSubnegotiation_DoesNotCrash()
    {
        var callCount = 0;
        var parser = CreateParser(sizeReceived: (r, c) => callCount++);

        // Create subnegotiation larger than 1024 bytes
        var sequence = new List<byte>
        {
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS
        };
        for (int i = 0; i < 1500; i++)
        {
            sequence.Add(0x00);
        }
        sequence.AddRange([TelnetConstants.IAC, TelnetConstants.SE]);

        // Should not throw
        var ex = Record.Exception(() => parser.Feed(sequence.ToArray()));

        Assert.Null(ex);
    }

    #endregion

    #region Malformed Sequences

    [Fact]
    public void Feed_WithUnknownIacCommand_DoesNotCrash()
    {
        var receivedData = new List<byte>();
        var parser = CreateParser(data => receivedData.AddRange(data.ToArray()));

        // Unknown command (0xF5 is not a standard telnet command)
        parser.Feed([0x41, TelnetConstants.IAC, 0xF5, 0x42]);

        // Should continue normally - "A" before, "B" after
        Assert.Contains((byte)0x41, receivedData);
        Assert.Contains((byte)0x42, receivedData);
    }

    [Fact]
    public void Feed_WithEmptySubnegotiation_DoesNotCrash()
    {
        var parser = CreateParser();

        // Empty subnegotiation (SB immediately followed by SE)
        var ex = Record.Exception(() => parser.Feed([
            TelnetConstants.IAC, TelnetConstants.SB,
            TelnetConstants.IAC, TelnetConstants.SE
        ]));

        Assert.Null(ex);
    }

    #endregion

    #region Mixed Data and Commands

    [Fact]
    public void Feed_WithInterleavedDataAndCommands_ProcessesCorrectly()
    {
        var receivedData = new List<byte>();
        var (rows, columns) = (0, 0);
        var receivedTtype = string.Empty;

        var parser = new TelnetProtocolParser(
            data => receivedData.AddRange(data.ToArray()),
            (r, c) => (rows, columns) = (r, c),
            ttype => receivedTtype = ttype);

        // Complex sequence: data, NAWS, data, command, data
        var sequence = new List<byte>();
        sequence.AddRange("AB"u8.ToArray()); // Data
        sequence.AddRange([ // NAWS
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x50, 0x00, 0x18,
            TelnetConstants.IAC, TelnetConstants.SE
        ]);
        sequence.AddRange("CD"u8.ToArray()); // Data
        sequence.AddRange([TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.ECHO]); // Command
        sequence.AddRange("EF"u8.ToArray()); // Data

        parser.Feed(sequence.ToArray());

        Assert.Equal("ABCDEF"u8.ToArray(), receivedData.ToArray());
        Assert.Equal(24, rows);
        Assert.Equal(80, columns);
    }

    #endregion

    #region Helper Methods

    private static TelnetProtocolParser CreateParser(
        TelnetProtocolParser.DataReceivedCallback? dataReceived = null,
        TelnetProtocolParser.SizeReceivedCallback? sizeReceived = null,
        TelnetProtocolParser.TtypeReceivedCallback? ttypeReceived = null)
    {
        return new TelnetProtocolParser(
            dataReceived ?? (data => { }),
            sizeReceived ?? ((rows, cols) => { }),
            ttypeReceived ?? (ttype => { }));
    }

    #endregion
}
