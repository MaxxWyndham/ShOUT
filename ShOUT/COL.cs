using System.Drawing;

namespace ShOUT
{
    public class COL
    {
        public List<Color> Palette { get; set; } = [];

        public static COL Load(string path)
        {
            FileInfo fi = new(path);
            COL col = new();

            using BinaryReader pal = new(fi.OpenRead());

            while (pal.BaseStream.Position < pal.BaseStream.Length)
            {
                col.Palette.Add(Color.FromArgb(255, pal.ReadByte() * 4, pal.ReadByte() * 4, pal.ReadByte() * 4));
            }

            pal.Close();

            return col;
        }
    }
}
