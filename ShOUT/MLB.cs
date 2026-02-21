using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ShOUT
{
    public class MLB
    {
        public List<MLBEntry> Entries { get; set; } = [];

        public static MLB Load(string path)
        {
            FileInfo fi = new(path);
            MLB mlb = new();

            using BinaryReader br = new(fi.OpenRead());

            int mode = 0;

            do
            {
                MLBEntry entry = new()
                {
                    Mode = br.ReadInt32(),
                    Offset = br.ReadInt32(),
                    Width = (int)br.ReadUInt32(),
                    Height = (int)br.ReadUInt32()
                };

                mode = entry.Mode;

                if (mode > 0) { mlb.Entries.Add(entry); }
            } while (mode != 0);

            for (int j = 0; j < mlb.Entries.Count; j++)
            {
                MLBEntry entry = mlb.Entries[j];

                if (entry.Offset > 0 && br.BaseStream.Position != entry.Offset)
                {
                    Console.WriteLine("huh?");
                }

                if (entry.Offset > 0)
                {
                    int offsetCount = br.ReadInt32();
                    entry.UnknownB = br.ReadInt32();

                    for (int i = 0; i < offsetCount; i++)
                    {
                        entry.Offsets.Add(br.ReadInt32());
                    }
                }
            }

            foreach (MLBEntry e in mlb.Entries)
            {
                foreach (int o in e.Offsets)
                {
                    br.BaseStream.Seek(o, SeekOrigin.Begin);

                    e.Data.Add(br.ReadBytes(256 * 256));
                }
            }

            br.Close();

            return mlb;
        }
    }

    public class MLBEntry
    {
        public int Mode { get; set; }

        public int Offset { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int UnknownB { get; set; }

        public List<int> Offsets { get; set; } = [];

        public List<byte[]> Data { get; set; } = [];

        public Bitmap ToBitmap(int offset, List<Color> palette)
        {
            Bitmap bmp = new(Width, Height, PixelFormat.Format32bppArgb);
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            MemoryStream nms = new();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    byte index = Data[offset][y * 256 + x];

                    if (x < Width)
                    {
                        Color c = palette[index];

                        nms.WriteByte(c.B);
                        nms.WriteByte(c.G);
                        nms.WriteByte(c.R);
                        nms.WriteByte(c.A);
                    }
                }
            }

            byte[] contentBuffer = new byte[nms.Length];

            nms.Position = 0;
            nms.Read(contentBuffer, 0, contentBuffer.Length);

            Marshal.Copy(contentBuffer, 0, bmpdata.Scan0, contentBuffer.Length);

            nms.Close();

            bmp.UnlockBits(bmpdata);

            return bmp;
        }
    }
}
