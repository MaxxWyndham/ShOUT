using System.Drawing.Imaging;
using System.Numerics;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace ShOUT
{
    public class Batch
    {
        public static void Process(string path)
        {
            // extract all DATs
            foreach (var item in new DirectoryInfo(path).GetFiles("*.dat"))
            {
                if (item.Name.Equals("FAKECD.DAT", StringComparison.CurrentCultureIgnoreCase)) { continue; }
                if (item.Name.Equals("DUMMY.DAT", StringComparison.CurrentCultureIgnoreCase)) { continue; }

                Console.WriteLine($"Processing {item.FullName}...");

                DAT dat = DAT.Load(item.FullName);

                foreach (DATEntry de in dat.Entries)
                {
                    Console.WriteLine(de.Name);
                    Console.WriteLine($"{de.Start:x2} : {de.Length:x2} : {de.Size} : {de.Unknown}");
                    Console.WriteLine();

                    dat.Extract(de, Path.Combine(path, Path.GetFileNameWithoutExtension(item.Name)));
                }
            }

            // extract all textures
            foreach (var item in new DirectoryInfo(path).GetFiles("*.mlb"))
            {
                Console.WriteLine($"Processing {item.FullName}...");

                COL col = COL.Load(Path.Combine(item.DirectoryName, $"{Path.GetFileNameWithoutExtension(item.Name)}.COL"));

                MLB mlb = MLB.Load(item.FullName);

                for (int i = 0; i < mlb.Entries.Count; i++)
                {
                    MLBEntry entry = mlb.Entries[i];

                    Console.WriteLine($"{entry.Mode} : {entry.Offset:x2} : {entry.Width}x{entry.Height} : {entry.UnknownB} : {string.Join(", ", entry.Offsets.Select(o => $"{o:x2}"))}");

                    for (int j = 0; j < entry.Offsets.Count; j++)
                    {
                        //entry.ToBitmap(col.Palette).Save(Path.Combine(item.DirectoryName, $"{Path.GetFileNameWithoutExtension(item.Name)}-{i}-{j}.png"), ImageFormat.Png);
                        entry.ToBitmap(j, col.Palette).Save(Path.Combine(item.DirectoryName, $"{i}-{j}.png"), ImageFormat.Png);
                    }
                }
            }

            // convert all models
            foreach (var item in new DirectoryInfo(path).GetFiles("*.3dx"))
            {
                Console.WriteLine($"Processing {item.FullName}...");

                Screamer3DX threedx = Screamer3DX.Load(item.FullName);

                string meshName = $"{Path.GetFileNameWithoutExtension(item.FullName)}";
                List<string> materials = [];

                foreach (int level in threedx.LODs.Keys)
                {
                    using TextWriter tw = new StreamWriter($@"{Path.Combine(Path.GetDirectoryName(item.FullName), meshName)}-{level}.obj");

                    tw.WriteLine($"mtllib {meshName}.MTL");

                    int vOffset = 0;
                    int lastMaterialId = -1;

                    foreach (Screamer3DXLOD lod in threedx.LODs[level])
                    {
                        tw.WriteLine($"o {lod.Name}");

                        foreach (Vector3 v in lod.Vertices)
                        {
                            tw.WriteLine($"v {-v.X - lod.Position.X} {v.Y + lod.Position.Y} {v.Z + lod.Position.Z} 1");
                        }

                        foreach (Vector3 uv in lod.UVs)
                        {
                            tw.WriteLine($"vt {uv.X / 255f} -{uv.Y / 255f}");
                        }

                        foreach (Screamer3DXFace face in lod.Faces)
                        {
                            if (face.MaterialId != lastMaterialId)
                            {
                                string materialName = $"m{face.MaterialId}";

                                tw.WriteLine($"usemtl {materialName}");

                                if (materials.IndexOf(materialName) < 0) { materials.Add(materialName); }

                                lastMaterialId = face.MaterialId;
                            }

                            tw.Write($"f");
                            foreach (int v in face.Vertices)
                            {
                                tw.Write($" {v + 1 + vOffset}/{v + 1 + vOffset}");
                            }
                            tw.WriteLine();
                        }

                        vOffset += lod.Vertices.Count;
                    }

                    tw.Close();
                }

                if (materials.Count > 0)
                {
                    using TextWriter tw = new StreamWriter($@"{Path.Combine(Path.GetDirectoryName(item.FullName), meshName)}.mtl");

                    foreach (string material in materials)
                    {
                        tw.WriteLine($"newmtl {material}");
                        tw.WriteLine("Ka 1.000000 1.000000 1.000000");
                        tw.WriteLine("Kd 1.000000 1.000000 1.000000");
                        tw.WriteLine("Ks 0.000000 0.000000 0.000000");
                        tw.WriteLine($"map_Kd {material[1..]}-0.png");
                    }

                    tw.Close();
                }
            }

            foreach (var folder in new DirectoryInfo(path).GetDirectories())
            {
                Process(folder.FullName);
            }
        }
    }
}
