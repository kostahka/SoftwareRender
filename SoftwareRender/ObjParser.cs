using SoftwareRender.Render;
using SoftwareRender.Render.MaterialSupport;
using SoftwareRender.Render.ModelSupport;
using SoftwareRender.RenderConveyor;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SoftwareRender
{
    internal class ObjParser
    {
        static Texture? ParseTextureLine(string line, string mat_path)
        {
            string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

            string? path = System.IO.Path.GetDirectoryName(mat_path);
            if (path == null)
                return null;

            path += "\\" + tokens[1];

            return new Texture(path);
        }
        static Vector3 ParseVectorLine(string line)
        {
            string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

            float x = ParseFloat(tokens[1]);
            float y = ParseFloat(tokens[2]);
            float z = ParseFloat(tokens[3]);
            Vector3 res = new Vector3(x, y, z);
            return res;
        }

        static Dictionary<string, Material> ParseMaterials(string file_path)
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>();
            if (!System.IO.File.Exists(file_path))
                return materials;

            using (var reader = new StreamReader(stream: new FileStream(file_path, FileMode.Open)))
            {
                string? materialName = null;
                Vector3 valAmbient = new(0.2f);
                Vector3 valDiffuse = new(0.5f);
                Vector3 valSpecullar = new(0.8f);
                Texture? texAmbient = null;
                Texture? texDiffuse = null;
                Texture? texSpecullar = null;
                Texture? texNormal = null;
                float SpecNs = 5;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();

                    if (line.StartsWith("newmtl "))
                    {
                        if (materialName != null)
                        {
                            MaterialProperty Ambient = texAmbient == null ?
                                new ConstMaterialProperty(valAmbient) : new TexturedMaterialProperty(texAmbient, valAmbient);

                            MaterialProperty Diffuse = texDiffuse == null ?
                                new ConstMaterialProperty(valDiffuse) : new TexturedMaterialProperty(texDiffuse, valDiffuse);

                            MaterialProperty Specullar = texSpecullar == null ?
                                new ConstMaterialProperty(valSpecullar) : new TexturedMaterialProperty(texSpecullar, valSpecullar);

                            NormalMap? normalMap = texNormal == null ? null : new(texNormal);

                            materials[materialName] = new Material(Ambient, Diffuse, Specullar, normalMap, SpecNs);
                            texAmbient = null;
                            texDiffuse = null;
                            texSpecullar = null;
                            texNormal = null;
                            SpecNs = 5f;
                            materialName = null;
                        }

                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                        materialName = tokens[1];
                    }
                    else if (line.StartsWith("Ka "))
                    {
                        valAmbient = ParseVectorLine(line);
                    }
                    else if (line.StartsWith("Kd "))
                    { 
                        valDiffuse = ParseVectorLine(line);
                    }
                    else if (line.StartsWith("Ks "))
                    {
                        valSpecullar = ParseVectorLine(line);
                    }
                    else if (line.StartsWith("Ns "))
                    {
                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

                        SpecNs = ParseFloat(tokens[1]);
                    }
                    else if (line.StartsWith("map_Ka "))
                    {
                        texAmbient = ParseTextureLine(line, file_path);
                    }
                    else if (line.StartsWith("map_Kd "))
                    {
                        texDiffuse = ParseTextureLine(line, file_path);
                    }
                    else if (line.StartsWith("map_Ks "))
                    {
                        texSpecullar = ParseTextureLine(line, file_path);
                    }
                    else if (line.StartsWith("map_bump ") || line.StartsWith("bump "))
                    {
                        texNormal = ParseTextureLine(line, file_path);
                    }
                }

                if (materialName != null)
                {
                    MaterialProperty Ambient = texAmbient == null ?
                        new ConstMaterialProperty(valAmbient) : new TexturedMaterialProperty(texAmbient, valAmbient);

                    MaterialProperty Diffuse = texDiffuse == null ?
                        new ConstMaterialProperty(valDiffuse) : new TexturedMaterialProperty(texDiffuse, valDiffuse);

                    MaterialProperty Specullar = texSpecullar == null ?
                        new ConstMaterialProperty(valSpecullar) : new TexturedMaterialProperty(texSpecullar, valSpecullar);

                    NormalMap? normalMap = texNormal == null ? null : new(texNormal);

                    materials[materialName] = new Material(Ambient, Diffuse, Specullar, normalMap, SpecNs);
                }
            }
            return materials;
        }
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
        public static Model parse(string file_path)
        {
            List<Vector4> vertices = new List<Vector4>();
            List<Vector3> texture_uv = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Triangle> triangles = new List<Triangle>();
            Material currentMaterial = new();
            
            Dictionary<string, Material> materials = new();

            if(!System.IO.File.Exists(file_path))
                return new Model(vertices, texture_uv, normals, triangles);

            using (var reader = new StreamReader(stream: new FileStream(file_path, FileMode.Open)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();

                    if (line.StartsWith("mtllib "))
                    {
                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

                        string? path = System.IO.Path.GetDirectoryName(file_path);
                        if(path != null)
                        {
                            path += "\\" + tokens[1];
                            Dictionary<string, Material> newMaterials = ParseMaterials(path);
                            foreach(var matName in newMaterials.Keys)
                            {
                                materials[matName] = newMaterials[matName];
                            }
                        }
                    }
                    else if (line.StartsWith("v "))
                    {
                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
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
                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
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
                        normals.Add(Vector3.Normalize(ParseVectorLine(line)));
                    }
                    else if (line.StartsWith("usemtl "))
                    {
                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

                        string materialName = tokens[1];
                        if(materials.ContainsKey(materialName))
                            currentMaterial = materials[materialName];
                    }
                    else if (line.StartsWith("f "))
                    {
                        string[] tokens = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

                        PrimitiveType type = tokens.Length == 4 ? PrimitiveType.triangles : PrimitiveType.quads;

                        VertexIndexes[] vertexIndexes = new VertexIndexes[tokens.Length - 1];
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            var str_indices = tokens[i].Split('/');

                            vertexIndexes[i- 1] = new VertexIndexes(int.Parse(str_indices[0]));
                            if (str_indices.Length > 1 && str_indices[1].Length != 0)
                            {
                                vertexIndexes[i - 1].t_i = int.Parse(str_indices[1]);
                            }
                            if(str_indices.Length > 2 && str_indices[2].Length != 0)
                            {
                                vertexIndexes[i - 1].n_i = int.Parse(str_indices[2]);
                            }
                        }

                        if (type == PrimitiveType.triangles)
                        {
                            triangles.Add(new(currentMaterial, vertexIndexes));
                        }
                        else if (type == PrimitiveType.quads)
                        {
                            VertexIndexes[] firstTriangle = new VertexIndexes[3] 
                            {
                                vertexIndexes[0], vertexIndexes[1], vertexIndexes[2]
                            };
                            VertexIndexes[] secondTriangle = new VertexIndexes[3]
                            {
                                vertexIndexes[2], vertexIndexes[3], vertexIndexes[0]
                            };
                            triangles.Add(new(currentMaterial, firstTriangle));
                            triangles.Add(new(currentMaterial, secondTriangle));
                        }
                    }
                }
            }

            return new Model(vertices, texture_uv, normals, triangles);
        }
    }
}
