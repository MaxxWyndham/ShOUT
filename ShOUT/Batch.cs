using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Numerics;
using System.Text;
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
                        entry.ToBitmap(col.Palette).Save(Path.Combine(item.DirectoryName, $"{Path.GetFileNameWithoutExtension(item.Name)}-{i}-{j}.png"), ImageFormat.Png);
                    }
                }
            }

            // convert all models
            foreach (var item in new DirectoryInfo(path).GetFiles("*.3dx"))
            {
                Console.WriteLine($"Processing {item.FullName}...");

                Screamer3DX threedx = Screamer3DX.Load(item.FullName);

                foreach (int level in threedx.LODs.Keys)
                {
                    using TextWriter tw = new StreamWriter($@"{Path.Combine(Path.GetDirectoryName(item.FullName), Path.GetFileNameWithoutExtension(item.FullName))}-{level}.obj");
                    int vOffset = 0;

                    foreach (Screamer3DXLOD lod in threedx.LODs[level])
                    {
                        tw.WriteLine($"o {lod.Name}");

                        foreach (Vector3 v in lod.Vertices)
                        {
                            tw.WriteLine($"v {v.X} {v.Y} {v.Z} 1");
                        }

                        foreach (List<int> verts in lod.Faces)
                        {
                            tw.Write($"f");
                            foreach (int v in verts)
                            {
                                tw.Write($" {v + 1 + vOffset}");
                            }
                            tw.WriteLine();
                        }

                        vOffset += lod.Vertices.Count;
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
