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

            for (int level = 1; level <= 3; level++)
            {
                threedx.LODs[level] = [];

                int vOffset = 0;

                foreach (int offset in lodOffsets[level])
                {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);

                    Screamer3DXLOD lod = new();

                    Console.WriteLine($"{br.ReadInt32()}");
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt16());
                    Console.WriteLine(br.ReadInt32());

                    int vertexCount = br.ReadInt32();
                    int faceCount = br.ReadInt32();

                    Console.WriteLine($"Vertex Count: {vertexCount}");
                    Console.WriteLine($"Face Count: {faceCount}");

                    for (int i = 0; i < 20; i++)
                    {
                        int n = br.ReadInt32();

                        Console.WriteLine($"{n} :: {n:x2}");
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

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");
                    // BaseStream.Position == i:1 above

                    for (int i = 0; i < vertexCount; i++)
                    {
                        int n = i * 8;

                        lod.Vertices.Add(new(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()));
                    }

                    _ = br.ReadBytes(0x6); // 0xff7f 0xff7f 0xff7f

                    Console.WriteLine($":[ {br.BaseStream.Position % 4} ]:");

                    _ = br.ReadBytes((int)(br.BaseStream.Position % 4)); // null

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");
                    // BaseStream.Position == i:2 above

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

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");
                    // BaseStream.Position == i:3 above

                    // U V MaterialIndex?
                    for (int i = 0; i < vertexCount; i++)
                    {
                        lod.UVs.Add(new(br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16()));
                    }

                    _ = br.ReadBytes(0x6); // 0xffff 0xffff 0xffff
                    _ = br.ReadBytes(0x2); // null

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");
                    // BaseStream.Position == i:11 above

                    _ = br.ReadBytes(4); // null

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

                    Console.WriteLine($":: {br.BaseStream.Position:x2} ::");
                    // BaseStream.Position == unknown1

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
    }

    public class Screamer3DXFace
    {
        public int MaterialId { get; set; }

        public List<int> Vertices { get; set; } = [];
    }
}
