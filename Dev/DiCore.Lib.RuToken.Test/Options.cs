using CommandLine;
using CommandLine.Text;

namespace DiCore.Lib.RuToken.Test
{
    class Options
    {
        [Option('e', "enum", HelpText = "Enum all rutoken keys.")]
        public bool EnumKeys { get; set; }

        [Option('i', "index", DefaultValue = -1, HelpText = "Rutoken key index")]
        public int KeyIndex { get; set; }

        [Option('k', "key", HelpText = "Read Rutoken key")]
        public bool ReadKey { get; set; }

        [Option('u', "unincut", HelpText = "Read Rutoken key unincut")]
        public bool UnIncutGetKey { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
