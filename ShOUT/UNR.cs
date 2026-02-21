namespace ShOUT
{
    public class UNR
    {
        List<Screamer3DX> Meshes { get; set; } = [];

        public static UNR Load(string path)
        {
            FileInfo fi = new(path);
            UNR unr = new();

            BinaryReader br = new(fi.OpenRead());

            int dataLength = br.ReadInt32(); // some sort of length
            int meshCount = br.ReadInt32();
            List<int> offsets = [];

            for (int i = 0; i < meshCount; i++)
            {
                offsets.Add(br.ReadInt32());
            }

            // work in progress, danger, danger!

            for (int i = 0; i < meshCount; i++)
            {
                int length = (i + 1 < meshCount ? offsets[i + 1] : dataLength) - offsets[i];

                Console.WriteLine($"{i} => {br.BaseStream.Position:x2} :: {length}");

                using TextWriter tw = new StreamWriter($@"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}-{i}.obj");

                using MemoryStream ms = new(br.ReadBytes(length));
                //unr.Meshes.Add(Screamer3DX.Load(ms, tw));
                ms.Close();

                tw.Close();
            }

            br.Close();

            

            return unr;
        }
    }
}
