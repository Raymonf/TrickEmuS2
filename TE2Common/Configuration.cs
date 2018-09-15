using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TE2Common
{
    public class Configuration
    {
        public Dictionary<string, string> Server = new Dictionary<string, string>();
        public Dictionary<string, string> DB = new Dictionary<string, string>();
        private IniData data = null;

        public Configuration()
        {
            DB["Host"] = "127.0.0.1";
            DB["Database"] = "trickemu";
            DB["Username"] = "root";
            DB["Password"] = "root";
            Server["LoginPort"] = "9980";

            Server["ChannelIP"] = "127.0.0.1";
            Server["ChannelPort"] = "10006";

            Server["GameIP"] = "127.0.0.1";
            Server["GamePort"] = "22006";

            Server["SystemIP"] = "127.0.0.1";
            Server["SystemPort"] = "13336";
            Server["SystemEnabled"] = "0";

            var parser = new FileIniDataParser();

            try
            {
                data = parser.ReadFile("config.ini");

                DB["Host"] = data["DB"]["Host"];
                DB["Database"] = data["DB"]["Database"];
                DB["Username"] = data["DB"]["Username"];
                DB["Password"] = data["DB"]["Password"];

                Server["LoginPort"] = data["Login"]["Port"];

                Server["ChannelIP"] = data["Channel"]["IP"];
                Server["ChannelPort"] = data["Channel"]["Port"];

                Server["GameIP"] = data["Game"]["IP"];
                Server["GamePort"] = data["Game"]["Port"];

                Server["SystemIP"] = data["System"]["IP"];

                try
                {
                    // validate port
                    Server["SystemPort"] = ushort.Parse(data["System"]["Port"]).ToString();
                }
                catch { }

                Server["SystemEnabled"] = data["System"]["Enabled"];
            }
            catch
            {
                data = new IniData();
                data.Sections.AddSection("DB");

                data["DB"]["Host"] = "127.0.0.1";
                data["DB"]["Database"] = "trickemu";
                data["DB"]["Username"] = "root";
                data["DB"]["Password"] = "root";

                data.Sections.AddSection("Login");
                data["Login"]["Port"] = "9980";

                data.Sections.AddSection("Channel");
                data["Channel"]["IP"] = "127.0.0.1";
                data["Channel"]["Port"] = "10006";

                data.Sections.AddSection("Game");
                data["Game"]["IP"] = "127.0.0.1";
                data["Game"]["Port"] = "22006";

                data.Sections.AddSection("System");
                data["System"]["IP"] = "127.0.0.1";
                data["System"]["Port"] = "13336";
                data["System"]["Enabled"] = "0";

                parser.WriteFile("config.ini", data);
            }
        }
    }
}
