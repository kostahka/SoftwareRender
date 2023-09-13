using SoftwareRender.RenderConveyor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using static SoftwareRender.Model;

namespace SoftwareRender
{
    internal class ObjParser
    {
        static float ParseFloat(string text)
        {
            float res = 0;
            try
            {
                CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";
                res = float.Parse(text, NumberStyles.Any, ci);
            }
            catch { }
            return res;
        }
        public static Model parse(String file_path)
        {
            List<Vector4> vertices = new List<Vector4>();
            List<Vector3> texture_uv = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            Dictionary<PrimitiveType, List<int>> verticesIndexes = new ();
            Dictionary<PrimitiveType, List<int>> textureIndexes = new ();
            Dictionary<PrimitiveType, List<int>> normalsIndexes = new ();
            using (var reader = new StreamReader(stream: new FileStream(file_path, FileMode.Open)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    if (line.StartsWith("v "))
                    {
                        var tokens = line.Split(' ');
                        float x = ParseFloat(tokens[1]);
                        float y = ParseFloat(tokens[2]);
                        float z = ParseFloat(tokens[3]);

                        if (tokens.Length > 4)
                        {
                            float w = ParseFloat(tokens[4]);
                            x /= w;
                            y /= w;
                            z /= w;
                        }
                        vertices.Add(new Vector4(x, y, z, 1f));
                    }
                    else if(line.StartsWith("vt "))
                    {
                        var tokens = line.Split(' ');
                        Vector3 tex_uv = new Vector3(0);
                        tex_uv.X = ParseFloat(tokens[1]);
                        if(tokens.Length > 2)
                        {
                            tex_uv.Y = ParseFloat(tokens[2]);
                        }
                        if(tokens.Length > 3)
                        {
                            tex_uv.Z = ParseFloat(tokens[3]);
                        }
                        texture_uv.Add(tex_uv);
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var tokens = line.Split(' ');
                        var x = ParseFloat(tokens[1]);
                        var y = ParseFloat(tokens[2]);
                        var z = ParseFloat(tokens[3]);
                        normals.Add(new Vector3(x, y, z));
                    }
                    else if (line.StartsWith("f "))
                    {
                        var tokens = line.Split(' ');

                        PrimitiveType type = tokens.Length == 4 ? PrimitiveType.triangles : PrimitiveType.quads;
                            
                        for(int i = 1; i < tokens.Length; i++)
                        {
                            var str_indices = tokens[i].Split('/');
                            VertexIndexes vertexIndexes = new VertexIndexes(int.Parse(str_indices[0]));
                            if(str_indices.Length > 1 && str_indices[1].Length != 0)
                            {
                                vertexIndexes.t_i = int.Parse(str_indices[1]);
                            }
                            if(str_indices.Length > 2 && str_indices[2].Length != 0)
                            {
                                vertexIndexes.n_i = int.Parse(str_indices[2]);
                            }

                            if (!verticesIndexes.ContainsKey(type))
                            {
                                verticesIndexes.Add(type, new List<int>());
                                textureIndexes.Add(type, new List<int>());
                                normalsIndexes.Add(type, new List<int>());
                            }

                            verticesIndexes[type].Add(vertexIndexes.v_i);
                            textureIndexes[type].Add(vertexIndexes.t_i);
                            normalsIndexes[type].Add(vertexIndexes.n_i);
                        }
                    }
                }
            }

            return new Model(vertices, texture_uv, normals, verticesIndexes, textureIndexes, normalsIndexes);
        }
    }
}
