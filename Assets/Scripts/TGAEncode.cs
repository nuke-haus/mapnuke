using UnityEngine;

public static class TGAEncode
{
    public enum Channel
    {
        R,
        G,
        B,
        A,
        White,
        Black,
        Gray
    }

    private static readonly byte[] TGA_FOOTER = new byte[]
    {
        0, 0, 0, 0, // extension offset
        0, 0, 0, 0, // dev area offset
        (byte)'T',
        (byte)'R',
        (byte)'U',
        (byte)'E',
        (byte)'V',
        (byte)'I',
        (byte)'S',
        (byte)'I',
        (byte)'O',
        (byte)'N',
        (byte)'-',
        (byte)'X',
        (byte)'F',
        (byte)'I',
        (byte)'L',
        (byte)'E',
        (byte)'.',
        0
    };

    /// <summary>
    /// Encode the texture2d into TGA format. Default setup is RGB.
    /// </summary>
    public static byte[] EncodeToTGA(this Texture2D texture, Channel[] channels = null)
    {
        if (channels == null)
        {
            channels = new[]
            {
                Channel.R,
                Channel.G,
                Channel.B
                // Channel.A  
            };
        }

        var num_channels = channels.Length;

        if (num_channels != 3 && num_channels != 4)
        {
            throw new UnityException("Can only save TGA with 3 or 4 channels");
        }

        var pixels = texture.GetPixels32();
        var header_bytes = CreateTGAHeader(texture.width, texture.height, num_channels == 4);
        var tga_bytes = new byte[header_bytes.Length + TGA_FOOTER.Length + pixels.Length * num_channels];
        var cur_byte = header_bytes.Length;

        foreach (var pixel in pixels)
        {
            // RGBA textures are stored as BGRA internally
            tga_bytes[cur_byte + 0] = GetChannel(pixel, channels[2]);         // B
            tga_bytes[cur_byte + 1] = GetChannel(pixel, channels[1]);         // G
            tga_bytes[cur_byte + 2] = GetChannel(pixel, channels[0]);         // R

            if (num_channels == 4)
            {
                tga_bytes[cur_byte + 3] = GetChannel(pixel, channels[3]);     // A
            }

            cur_byte += num_channels;
        }

        System.Array.ConstrainedCopy(header_bytes, 0, tga_bytes, 0, header_bytes.Length);
        System.Array.ConstrainedCopy(TGA_FOOTER, 0, tga_bytes, cur_byte, TGA_FOOTER.Length);

        return tga_bytes;
    }

    private static byte GetChannel(Color32 color, Channel channel)
    {
        switch (channel)
        {
            case Channel.R: return color.r;
            case Channel.G: return color.g;
            case Channel.B: return color.b;
            case Channel.A: return color.a;
            case Channel.Black: return 0;
            case Channel.Gray: return 127;
            default:
            case Channel.White: return 255;
        }
    }

    private static byte[] CreateTGAHeader(int width, int height, bool has_fourth = true)
    {
        return new byte[]
        {
            0, // ID length
            0, // no color map
            2, // uncompressed, true color
            0, 0, 0, 0,
            0,
            0, 0, 0, 0, // x and y origin
            (byte)(width & 0x00FF),
            (byte)((width & 0xFF00) >> 8),
            (byte)(height & 0x00FF),
            (byte)((height & 0xFF00) >> 8),
            (byte)(has_fourth ? 32 : 24), // 32 or 24 bit bitmap
            0
        };
    }
}