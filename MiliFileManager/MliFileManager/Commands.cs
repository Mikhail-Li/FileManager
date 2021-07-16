using System;

namespace MiLiFileManager
{
    [Serializable]
    public class Commands
    {
        public string Command { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Mode { get; set; }
        public int Value { get; set; }
        public string Syntax { get; set; }
        public string Description { get; set; }
        public Commands() { }
    }
}
