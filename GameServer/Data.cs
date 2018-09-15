using RoyT.AStar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TricksterMap;

namespace TrickEmu2
{
    public class XmlStore
    {
        public static Dictionary<string, XElement> Xml { get; set; } = new Dictionary<string, XElement>();
    }

    public class CharacterInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class NpcTalkEntry
    {
        public ushort EntityId { get; set; }
        public int Type { get; set; }
        public int PosId { get; set; }
        public string MsgTableFilename { get; set; }
        public int HeadMarkType { get; set; }
    }

    public class Data
    {
        public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
        public static XmlStore XmlStore = new XmlStore();
        public static Dictionary<int, CharacterInfo> CharacterInfo = new Dictionary<int, CharacterInfo>();

        public static void LoadMaps()
        {
            var xml = XElement.Load(@"E:\Trickster\kTO-2014\lifeto-xml\MapInfoEx.xml");
            foreach (var map in xml.Elements("ROW"))
            {
                var id = int.Parse(map.Element("ID").Value);
                var name = map.Element("Name").Value;
                var fileName = map.Element("FileName").Value;
                var npcTable = map.Element("NpcTable").Value.Trim();

                if (id < 1)
                {
                    continue;
                }

                Program.logger.Info($"Load Map - {fileName}");

                var data = MapDataLoader.Load(new BinaryReader(File.Open(@"E:\Trickster\kTO-2014\" + fileName, FileMode.Open)));

                var collision = data.ConfigLayers.Where(x => x.Type == 1).FirstOrDefault();

                if (collision == null)
                {
                    Program.logger.Error($"Collision data not found for map {fileName} / {name} ({id})");
                }
                else
                {
                    Maps[id] = new Map()
                    {
                        Id = id,
                        Name = name,
                        FileName = fileName,
                        Width = data.MapSizeX,
                        Height = data.MapSizeY,
                        NpcTableFilename = npcTable,
                        Collision = collision,
                        PointObjects = data.PointObjects,
                        RangeObjects = data.RangeObjects
                    };

                    Program.logger.Info($"{name} ({id})  Size: {data.MapSizeX}x{data.MapSizeY}  Collision Size: {collision.X}x{collision.Y}");
                    
                    var grid = new Grid(collision.X, collision.Y, 1.0f);
                    var iTotal = 0;

                    for (int iY = 0; iY < collision.Y; iY++)
                    {
                        for (int iX = 0; iX < collision.X; iX++)
                        {
                            if (collision.Data[iTotal] != 0x00)
                            {
                                grid.BlockCell(new Position(iX, iY));
                            }
                            iTotal++;
                        }
                    }

                    Maps[id].Grid = grid;
                }
                
                collision = null;

                data = null;
            }

            xml = null;
        }

        public static void BuildMaps()
        {
            foreach (var map in Maps)
            {
                if (map.Key > 290 || map.Value.NpcTableFilename == "")
                {
                    continue;
                }

                try
                {
                    var xml = XElement.Load(@"E:\Trickster\kTO-2014\xml\" + map.Value.NpcTableFilename + ".xml");
                    
                    foreach (var entry in xml.Elements("ROW"))
                    {
                        map.Value.NpcTalk.Add(new NpcTalkEntry()
                        {
                            EntityId = ++Program.EntityId,
                            Type = int.Parse(entry.Element("Type").Value),
                            MsgTableFilename = entry.Element("MsgTable").Value,
                            PosId = int.Parse(entry.Element("PosId").Value),
                            HeadMarkType = int.Parse(entry.Element("HeadMarkType").Value)
                        });
                    }

                    Program.logger.Debug("-> {0} : {1} entries", map.Value.NpcTableFilename, map.Value.NpcTalk.Count);

                    xml = null;
                }
                catch (FileNotFoundException ex)
                {
                    Program.logger.Error(ex, "NPC message table not found: {0}", map.Value.NpcTableFilename);

                    if (Debugger.IsAttached) throw ex;
                }
            }
        }

        public static void LoadCharacterInfo()
        {
            var xml = XElement.Load(@"E:\Trickster\kTO-2014\xml\CharacterInfo.xml");

            foreach (var map in xml.Elements("ROW"))
            {
                var id = int.Parse(map.Element("ID").Value);
                var name = map.Element("CommonName").Value;

                CharacterInfo.Add(id, new CharacterInfo()
                {
                    Id = id,
                    Name = name
                });
            }

            xml = null;
        }
    }
}
