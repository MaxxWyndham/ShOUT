using System.Numerics;

namespace ShOUT
{
    public class Screamer3DX
    {
        public Dictionary<int, List<Screamer3DXLOD>> LODs { get; set; } = [];

        public static Screamer3DX Load(string path)
        {
            Screamer3DX threedx = new();

            using (MemoryStream ms = new(File.ReadAllBytes(path)))
            {
                threedx = Load(ms);
            }

            return threedx;
        }

        public static Screamer3DX Load(Stream data)
        {
            Screamer3DX threedx = new();

            Dictionary<int, List<int>> lodOffsets = [];

            using BinaryReader br = new(data);

            _ = br.ReadBytes(0x14);
            uint count = br.ReadUInt32();

            for (int i = 1; i < 4; i++)
            {
                lodOffsets[i] = [];

                for (int j = 0; j < 64; j++)
                {
                    int offset = br.ReadInt32();

                    if (offset > 0) { lodOffsets[i].Add(offset); }
                }

                _ = br.ReadUInt32(); // null
            }

            for (int level = 1; level <= 1; level++)
            {
                threedx.LODs[level] = [];

                int vOffset = 0;

                foreach (int offset in lodOffsets[level])
                {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");

                    Screamer3DXLOD lod = new();

                    Console.WriteLine($"{br.ReadInt32()}");
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt16());

                    int unknownOffset = br.ReadInt32();
                    int vertexCount = br.ReadInt32();
                    int faceCount = br.ReadInt32();

                    Console.WriteLine($"Vertex Count: {vertexCount}");
                    Console.WriteLine($"Face Count: {faceCount}");

                    int unk1 = br.ReadInt32();

                    Console.WriteLine($"{unk1} :: {unk1:x2}");

                    int vertexOffset = br.ReadInt32();
                    int normalOffset = br.ReadInt32();
                    int uvOffset = br.ReadInt32();

                    // 16 offsets/lengths of something, the step between them is consistent for a given 3dx but different between 3dx files
                    // the fact there are 16 would suggest it is linked to the data set near the end of the file that also loops 16 times?
                    for (int i = 0; i < 16; i++)
                    {
                        int n = br.ReadInt32();

                        //Console.WriteLine($"{i} : {n} :: {n:x2}");
                    }

                    lod.Name = br.ReadString(16);

                    Console.WriteLine(lod.Name);

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");

                    for (int i = 0; i < faceCount; i++)
                    {
                        Screamer3DXFace face = new();

                        _ = br.ReadBytes(0x4);
                        face.MaterialId = br.ReadInt16() / 16;

                        int nVerts = br.ReadInt16();

                        for (int j = 0; j <= nVerts; j++)
                        {
                            face.Vertices.Add(br.ReadInt16() / 6);
                        }

                        lod.Faces.Add(face);

                        _ = br.ReadBytes(0x1c - (face.Vertices.Count * 2));
                        //int z = (0x1c - (face.Vertices.Count * 2)) / 2;

                        //for (int j = 0; j < z; j++)
                        //{
                        //    Console.WriteLine($"{br.BaseStream.Position:x2} => {br.ReadUInt16()}");
                        //}
                    }

                    _ = br.ReadBytes(0x24); // null

                    Console.WriteLine($":: {br.BaseStream.Position:x2} == {vertexOffset:x2} ::");

                    for (int i = 0; i < vertexCount; i++)
                    {
                        int n = i * 8;

                        lod.Vertices.Add(new(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()));
                    }

                    _ = br.ReadBytes(0x6); // 0xff7f 0xff7f 0xff7f


                    _ = br.ReadBytes((int)(br.BaseStream.Position % 4)); // padding to realign data

                    Console.WriteLine($":: {br.BaseStream.Position:x2} == {normalOffset:x2} ::");

                    int normalCount = br.ReadInt32();

                    Console.WriteLine($"Normal Count: {normalCount}");

                    if (normalCount > 0)
                    {
                        for (int i = 0; i < normalCount; i++)
                        {
                            lod.Normals.Add(new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                        }

                        _ = br.ReadBytes(0xc); // null
                        _ = br.ReadBytes(0x4); // null
                    }

                    _ = br.ReadBytes(0x4); // null

                    Console.WriteLine($":: {br.BaseStream.Position:x2} == {uvOffset:x2} ::");

                    // U V MaterialIndex?
                    for (int i = 0; i < vertexCount; i++)
                    {
                        lod.UVs.Add(new(br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16()));
                    }

                    _ = br.ReadBytes(0x6); // 0xffff 0xffff 0xffff
                    _ = br.ReadBytes(0x6); // null

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");

                    for (int i = 0; i < 16; i++)
                    {
                        ushort j = 0;

                        // (j - 120) / 36 falls into the range of 0 to faceCount
                        // 36 (or 0x24) is the size of the records looped by faceCount
                        // memory offsets rather than indexes!

                        do
                        {
                            j = br.ReadUInt16();

                            //if (j != 0xffff) { Console.WriteLine($"{(j - 120) / 36}"); }
                        } while (j != 0xffff);

                        //Console.WriteLine();
                    }

                    Console.WriteLine($":: {br.BaseStream.Position:x2} == {unknownOffset:x2} ::");

                    int unkA = br.ReadInt32();
                    int unkB = br.ReadInt32();
                    int unkC = br.ReadInt32();
                    int unkD = br.ReadInt32();
                    lod.Position = new(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                    int unkE = br.ReadInt32();
                    int unkF = br.ReadInt32();

                    Console.WriteLine();

                    threedx.LODs[level].Add(lod);
                }
            }

            br.Close();

            return threedx;
        }
    }

    public class Screamer3DXLOD
    {
        public string Name { get; set; }

        public List<Screamer3DXFace> Faces { get; set; } = [];

        public List<Vector3> Vertices { get; set; } = [];

        public List<Vector3> Normals { get; set; } = [];

        public List<Vector3> UVs { get; set; } = [];

        public Vector3 Position { get; set; }
    }

    public class Screamer3DXFace
    {
        public int MaterialId { get; set; }

        public List<int> Vertices { get; set; } = [];
    }
}
