namespace ShOUT
{
    public class DAT
    {
        protected static Dictionary<string, char> Lookup = new() 
        {
            ["0a"] = '0',
            ["0b"] = '1',
            ["08"] = '2',
            ["09"] = '3',
            ["0e"] = '4',
            ["0f"] = '5',
            ["0c"] = '6',
            ["0d"] = '7',
            ["02"] = '8',
            ["03"] = '9',
            ["14"] = '.',
            ["7b"] = 'A',
            ["78"] = 'B',
            ["79"] = 'C',
            ["7e"] = 'D',
            ["7f"] = 'E',
            ["7c"] = 'F',
            ["7d"] = 'G',
            ["72"] = 'H',
            ["73"] = 'I',
            ["70"] = 'J',
            ["71"] = 'K',
            ["76"] = 'L',
            ["77"] = 'M',
            ["74"] = 'N',
            ["75"] = 'O',
            ["6a"] = 'P',
            ["6b"] = 'Q',
            ["68"] = 'R',
            ["69"] = 'S',
            ["6e"] = 'T',
            ["6f"] = 'U',
            ["6c"] = 'V',
            ["6d"] = 'W',
            ["62"] = 'X',
            ["63"] = 'Y',
            ["60"] = 'Z'
        };

        protected string location;

        public List<DATEntry> Entries { get; set; } = [];

        public static DAT Load(string path)
        {
            FileInfo file = new(path);
            DAT dat = new()
            {
                location = path
            };

            using BinaryReader br = new(file.OpenRead());

            br.BaseStream.Seek(-4, SeekOrigin.End);

            uint entryLength = br.ReadUInt32();
            uint entryCount = entryLength / 0x20 - 1;

            br.BaseStream.Seek(-4 - entryLength, SeekOrigin.End);

            for (int i = 0; i < entryCount; i++)
            {
                dat.Entries.Add(new DATEntry
                {
                    Name = NameLookup(br.ReadBytes(16)),
                    Start = br.ReadUInt32(),
                    Length = br.ReadUInt32(),
                    Size = br.ReadUInt32(),
                    Unknown = br.ReadUInt32()
                });
            }

            br.Close();

            return dat;
        }

        public void Extract(DATEntry file, string destination)
        {
            if (!Directory.Exists(destination)) { Directory.CreateDirectory(destination); }

            BinaryWriter bw = new(new FileStream(Path.Combine(destination, file.Name), FileMode.Create));
            BinaryReader br = new(new FileInfo(location).OpenRead());

            br.BaseStream.Seek(file.Start, SeekOrigin.Begin);

            byte[] buff = new byte[file.Size];

            if (file.Unknown == 0)
            {
                buff = br.ReadBytes((int)file.Length);
            }
            else
            {
                new LZWX.LZWX().Decompress(br.ReadBytes((int)file.Length), (int)file.Size, buff);
            }

            bw.Write(buff);

            br.Close();
            bw.Close();
        }

        private static string NameLookup(byte[] bytes)
        {
            string s = string.Empty;

            foreach (byte b in bytes)
            {
                if (b == 0x3a) { break; }

                string l = $"{b:x2}";

                s += DAT.Lookup.TryGetValue(l, out char value) ? value : "-";
            }

            return s;
        }
    }

    public class DATEntry
    {
        public string Name { get; set; }

        public uint Start { get; set; }

        public uint Length { get; set; }

        public uint Size { get; set; }

        public uint Unknown { get; set; }

        public byte[] Data { get; set; }
    }
}
